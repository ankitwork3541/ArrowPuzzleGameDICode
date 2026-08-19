using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private Camera cam;

    [Header("Framing — constant cell size across every level")]
    // How many cells span the screen's WIDTH at rest. Width, not height: a portrait board is
    // width-constrained, and this keeps a cell the same PIXEL size on every aspect ratio.
    // The old height-based constant gave 18 cells across on a 16:9 phone but only 14 on a 20:9
    // one, so tall screens silently cramped every board and forced it to open zoomed out.
    public float cellsVisibleHorizontally = 18f;


    [Header("Zoom range — multiples of this level's resting framing")]
    // Absolute zoom limits could not work: a level's resting zoom ranges from ~7 (Level01) to ~49
    // (Level11), so one fixed pair meant roughly 5x zoom-IN and almost no zoom-OUT on every board.
    // Relative limits give both directions real room whatever the level's size.
    public float zoomInFactor = 2.5f;

    // Zoom-OUT is capped by the CONTENT, not by a multiplier. A fixed multiplier is inconsistent by
    // construction: 1.7x of resting made a ~20x20 board fully visible while a 40x50 one stayed
    // partly hidden, so the same gesture meant different things on different levels. This game's
    // difficulty is visual search, so seeing the whole board is the one thing zoom must never give.
    [Range(0.3f, 1f)] public float maxVisibleContentFraction = 0.75f;

    // Small boards fit at rest, so a content cap would leave them with no zoom-out at all. Allow a
    // little, purely so the gesture does not feel dead.
    public float minZoomOutFactor = 1.15f;

    [Header("Input feel")]
    // Android's own touch slop is 8dp. A raw pixel count is ~4dp on a 3x screen, which let drags
    // steal taps. Screen.dpi is unreliable on Android (Unity's own issue tracker), so this scales
    // off the screen's short side instead — ~2% lands near 8dp across real phones.
    public float dragThresholdPixels = 12f;
    public float dragThresholdScreenFraction = 0.02f;

    public float mouseZoomSpeed = 4f;       // editor ma scroll-wheel thi zoom testing mate

    // Flick deceleration, as `v *= exp(-friction * dt)`. Apple's documented UIScrollView rates
    // convert to roughly friction 2.0 ("normal") and 10.0 ("fast") in this form — the old 6 sat
    // near "fast", so momentum died almost immediately and the board read as sticky.
    public float panFriction = 2.5f;
    [Range(0f, 1f)] public float velocitySmoothing = 0.35f;   // one noisy frame must not launch the board

    // Content ni kinar thi ketlu aagal pan karva devu (view size na proportion ma).
    // Aa vagar level screen ni kinar sudhi jam thayelu lage — reference app ma scroll karo
    // tyare shape ni farate khali jagya dekhay che, e feel aa thi aave che.
    public float panPadding = 0.35f;

    // Past the padding the board resists elastically instead of hitting a wall, then springs back.
    // Apple's rubber-band curve, f(x) = (x*d*c)/(d + c*x) with c = 0.55, which asymptotes: pulling a
    // whole viewport past the edge still only shows ~0.355 of one.
    public float edgeSpringSpeed = 12f;

    [Header("Level intro")]
    // The board opens fully visible, then the camera closes in to the playing framing. Measured off
    // the reference game: 2.17s, and near-linear — the board's height grew at a steady ~0.33 px/ms
    // with no ease-out. This is also what makes capping zoom-out fair: the player is GIVEN the
    // overview once, as an animation, instead of being able to pull it up whenever they want.
    public float introDuration = 2.0f;

    // Breathing room around the board at the widest point of the move.
    //
    // There is deliberately NO minimum-move floor: the intro plays only on boards that actually
    // overflow the screen, which is what the reference game does — a level small enough to fit gets
    // no opening move at all, because there is nothing the player cannot already see.
    public float introFitPadding = 1.18f;

    // How much wider the overview has to be than the resting framing before the move is worth
    // playing. A board that overflows by a few percent would otherwise spend two seconds animating
    // a 5% change, which reads as a stutter on load rather than a reveal.
    public float introMinimumGain = 1.15f;

    [Header("Hint focus")]
    public float focusDuration = 0.45f;
    public float focusCellsVisible = 12f;   // how tight the hint view gets, in cells

    private float minZoom, maxZoom, restZoom;

    // The un-resisted camera XY. transform.position is DERIVED from this every frame by
    // ApplyPan(), which bends it back near the edges. Keeping the raw value separate is what makes
    // the elastic edge stable — resisting an already-resisted position compounds every frame.
    private Vector2 panTarget;

    private Vector2 panVelocity;
    private Bounds contentBounds;
    private bool hasContent;
    private Tween focusTween;

    private Tween introTween;
    private bool introPlaying;

    // Gesture state
    private bool pointerDown;
    private Vector2 pointerDownScreenPos;
    private Vector3 dragAnchorWorld;
    private bool gestureWasDrag;
    // Decided once, when the finger goes down. Judging it at release instead would let a drag that
    // STARTED on the HUD still pan the board, and a finger sliding off a button onto the board
    // would count as a board tap.
    private bool gestureStartedOnUI;
    private bool tapThisFrame;
    private Vector2 tapScreenPos;
    private float pinchPrevDistance;

    // The framing the scene was authored with. Menus are drawn over the same world the board
    // lives in, and a level leaves the camera somewhere else entirely, so anything world-space
    // behind them only lines up while the camera sits where it started.
    private Vector3 menuPosition;
    private float menuZoom;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();

        menuPosition = transform.position;
        menuZoom = cam.orthographicSize;
    }

    // Put the camera back where the scene started. Called whenever gameplay ends, because
    // FrameLevel() moves and zooms the camera to the board and nothing else ever undoes that:
    // returning to Home left the menu looking at whatever framing the last level happened to
    // need. (BackdropArrows no longer cares — it hangs off the camera and refits itself — but
    // the reset still has to happen for the reason below.)
    //
    // Clearing hasContent matters just as much: Update() gates all pan/zoom input on it, so
    // leaving it set let the player drag and pinch the camera around on the menu screens.
    public void FrameMenu()
    {
        focusTween.Stop();
        introTween.Stop();
        introPlaying = false;
        hasContent = false;
        panVelocity = Vector2.zero;

        transform.position = menuPosition;
        panTarget = new Vector2(menuPosition.x, menuPosition.y);
        cam.orthographicSize = menuZoom;
    }

    // Freeze board input without moving the camera. The Win and Lose cards are scrims over the
    // level that was just played, so the board has to stay exactly where it is AND stop responding
    // — otherwise the player can still drag, pinch and tap arrows through the dimmed overlay.
    // FrameMenu() cannot be used for this: it would snap the camera back to the menu framing and
    // throw away the very view the card is sitting on top of.
    //
    // No matching Resume(): every route off a result card either loads a level (FrameLevel sets
    // hasContent again) or goes Home (FrameMenu, which wants it off anyway).
    public void SuspendInput()
    {
        hasContent = false;
        panVelocity = Vector2.zero;   // or a fling in progress keeps coasting under the card
    }

    // bounds = ACTUAL rendered content ni bounding box (LineRenderer + head/tail sprites saathe).
    public void FrameLevel(Bounds bounds, float spacing)
    {
        contentBounds = bounds;
        hasContent = true;
        panVelocity = Vector2.zero;

        // orthographicSize is HALF the world height, and world width = 2 * size * aspect, so
        // "N cells across" is size = N * spacing / (2 * aspect).
        //
        // The resting framing is ONLY this — a cell is the same size on every level, full stop.
        // It used to be Min(Max(defaultZoom, fitZoom), capZoom), i.e. zoom out until the board
        // fits. That quietly contradicted the constant-cell-size rule this header claims: a 20x20
        // board opened at 17.5pt per cell against 21.8pt on a small one, AND it was why a 20x20
        // was entirely visible before the player touched anything. A board bigger than the view
        // now simply overflows and is panned, which is what the reference game does — the opening
        // intro is what gives the player their one look at the whole thing.
        restZoom = cellsVisibleHorizontally * spacing / (2f * cam.aspect);

        minZoom = restZoom / zoomInFactor;

        // The visible region may never exceed maxVisibleContentFraction of the board on either
        // axis. On a board that already fits at rest that cap is tighter than resting, so the
        // minZoomOutFactor floor keeps the gesture alive there.
        float contentCapY = 0.5f * bounds.size.y * maxVisibleContentFraction;
        float contentCapX = 0.5f * bounds.size.x * maxVisibleContentFraction / cam.aspect;
        maxZoom = Mathf.Max(restZoom * minZoomOutFactor, Mathf.Min(contentCapY, contentCapX));

        cam.orthographicSize = restZoom;
        panTarget = new Vector2(bounds.center.x, bounds.center.y);
        ApplyPan();
    }

    // Open the level fully visible and close in to the playing framing.
    //
    // Starts at max(introZoom, restZoom): on a board that already fits at rest there is nothing to
    // reveal, so the intro collapses to a no-op rather than zooming OUT and then back in.
    public void PlayIntro()
    {
        if (!hasContent) return;

        introTween.Stop();

        float fitY = contentBounds.extents.y;
        float fitX = contentBounds.extents.x / cam.aspect;
        float from = Mathf.Max(fitY, fitX) * introFitPadding;

        // Board already fits, or barely overflows — nothing worth revealing, so no move at all.
        if (from <= restZoom * introMinimumGain) return;

        float to = restZoom;

        introPlaying = true;
        cam.orthographicSize = from;
        panTarget = new Vector2(contentBounds.center.x, contentBounds.center.y);
        ApplyPan();

        // Interpolate 1/size, not size.
        //
        // On-screen scale is proportional to 1/orthographicSize, so a linear tween of SIZE grows the
        // board slowly at first and then accelerates into the finish — which is exactly why the move
        // read as abrupt. Measured on the reference, the board's pixel height grew at a steady rate
        // throughout, and constant apparent growth IS linear in the reciprocal. Same endpoints, and
        // the arrival decelerates on its own without an ease having to fake it.
        float invFrom = 1f / from;
        float invTo = 1f / to;

        introTween = Tween.Custom(this, invFrom, invTo, duration: introDuration, ease: Ease.Linear,
            onValueChange: (controller, inv) =>
            {
                controller.cam.orthographicSize = 1f / inv;
                controller.ApplyPan();
            })
            .OnComplete(this, warnIfTargetDestroyed: false, onComplete: controller =>
            {
                controller.cam.orthographicSize = controller.restZoom;
                controller.introPlaying = false;
                controller.ApplyPan();
            });
    }

    // Hint: glide the camera onto one arrow so the player can see which one to tap.
    // Zooming IN is allowed here, unlike FrameLevel's zoom-out-only rule — this is a
    // deliberate, temporary look at one arrow, not the level's resting framing.
    public void FocusOn(Bounds bounds, float spacing)
    {
        if (!hasContent) return;

        focusTween.Stop();
        panVelocity = Vector2.zero;   // a leftover glide would fight the hint's camera move

        // On a level that already fits on screen everything is visible anyway, so a zoom-in
        // would be pure noise — pan only. Big boards are the ones that actually need it.
        bool contentFits = contentBounds.size.y <= cam.orthographicSize * 2f
                        && contentBounds.size.x <= cam.orthographicSize * 2f * cam.aspect;

        float startSize = cam.orthographicSize;
        float targetSize = contentFits
            ? startSize
            : Mathf.Clamp(focusCellsVisible * spacing / 2f, minZoom, maxZoom);

        Vector3 startPos = transform.position;
        Vector3 targetPos = new Vector3(bounds.center.x, bounds.center.y, startPos.z);

        focusTween = Tween.Custom(this, 0f, 1f, duration: focusDuration, ease: Ease.OutCubic,
            onValueChange: (controller, t) =>
            {
                controller.cam.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
                Vector3 glide = Vector3.Lerp(startPos, targetPos, t);
                controller.panTarget = new Vector2(glide.x, glide.y);
                controller.ApplyPan();        // never glide past the board's pan limits
            });
    }

    void Update()
    {
        tapThisFrame = false;
        if (!hasContent) return;

        // The opening camera move owns the view — a pinch or drag mid-intro fights the tween and
        // leaves the board at a framing the level never chose.
        if (introPlaying) return;

        if (HandlePinch()) return; // be aangdi vaparai rahi che — pan/tap skip
        HandleMouseWheelZoom();
        HandleDragAndTap();
        ApplyPanInertia();
        SpringBackToBounds();
    }

    // Scaled off the screen rather than a raw pixel count, so the same gesture reads the same on a
    // 1080p phone and a tablet.
    float DragThreshold()
    {
        float shortSide = Mathf.Min(Screen.width, Screen.height);
        return Mathf.Max(dragThresholdPixels, shortSide * dragThresholdScreenFraction);
    }

    // ---------- ZOOM ----------

    bool HandlePinch()
    {
        Touchscreen touch = Touchscreen.current;
        if (touch == null) return false;

        bool twoDown = touch.touches.Count >= 2
            && touch.touches[0].press.isPressed
            && touch.touches[1].press.isPressed;

        if (!twoDown)
        {
            pinchPrevDistance = 0f;
            return false;
        }

        // Be aangdi aavi etle chalu drag/tap cancel karo
        pointerDown = false;
        gestureWasDrag = true;
        panVelocity = Vector2.zero;

        Vector2 a = touch.touches[0].position.ReadValue();
        Vector2 b = touch.touches[1].position.ReadValue();
        float distance = Vector2.Distance(a, b);

        if (pinchPrevDistance > 0f && distance > 0f)
        {
            // Aangdio door thay (distance vadhe) = zoom IN = orthographicSize ghate.
            // Anchored on the point BETWEEN the fingers, not the screen centre — zooming towards
            // the centre pulls whatever you were looking at away from under your hands, which is
            // most of why the old zoom felt wrong.
            ApplyZoomAt(cam.orthographicSize * (pinchPrevDistance / distance), (a + b) * 0.5f);
        }

        pinchPrevDistance = distance;
        return true;
    }

    void HandleMouseWheelZoom()
    {
        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Approximately(scroll, 0f)) return;

        ApplyZoomAt(cam.orthographicSize - scroll * 0.01f * mouseZoomSpeed, mouse.position.ReadValue());
    }

    // Zoom while keeping the world point under `screenFocus` pinned to that same screen point.
    void ApplyZoomAt(float newSize, Vector2 screenFocus)
    {
        Vector3 before = cam.ScreenToWorldPoint(screenFocus);
        cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        Vector3 after = cam.ScreenToWorldPoint(screenFocus);

        Vector3 correction = before - after;
        panTarget += new Vector2(correction.x, correction.y);
        ApplyPan();
    }

    // ---------- PAN + TAP ----------

    void HandleDragAndTap()
    {
        Vector2 screenPos;
        bool pressed = TryGetPointer(out screenPos);

        if (pressed && !pointerDown)
        {
            pointerDown = true;
            gestureWasDrag = false;
            gestureStartedOnUI = GameplayHUDController.Instance != null
                && GameplayHUDController.Instance.IsPointerOverUI(screenPos);
            pointerDownScreenPos = screenPos;
            dragAnchorWorld = cam.ScreenToWorldPoint(screenPos);
            return;
        }

        if (pressed && pointerDown)
        {
            if (!gestureWasDrag
                && Vector2.Distance(screenPos, pointerDownScreenPos) > DragThreshold())
                gestureWasDrag = true;

            if (gestureWasDrag && !gestureStartedOnUI)
            {
                // Aangdi niche je world-point hato e j aangdi niche rahe — "grab and move" feel.
                // Deliberately 1:1 and unsmoothed: any damping here would let the board lag behind
                // the finger, which reads as sluggish, not smooth. Smoothing belongs after release.
                Vector3 posBefore = transform.position;

                Vector3 current = cam.ScreenToWorldPoint(screenPos);
                Vector3 delta = dragAnchorWorld - current;
                panTarget += new Vector2(delta.x, delta.y);
                ApplyPan();

                // Measured from the movement that actually survived clamping, so a drag pressed
                // against an edge builds no velocity to fling with when it is released.
                if (Time.deltaTime > 0f)
                {
                    Vector2 moved = transform.position - posBefore;
                    panVelocity = Vector2.Lerp(panVelocity, moved / Time.deltaTime, velocitySmoothing);
                }
            }
            return;
        }

        if (!pressed && pointerDown)
        {
            pointerDown = false;

            if (gestureStartedOnUI) panVelocity = Vector2.zero;

            if (!gestureWasDrag && !gestureStartedOnUI)
            {
                tapThisFrame = true;
                tapScreenPos = screenPos;
                panVelocity = Vector2.zero;   // a tap is not a flick
            }
        }
    }

    // Kinetic glide after a flick. Runs only while nothing is touching the screen.
    void ApplyPanInertia()
    {
        if (pointerDown || panVelocity.sqrMagnitude < 0.0001f) return;

        Vector3 posBefore = transform.position;
        panTarget += panVelocity * Time.deltaTime;
        ApplyPan();

        // Once the elastic edge has compressed the glide to nothing there is no point carrying the
        // velocity further — SpringBackToBounds() is what actually returns the board from here.
        Vector3 moved = transform.position - posBefore;
        if (Mathf.Abs(moved.x) < 0.0001f) panVelocity.x = 0f;
        if (Mathf.Abs(moved.y) < 0.0001f) panVelocity.y = 0f;

        // Exponential decay is framerate-independent, unlike multiplying by a constant per frame.
        panVelocity *= Mathf.Exp(-panFriction * Time.deltaTime);
        if (panVelocity.sqrMagnitude < 0.01f) panVelocity = Vector2.zero;
    }

    bool TryGetPointer(out Vector2 screenPos)
    {
        Touchscreen touch = Touchscreen.current;
        if (touch != null && touch.touches.Count > 0 && touch.touches[0].press.isPressed)
        {
            screenPos = touch.touches[0].position.ReadValue();
            return true;
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed)
        {
            screenPos = mouse.position.ReadValue();
            return true;
        }

        // release thayu tyare pan chhelli position joiye che
        screenPos = mouse != null ? mouse.position.ReadValue() : Vector2.zero;
        if (touch != null && touch.touches.Count > 0)
            screenPos = touch.touches[0].position.ReadValue();
        return false;
    }

    // The free pan range, padding included. min > max means the content is narrower than the view
    // on that axis, so there is nothing to pan and the board simply centres.
    void GetPanRange(out Vector2 min, out Vector2 max, out Vector2 viewSize)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        viewSize = new Vector2(halfWidth * 2f, halfHeight * 2f);

        // Kinar thi thodu aagal javani chhut, jethi shape ni farate khali jagya dekhay.
        float marginX = halfWidth * panPadding;
        float marginY = halfHeight * panPadding;

        min = new Vector2(contentBounds.min.x + halfWidth - marginX,
                          contentBounds.min.y + halfHeight - marginY);
        max = new Vector2(contentBounds.max.x - halfWidth + marginX,
                          contentBounds.max.y - halfHeight + marginY);
    }

    // Apple's rubber-band curve. Asymptotes towards `dimension`, so however hard the board is
    // dragged past its edge the visible overshoot stays bounded and always looks intentional.
    static float RubberBand(float overshoot, float dimension)
    {
        const float c = 0.55f;
        return (overshoot * dimension * c) / (dimension + c * overshoot);
    }

    // Derive transform.position from panTarget, bending it back past the edges instead of stopping
    // dead against them. A hard clamp is what made the board feel bolted down.
    void ApplyPan()
    {
        if (!hasContent) return;

        Vector2 min, max, viewSize;
        GetPanRange(out min, out max, out viewSize);

        Vector3 p = transform.position;
        p.x = ResistAxis(panTarget.x, min.x, max.x, contentBounds.center.x, viewSize.x);
        p.y = ResistAxis(panTarget.y, min.y, max.y, contentBounds.center.y, viewSize.y);
        transform.position = p;
    }

    static float ResistAxis(float target, float min, float max, float centre, float viewSize)
    {
        if (min > max) return centre;
        if (target < min) return min - RubberBand(min - target, viewSize);
        if (target > max) return max + RubberBand(target - max, viewSize);
        return target;
    }

    // Pull panTarget back into range once nothing is holding the board out there.
    void SpringBackToBounds()
    {
        if (!hasContent || pointerDown) return;

        Vector2 min, max, viewSize;
        GetPanRange(out min, out max, out viewSize);

        Vector2 settled = new Vector2(
            min.x > max.x ? contentBounds.center.x : Mathf.Clamp(panTarget.x, min.x, max.x),
            min.y > max.y ? contentBounds.center.y : Mathf.Clamp(panTarget.y, min.y, max.y));

        if (Mathf.Approximately(settled.x, panTarget.x) && Mathf.Approximately(settled.y, panTarget.y))
            return;

        // Momentum must not fight the spring, or the board buzzes at the edge.
        if (!Mathf.Approximately(settled.x, panTarget.x)) panVelocity.x = 0f;
        if (!Mathf.Approximately(settled.y, panTarget.y)) panVelocity.y = 0f;

        float t = 1f - Mathf.Exp(-edgeSpringSpeed * Time.deltaTime);
        panTarget += (settled - panTarget) * t;
        ApplyPan();
    }

    // Arrows aa vaapre che: khali SACHO tap (drag/pinch nahi) j arrow ne move karave.
    // Input ek j jagyaae vanchay che — dar arrow potani rite nahi vanchtu (vadhu efficient).
    public bool TryGetTapThisFrame(out Vector2 screenPos)
    {
        screenPos = tapScreenPos;
        return tapThisFrame;
    }
}

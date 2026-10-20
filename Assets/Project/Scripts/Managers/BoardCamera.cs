using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

public class BoardCamera : MonoBehaviour
{
    public static BoardCamera Instance;

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

    // Longest a finger may stay down and still count as a tap. Generous on purpose — this exists to
    // reject a resting finger, not to hurry a deliberate one. Raise it if a slow tap ever misses.
    public float maxTapSeconds = 1f;

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

    // Which contact the pan is following. Tracked by id, not by index: a finger's slot in the
    // touch list changes as other fingers land and lift, so "the first touch" is a different
    // finger from one frame to the next.
    private int pointerFingerId = NoFinger;

    // Set the moment a second finger lands, and cleared ONLY once the screen is completely empty
    // — Android's ACTION_POINTER_DOWN rule. Clearing it when the count merely drops back to one is
    // what let every pinch end as a tap: the finger still down would open a fresh gesture the
    // instant its partner lifted, and that gesture had nothing left to say it came from a pinch.
    private bool gestureCancelled;

    private float gestureStartTime;

    // The last position the live pointer reported. A lifted finger is gone by the time the release
    // is handled, so the press-to-release distance has to be measured against this.
    private Vector2 lastPointerScreenPos;

    private const int NoFinger = -1;
    private const int MouseFinger = -2;

    // The framing the scene was authored with. Menus are drawn over the same world the board
    // lives in, and a level leaves the camera somewhere else entirely, so anything world-space
    // behind them only lines up while the camera sits where it started.
    private Vector3 menuPosition;
    private float menuZoom;

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();

        // activeTouches is empty unless this is on. It is the only list that holds real contacts:
        // Touchscreen.touches is a fixed-size array of slots whose length says nothing about how
        // many fingers are actually down, and whose indices are recycled between gestures.
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();

        menuPosition = transform.position;
        menuZoom = cam.orthographicSize;
    }

    void OnDestroy()
    {
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable();
    }

    // Put the camera back where the scene started. Called whenever gameplay ends, because
    // FrameLevel() moves and zooms the camera to the board and nothing else ever undoes that:
    // returning to Home left the menu looking at whatever framing the last level happened to
    // need. (LanternBackdrop no longer cares — it hangs off the camera and refits itself — but
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
        panTarget = PanCentre();
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

        // The one look the player gets at the whole board has to fit it into the strip the HUD is
        // NOT covering — fitting it to the full screen just hides the top and bottom rows behind
        // the bars, which is the opposite of what this move exists for. Only Y is divided down:
        // nothing covers the sides.
        float top, bottom;
        GetHudInsetFractions(out top, out bottom);
        float visibleFraction = Mathf.Max(0.2f, 1f - top - bottom);

        float fitY = contentBounds.extents.y / visibleFraction;
        float fitX = contentBounds.extents.x / cam.aspect;
        float from = Mathf.Max(fitY, fitX) * introFitPadding;

        // Board already fits, or barely overflows — nothing worth revealing, so no move at all.
        if (from <= restZoom * introMinimumGain) return;

        float to = restZoom;

        introPlaying = true;
        cam.orthographicSize = from;
        panTarget = PanCentre();
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

                // Re-centred every frame, not once up front: the HUD's inset is a fixed slice of
                // the SCREEN, so its world size shrinks along with the zoom. Holding one centre
                // would let the board drift against the bars as the move plays.
                controller.panTarget = controller.PanCentre();
                controller.ApplyPan();
            })
            .OnComplete(this, warnIfTargetDestroyed: false, onComplete: controller =>
            {
                controller.cam.orthographicSize = controller.restZoom;
                controller.introPlaying = false;
                controller.panTarget = controller.PanCentre();
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

        UpdateGestureCancel();     // finger count pehla joi lo — pinch ne tap banta atkave che
        if (HandlePinch()) return; // be aangdi vaparai rahi che — pan/tap skip
        HandleMouseWheelZoom();
        HandleDragAndTap();
        ApplyPanInertia();
        SpringBackToBounds();
    }

    // A gesture that ever had two fingers in it can never produce a tap, however it ends. The flag
    // outlives the second finger on purpose — it is only lifted once nothing is touching the glass.
    void UpdateGestureCancel()
    {
        int fingers = LiveTouchCount();

        if (fingers >= 2)
        {
            gestureCancelled = true;

            // Drop the single-finger gesture outright rather than letting the release path run:
            // this is a pinch now, and there is nothing left worth releasing.
            pointerDown = false;
            pointerFingerId = NoFinger;
            return;
        }

        // !pointerDown matters as much as the empty screen does. This runs BEFORE the release is
        // handled, so clearing the flag the moment the last finger lifts would hand that very
        // release a clean slate — and the pinch would end as a tap after all. Waiting for the
        // release to have been consumed costs one frame and closes the hole.
        if (fingers == 0 && !MousePressed() && !pointerDown)
        {
            gestureCancelled = false;
        }
    }

    // Scaled off the screen rather than a raw pixel count, so the same gesture reads the same on a
    // 1080p phone and a tablet.
    float DragThreshold()
    {
        float shortSide = Mathf.Min(Screen.width, Screen.height);
        return Mathf.Max(dragThresholdPixels, shortSide * dragThresholdScreenFraction);
    }

    // ---------- TOUCH SOURCE ----------

    // activeTouches keeps a contact for the one frame it lifts on, so "still on the glass" has to
    // be read from the phase — the list's length alone would report a finger that is already gone.
    static bool IsLive(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        UnityEngine.InputSystem.TouchPhase phase = touch.phase;
        return phase == UnityEngine.InputSystem.TouchPhase.Began
            || phase == UnityEngine.InputSystem.TouchPhase.Moved
            || phase == UnityEngine.InputSystem.TouchPhase.Stationary;
    }

    static int LiveTouchCount()
    {
        var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        int count = 0;

        for (int i = 0; i < touches.Count; i++)
        {
            if (IsLive(touches[i])) count++;
        }

        return count;
    }

    static bool MousePressed()
    {
        Mouse mouse = Mouse.current;
        return mouse != null && mouse.leftButton.isPressed;
    }

    // The first two contacts actually on the glass. Deliberately not touches[0] and touches[1] of
    // Touchscreen: those are slots, and two fingers can easily sit in slots 0 and 2 with slot 1
    // empty — in which case the old check decided there was no pinch and treated the whole thing
    // as a one-finger drag.
    static bool TryGetTwoLiveTouches(out Vector2 a, out Vector2 b)
    {
        a = Vector2.zero;
        b = Vector2.zero;

        var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
        int found = 0;

        for (int i = 0; i < touches.Count && found < 2; i++)
        {
            if (!IsLive(touches[i])) continue;

            if (found == 0) a = touches[i].screenPosition;
            else b = touches[i].screenPosition;

            found++;
        }

        return found == 2;
    }

    // ---------- ZOOM ----------

    bool HandlePinch()
    {
        Vector2 a, b;
        if (!TryGetTwoLiveTouches(out a, out b))
        {
            pinchPrevDistance = 0f;
            return false;
        }

        // The drag/tap cancel itself lives in UpdateGestureCancel(), which runs before this and
        // keeps holding once a finger lifts. Doing it here only worked for as long as BOTH fingers
        // stayed down.
        panVelocity = Vector2.zero;

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
        int fingerId;
        bool pressed = TryGetPointer(out screenPos, out fingerId);

        if (pressed) lastPointerScreenPos = screenPos;

        if (pressed && !pointerDown)
        {
            pointerDown = true;
            pointerFingerId = fingerId;
            gestureWasDrag = false;
            gestureStartTime = Time.unscaledTime;
            gestureStartedOnUI = PlayScreenController.Instance != null
                && PlayScreenController.Instance.IsPointerOverUI(screenPos);
            pointerDownScreenPos = screenPos;
            lastPointerScreenPos = screenPos;
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
            pointerFingerId = NoFinger;

            if (gestureStartedOnUI) panVelocity = Vector2.zero;

            // The slop is re-checked against where the finger actually LEFT, not only against the
            // frames in between. A quick flick can go down and up inside a frame or two and never
            // produce a mid-gesture sample to measure, which used to make it a tap.
            if (Vector2.Distance(lastPointerScreenPos, pointerDownScreenPos) > DragThreshold())
                gestureWasDrag = true;

            // A finger resting in one spot for a second is a hold, not a tap. Without this a slow,
            // careful nudge of the board that never travels a full slop still fires an arrow.
            bool withinTapTime = Time.unscaledTime - gestureStartTime <= maxTapSeconds;

            if (!gestureWasDrag && !gestureStartedOnUI && !gestureCancelled && withinTapTime)
            {
                tapThisFrame = true;
                tapScreenPos = lastPointerScreenPos;
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

    // Follows ONE contact for the whole gesture, matched by id. Reading "the first touch" instead
    // hands the pan to whichever finger currently sits at that index — which changes every time a
    // finger lands or lifts, and was why a released pinch could report a position belonging to a
    // completely different part of the screen.
    bool TryGetPointer(out Vector2 screenPos, out int fingerId)
    {
        var touches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;

        // Stay with the finger already being followed. The frame it lifts on it is still in the
        // list, just no longer live — and that entry carries the only record of where the gesture
        // actually ended, which is what the release needs to tell a flick from a tap.
        if (pointerFingerId != NoFinger && pointerFingerId != MouseFinger)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                if (touches[i].touchId != pointerFingerId) continue;

                screenPos = touches[i].screenPosition;

                if (IsLive(touches[i]))
                {
                    fingerId = touches[i].touchId;
                    return true;
                }

                lastPointerScreenPos = screenPos;
                fingerId = NoFinger;
                return false;
            }
        }

        // Otherwise adopt the first live contact — this is a new gesture.
        if (pointerFingerId == NoFinger)
        {
            for (int i = 0; i < touches.Count; i++)
            {
                if (!IsLive(touches[i])) continue;

                screenPos = touches[i].screenPosition;
                fingerId = touches[i].touchId;
                return true;
            }
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.isPressed)
        {
            screenPos = mouse.position.ReadValue();
            fingerId = MouseFinger;
            return true;
        }

        // Released. lastPointerScreenPos already holds where the pointer was, so nothing has to be
        // dug back out of a list the finger has just left.
        screenPos = lastPointerScreenPos;
        fingerId = NoFinger;
        return false;
    }

    // What fraction of the view's HEIGHT the HUD covers at each end. The board is drawn behind the
    // top panel and the bottom bar, so those are content insets in exactly the sense UIScrollView
    // means: the pannable range has to run past the board's edge by however much is covered, or the
    // edge rows can never be dragged out from under the bars.
    //
    // Fractions rather than world units because they are resolution-independent AND zoom-independent
    // — the bars are a fixed slice of the screen whatever the camera is doing.
    void GetHudInsetFractions(out float top, out float bottom)
    {
        top = 0f;
        bottom = 0f;

        PlayScreenController hud = PlayScreenController.Instance;
        if (hud == null || Screen.height <= 0) return;

        top = Mathf.Clamp01(hud.TopInsetPixels / Screen.height);
        bottom = Mathf.Clamp01(hud.BottomInsetPixels / Screen.height);
    }

    // The board's centre, biased so it lands in the middle of the strip the HUD is NOT covering
    // rather than the middle of the screen.
    Vector2 PanCentre()
    {
        float top, bottom;
        GetHudInsetFractions(out top, out bottom);

        return new Vector2(contentBounds.center.x,
                           contentBounds.center.y + (top - bottom) * cam.orthographicSize);
    }

    // The free pan range, padding and HUD insets included. min > max means the content is narrower
    // than the view on that axis, so there is nothing to pan and the board simply centres.
    void GetPanRange(out Vector2 min, out Vector2 max, out Vector2 viewSize, out Vector2 centre)
    {
        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        viewSize = new Vector2(halfWidth * 2f, halfHeight * 2f);
        centre = PanCentre();

        // Kinar thi thodu aagal javani chhut, jethi shape ni farate khali jagya dekhay.
        float marginX = halfWidth * panPadding;
        float marginY = halfHeight * panPadding;

        float topInset, bottomInset;
        GetHudInsetFractions(out topInset, out bottomInset);

        // Only the Y axis moves: nothing covers the left or right of the view, so widening those
        // would be slack for its own sake.
        float insetTopWorld = topInset * viewSize.y;
        float insetBottomWorld = bottomInset * viewSize.y;

        min = new Vector2(contentBounds.min.x + halfWidth - marginX,
                          contentBounds.min.y + halfHeight - marginY - insetBottomWorld);
        max = new Vector2(contentBounds.max.x - halfWidth + marginX,
                          contentBounds.max.y - halfHeight + marginY + insetTopWorld);
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

        Vector2 min, max, viewSize, centre;
        GetPanRange(out min, out max, out viewSize, out centre);

        Vector3 p = transform.position;
        p.x = ResistAxis(panTarget.x, min.x, max.x, centre.x, viewSize.x);
        p.y = ResistAxis(panTarget.y, min.y, max.y, centre.y, viewSize.y);
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

        Vector2 min, max, viewSize, centre;
        GetPanRange(out min, out max, out viewSize, out centre);

        Vector2 settled = new Vector2(
            min.x > max.x ? centre.x : Mathf.Clamp(panTarget.x, min.x, max.x),
            min.y > max.y ? centre.y : Mathf.Clamp(panTarget.y, min.y, max.y));

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

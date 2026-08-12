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

    // How far a level is allowed to open zoomed-out. A board bigger than this opens at this size
    // and has to be panned — it is not shrunk until it fits, which would make its cells tiny.
    public float maxCellsVisibleHorizontally = 26f;

    public float fitPadding = 1.12f; // content-fit ni upar thodi breathing room

    [Header("Zoom range — multiples of this level's resting framing")]
    // Absolute zoom limits could not work: a level's resting zoom ranges from ~7 (Level01) to ~49
    // (Level11), so one fixed pair meant roughly 5x zoom-IN and almost no zoom-OUT on every board.
    // Relative limits give both directions real room whatever the level's size.
    public float zoomInFactor = 2.5f;
    public float zoomOutFactor = 1.7f;

    [Header("Input feel")]
    public float dragThresholdPixels = 12f; // aa thi vadhu khasyu = drag (tap nahi)
    public float mouseZoomSpeed = 4f;       // editor ma scroll-wheel thi zoom testing mate

    // A flick keeps the board gliding and decelerating, instead of stopping dead the instant the
    // finger lifts. Higher friction stops sooner. There is deliberately no elastic overshoot at
    // the edges — the glide just stops there.
    public float panFriction = 6f;
    [Range(0f, 1f)] public float velocitySmoothing = 0.35f;   // one noisy frame must not launch the board

    // Content ni kinar thi ketlu aagal pan karva devu (view size na proportion ma).
    // Aa vagar level screen ni kinar sudhi jam thayelu lage — reference app ma scroll karo
    // tyare shape ni farate khali jagya dekhay che, e feel aa thi aave che.
    public float panPadding = 0.35f;

    [Header("Hint focus")]
    public float focusDuration = 0.45f;
    public float focusCellsVisible = 12f;   // how tight the hint view gets, in cells

    private float minZoom, maxZoom, restZoom;
    private Vector2 panVelocity;
    private Bounds contentBounds;
    private bool hasContent;
    private Tween focusTween;

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

    void Awake()
    {
        Instance = this;
        cam = GetComponent<Camera>();
    }

    // bounds = ACTUAL rendered content ni bounding box (LineRenderer + head/tail sprites saathe).
    // Rule: ZOOM-OUT ONLY. Nani level default (constant cell size) par j rahe — nani, centered,
    // farate khali jagya — jem reference app ma thay che. Khali moti level j zoom-out thay,
    // ane e pan maxCellsVisibleHorizontally sudhi j.
    public void FrameLevel(Bounds bounds, float spacing)
    {
        contentBounds = bounds;
        hasContent = true;
        panVelocity = Vector2.zero;

        // orthographicSize is HALF the world height, and world width = 2 * size * aspect, so
        // "N cells across" is size = N * spacing / (2 * aspect).
        float defaultZoom = cellsVisibleHorizontally * spacing / (2f * cam.aspect);
        float capZoom = maxCellsVisibleHorizontally * spacing / (2f * cam.aspect);

        float sizeForHeight = bounds.extents.y;
        float sizeForWidth = bounds.extents.x / cam.aspect;
        float fitZoom = Mathf.Max(sizeForHeight, sizeForWidth) * fitPadding;

        // Zoom-OUT only, then capped: a small level rests at the constant cell size with space
        // around it; a big one opens as far out as the cap allows and is panned from there.
        restZoom = Mathf.Min(Mathf.Max(defaultZoom, fitZoom), capZoom);

        minZoom = restZoom / zoomInFactor;
        // Whatever the resting framing, pinching out must always reach a view of the WHOLE board —
        // on a level too big to open in full, surveying it is exactly when zoom-out is needed most.
        maxZoom = Mathf.Max(restZoom * zoomOutFactor, fitZoom * 1.05f);

        cam.orthographicSize = restZoom;
        transform.position = new Vector3(bounds.center.x, bounds.center.y, transform.position.z);
        ClampPosition();
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
                controller.transform.position = Vector3.Lerp(startPos, targetPos, t);
                controller.ClampPosition();   // never glide past the board's pan limits
            });
    }

    void Update()
    {
        tapThisFrame = false;
        if (!hasContent) return;

        if (HandlePinch()) return; // be aangdi vaparai rahi che — pan/tap skip
        HandleMouseWheelZoom();
        HandleDragAndTap();
        ApplyPanInertia();
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
        transform.position += new Vector3(correction.x, correction.y, 0f);
        ClampPosition();
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
                && Vector2.Distance(screenPos, pointerDownScreenPos) > dragThresholdPixels)
                gestureWasDrag = true;

            if (gestureWasDrag && !gestureStartedOnUI)
            {
                // Aangdi niche je world-point hato e j aangdi niche rahe — "grab and move" feel.
                // Deliberately 1:1 and unsmoothed: any damping here would let the board lag behind
                // the finger, which reads as sluggish, not smooth. Smoothing belongs after release.
                Vector3 posBefore = transform.position;

                Vector3 current = cam.ScreenToWorldPoint(screenPos);
                Vector3 delta = dragAnchorWorld - current;
                transform.position += new Vector3(delta.x, delta.y, 0f);
                ClampPosition();

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
        transform.position += new Vector3(panVelocity.x, panVelocity.y, 0f) * Time.deltaTime;
        ClampPosition();

        // Reaching a limit ends the glide on that axis rather than grinding against it.
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

    void ClampPosition()
    {
        if (!hasContent) return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = contentBounds.min.x + halfWidth;
        float maxX = contentBounds.max.x - halfWidth;
        float minY = contentBounds.min.y + halfHeight;
        float maxY = contentBounds.max.y - halfHeight;

        float marginX = halfWidth * panPadding;
        float marginY = halfHeight * panPadding;

        Vector3 p = transform.position;

        // Content view karta nanu hoy (minX > maxX) to pan karvani jarur j nathi — center par lock.
        // Motu hoy to j pan thay, ane tyare kinar thi thodu aagal javani chhut aapo, jethi
        // shape ni farate khali jagya dekhay (reference app jem).
        p.x = minX > maxX ? contentBounds.center.x : Mathf.Clamp(p.x, minX - marginX, maxX + marginX);
        p.y = minY > maxY ? contentBounds.center.y : Mathf.Clamp(p.y, minY - marginY, maxY + marginY);
        transform.position = p;
    }

    // Arrows aa vaapre che: khali SACHO tap (drag/pinch nahi) j arrow ne move karave.
    // Input ek j jagyaae vanchay che — dar arrow potani rite nahi vanchtu (vadhu efficient).
    public bool TryGetTapThisFrame(out Vector2 screenPos)
    {
        screenPos = tapScreenPos;
        return tapThisFrame;
    }
}

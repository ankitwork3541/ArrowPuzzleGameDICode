using UnityEngine;
using UnityEngine.InputSystem;
using PrimeTween;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    private Camera cam;

    [Header("Framing — constant cell size across every level")]
    // Default view ma screen ni height ma keteli grid cells dekhay. Aa J apdo design constant che:
    // e nakki kare ke ek cell screen par ketla pixels no dekhashe, ane e dar level ma SAME rahe che.
    public float cellsVisibleVertically = 32f;

    // Zoom-OUT ni sacchi limit. Aa thi vadhu bahar na jai shakay — etle bahu moti level
    // zoom-out karva chhata pooru na dekhay, ane player e pan/scroll karvu j pade.
    public float maxCellsVisibleVertically = 44f;

    public float fitPadding = 1.12f; // content-fit ni upar thodi breathing room

    [Header("Zoom limits")]
    public float minZoom = 4f; // sauthi vadhu zoom-IN allowed

    [Header("Input feel")]
    public float dragThresholdPixels = 12f; // aa thi vadhu khasyu = drag (tap nahi)
    public float mouseZoomSpeed = 4f;       // editor ma scroll-wheel thi zoom testing mate

    // Content ni kinar thi ketlu aagal pan karva devu (view size na proportion ma).
    // Aa vagar level screen ni kinar sudhi jam thayelu lage — reference app ma scroll karo
    // tyare shape ni farate khali jagya dekhay che, e feel aa thi aave che.
    public float panPadding = 0.35f;

    [Header("Hint focus")]
    public float focusDuration = 0.45f;
    public float focusCellsVisible = 12f;   // how tight the hint view gets, in cells

    private float maxZoom;
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
    // ane e pan maxCellsVisibleVertically sudhi j.
    public void FrameLevel(Bounds bounds, float spacing)
    {
        contentBounds = bounds;
        hasContent = true;

        float defaultZoom = cellsVisibleVertically * spacing / 2f;
        maxZoom = maxCellsVisibleVertically * spacing / 2f;

        float sizeForHeight = bounds.extents.y;
        float sizeForWidth = bounds.extents.x / cam.aspect;
        float fitZoom = Mathf.Max(sizeForHeight, sizeForWidth) * fitPadding;

        cam.orthographicSize = Mathf.Clamp(Mathf.Max(defaultZoom, fitZoom), minZoom, maxZoom);
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

        float distance = Vector2.Distance(
            touch.touches[0].position.ReadValue(),
            touch.touches[1].position.ReadValue());

        if (pinchPrevDistance > 0f && distance > 0f)
        {
            // Aangdio door thay (distance vadhe) = zoom IN = orthographicSize ghate
            ApplyZoom(cam.orthographicSize * (pinchPrevDistance / distance));
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

        ApplyZoom(cam.orthographicSize - scroll * 0.01f * mouseZoomSpeed);
    }

    void ApplyZoom(float newSize)
    {
        cam.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
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
                // Aangdi niche je world-point hato e j aangdi niche rahe — "grab and move" feel
                Vector3 current = cam.ScreenToWorldPoint(screenPos);
                Vector3 delta = dragAnchorWorld - current;
                transform.position += new Vector3(delta.x, delta.y, 0f);
                ClampPosition();
            }
            return;
        }

        if (!pressed && pointerDown)
        {
            pointerDown = false;
            if (!gestureWasDrag && !gestureStartedOnUI)
            {
                tapThisFrame = true;
                tapScreenPos = screenPos;
            }
        }
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

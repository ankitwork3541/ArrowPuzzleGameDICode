using UnityEngine;

// Pins a full-screen backdrop sprite to the screen while the board zooms and pans underneath it.
//
// The HUD gets this behaviour for free: UI Toolkit draws in Overlay mode, outside the camera
// entirely, so pinching the board never touches it. A SpriteRenderer has no such luxury — it
// lives in the world. Parenting it to the camera is only half the job, because an orthographic
// camera's zoom IS its world-to-screen scale: a child still grows and shrinks with
// orthographicSize unless its own scale cancels that out exactly. That is what this does.
[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class ScreenBackdrop : MonoBehaviour
{
    // Cover, not fit: scaled by whichever axis needs MORE, so the sprite always overflows on the
    // other one. Fitting instead would letterbox, and portrait aspects run all the way from 0.75
    // (iPad) to 0.43 (tall Android) — no single image fits that range without bars.
    [Tooltip("Hairline margin past the screen edge, guarding against rounding on odd resolutions.")]
    public float overscan = 1.02f;

    private Camera cam;
    private SpriteRenderer sr;

    // Rescaling allocates nothing, but it does touch the transform, which dirties the renderer's
    // bounds. Skipping the write while the camera is still is free and keeps a static board quiet.
    private float lastSize = -1f;
    private float lastAspect = -1f;

    void OnEnable()
    {
        sr = GetComponent<SpriteRenderer>();
        cam = GetComponentInParent<Camera>();

        if (cam == null)
            Debug.LogError("[ScreenBackdrop] needs to be a child of the Camera — that is what "
                           + "keeps it from panning away when the player drags the board.");
        else if (!cam.orthographic)
            Debug.LogError("[ScreenBackdrop] assumes an orthographic camera; a perspective one "
                           + "would need the distance-to-plane maths instead of orthographicSize.");

        lastSize = -1f;   // force one rebuild on enable, and after a domain reload in the Editor
    }

    // LateUpdate, so the camera has finished zooming for this frame. In Update the backdrop would
    // always be one frame behind, and its edges would visibly breathe during a pinch.
    void LateUpdate()
    {
        if (cam == null || sr == null || sr.sprite == null) return;

        float size = cam.orthographicSize;
        float aspect = cam.aspect;
        if (Mathf.Approximately(size, lastSize) && Mathf.Approximately(aspect, lastAspect)) return;
        lastSize = size;
        lastAspect = aspect;

        float worldHeight = size * 2f;
        float worldWidth = worldHeight * aspect;

        // sprite.bounds is already in world units at scale 1, so this respects whatever Pixels Per
        // Unit the texture was imported with — no hard-coded pixel maths to drift out of sync.
        Vector2 spriteSize = sr.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f) return;

        float scale = Mathf.Max(worldWidth / spriteSize.x, worldHeight / spriteSize.y) * overscan;
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}

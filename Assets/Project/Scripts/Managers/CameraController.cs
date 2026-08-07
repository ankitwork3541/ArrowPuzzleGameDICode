using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    public float padding = 1.15f;      // exact-fit ni upar thodi extra jagya (percentage)
    public float minZoom = 3f;         // sauthi vadhu zoom-IN allowed (chhoti/sparse levels mate)
    public float defaultMaxZoom = 12f; // initial/default view mate sauthi vadhu zoom-OUT (comfortable range)
    public float referenceZoom = 9.2f; // orthographicSize jya apdi baseline line-width/sprite-size barobar lage

    private float contentFitZoom; // level ne POORU batavva mate joiti zoom — pinch-zoom-out ni sachi limit

    public float MinZoom => minZoom;
    public float MaxZoom => contentFitZoom;
    public float CurrentVisualScale => cam.orthographicSize / referenceZoom;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // bounds = ACTUAL rendered content ni bounding box (LineRenderer + sprites, badhu saathe) —
    // guess-karel margin nahi, Unity j calculate kare che line-width/sprite-size sathe.
    public void FitToBounds(Bounds bounds)
    {
        float sizeForHeight = bounds.extents.y;
        float sizeForWidth = bounds.extents.x / cam.aspect;

        float requiredSize = Mathf.Max(sizeForHeight, sizeForWidth, 1f);
        contentFitZoom = requiredSize * padding;

        // Default/initial view — comfortable range ma clamp, chahe content 5 arrow nu hoy ke 80 nu
        cam.orthographicSize = Mathf.Clamp(contentFitZoom, minZoom, defaultMaxZoom);
        transform.position = new Vector3(bounds.center.x, bounds.center.y, transform.position.z);
    }
}

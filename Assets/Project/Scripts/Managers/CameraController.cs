using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    [Header("Default framing — constant cell size across every level")]
    // Default view ma screen ni height ma keteli grid cells dekhay. Aa J apdo design constant che:
    // e nakki kare ke ek cell screen par ketla pixels no dekhashe, ane e dar level ma SAME rahe che.
    public float cellsVisibleVertically = 32f;

    [Header("Zoom-out (moti levels je default view ma na samay)")]
    public float fitPadding = 1.12f; // content-fit ni upar thodi breathing room

    [Header("Pinch-zoom limits (player manually zoom kare tyare)")]
    public float minZoom = 4f; // sauthi vadhu zoom-IN allowed

    private float framedZoom; // aa level mate apde je zoom set karyu — pinch-zoom-OUT ni limit

    public float MinZoom => minZoom;
    public float MaxZoom => framedZoom;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    // bounds = ACTUAL rendered content ni bounding box (LineRenderer + head/tail sprites saathe).
    // Rule: ZOOM-OUT ONLY. Nani level default (constant cell size) par j rahe — nani, centered,
    // farate khali jagya — jem reference app ma thay che. Khali moti level j zoom-out thay.
    public void FrameLevel(Bounds bounds, float spacing)
    {
        float defaultZoom = (cellsVisibleVertically * spacing) / 2f;

        float sizeForHeight = bounds.extents.y;
        float sizeForWidth = bounds.extents.x / cam.aspect;
        float fitZoom = Mathf.Max(sizeForHeight, sizeForWidth) * fitPadding;

        framedZoom = Mathf.Max(defaultZoom, fitZoom);

        cam.orthographicSize = framedZoom;
        transform.position = new Vector3(bounds.center.x, bounds.center.y, transform.position.z);
    }
}

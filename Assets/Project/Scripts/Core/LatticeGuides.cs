using UnityEngine.Serialization;
using System.Collections.Generic;
using UnityEngine;

// Faint guide rays showing where each arrow would travel, toggled by the HUD's grid
// button. A ray starts AT the arrow's head and runs in the head's direction to the
// screen edge — nothing is drawn behind the arrow.
//
// Every ray lives in ONE procedurally built mesh, so the whole overlay costs a single
// draw call however many rays there are. Unity's docs state that runtime-generated
// geometry (lines, trails, particles) "submits one draw call for each mesh", so a
// LineRenderer per ray would cost one draw call per ray.
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class LatticeGuides : MonoBehaviour
{
    public BoardBuilder gridBuilder;
    public StageLoader levelLoader;

    [Tooltip("Camera the ray length is derived from. Left empty, Camera.main is used.")]
    public Camera targetCamera;

    [Tooltip("World-space thickness of a ray. Arrow bodies are 0.25 of a cell.")]
    public float lineWidth = 0.04f;

    public Color lineColor = new Color(0.47f, 0.35f, 0.24f, 0.22f);   // warm brown, reads on cream

    [Tooltip("How far past the visible screen edge each ray extends, in cells.")]
    public float screenMarginCells = 2f;

    [Tooltip("Negative keeps the rays behind the arrows.")]
    public int sortingOrder = -1;

    [Tooltip("On: only arrows whose exit path is currently clear get a ray (fewer rays). Off: every arrow.")]
    [FormerlySerializedAs("onlyArrowsThatCanExit")]
    public bool onlyDragonsThatCanExit = false;

    private Mesh mesh;
    private MeshRenderer meshRenderer;

    // One entry per (lane, direction). The value is the head coordinate of the arrow
    // whose ray reaches FURTHEST, which fully covers every other ray in that lane and
    // direction. Without this, overlapping semi-transparent quads would double up and
    // show as brighter patches.
    private readonly Dictionary<int, int> raysRight = new Dictionary<int, int>(); // row -> min head.x
    private readonly Dictionary<int, int> raysLeft = new Dictionary<int, int>();  // row -> max head.x
    private readonly Dictionary<int, int> raysUp = new Dictionary<int, int>();    // col -> min head.y
    private readonly Dictionary<int, int> raysDown = new Dictionary<int, int>();  // col -> max head.y

    private readonly Dictionary<int, int> prevRight = new Dictionary<int, int>();
    private readonly Dictionary<int, int> prevLeft = new Dictionary<int, int>();
    private readonly Dictionary<int, int> prevUp = new Dictionary<int, int>();
    private readonly Dictionary<int, int> prevDown = new Dictionary<int, int>();

    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<Vector2> uvs = new List<Vector2>();
    private readonly List<Color> colors = new List<Color>();
    private readonly List<int> triangles = new List<int>();

    private bool dirty = true;

    // Ray length depends on the camera, so zooming or panning has to rebuild too.
    private Vector3 prevCamPos;
    private float prevCamSize;

    // Whichever colour path the assigned material actually honours — never both, or the
    // two would multiply and the rays would come out far too faint. Mirrors what
    // DragonController.SetLineColor() already does for the arrow bodies.
    private bool useVertexColor;

    public bool IsVisible => meshRenderer != null && meshRenderer.enabled;

    void Awake()
    {
        mesh = new Mesh { name = "LatticeGuides" };
        mesh.MarkDynamic();                    // rebuilt as arrows move
        GetComponent<MeshFilter>().mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = sortingOrder;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.enabled = false;

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void OnDestroy()
    {
        if (mesh != null)
            Destroy(mesh);
    }

    public void SetVisible(bool value)
    {
        meshRenderer.enabled = value;
        dirty = true;
    }

    public void Toggle()
    {
        SetVisible(!IsVisible);
    }

    void LateUpdate()
    {
        if (!meshRenderer.enabled || targetCamera == null) return;

        CollectRays();
        if (CameraMoved()) dirty = true;

        // Scanning the arrows is cheap; rebuilding the mesh is not free, so only do it
        // when a head has actually moved.
        if (!dirty && Unchanged()) return;

        RebuildMesh();

        dirty = false;
        Snapshot();
    }

    bool CameraMoved()
    {
        Vector3 pos = targetCamera.transform.position;
        float size = targetCamera.orthographicSize;

        if (pos == prevCamPos && Mathf.Approximately(size, prevCamSize)) return false;

        prevCamPos = pos;
        prevCamSize = size;
        return true;
    }

    void CollectRays()
    {
        raysRight.Clear();
        raysLeft.Clear();
        raysUp.Clear();
        raysDown.Clear();

        foreach (DragonController dragon in levelLoader.SpawnedDragons)
        {
            if (dragon == null) continue;
            if (onlyDragonsThatCanExit && !dragon.CanExit()) continue;

            List<Vector2Int> path = dragon.path;
            int n = path.Count;
            if (n < 2) continue;

            Vector2Int head = path[n - 1];
            Vector2Int dir = head - path[n - 2];

            if (dir.x > 0) KeepMin(raysRight, head.y, head.x);
            else if (dir.x < 0) KeepMax(raysLeft, head.y, head.x);
            else if (dir.y > 0) KeepMin(raysUp, head.x, head.y);
            else if (dir.y < 0) KeepMax(raysDown, head.x, head.y);
        }
    }

    static void KeepMin(Dictionary<int, int> map, int lane, int value)
    {
        if (!map.TryGetValue(lane, out int existing) || value < existing)
            map[lane] = value;
    }

    static void KeepMax(Dictionary<int, int> map, int lane, int value)
    {
        if (!map.TryGetValue(lane, out int existing) || value > existing)
            map[lane] = value;
    }

    bool Unchanged()
    {
        return Same(raysRight, prevRight) && Same(raysLeft, prevLeft)
            && Same(raysUp, prevUp) && Same(raysDown, prevDown);
    }

    static bool Same(Dictionary<int, int> a, Dictionary<int, int> b)
    {
        if (a.Count != b.Count) return false;
        foreach (KeyValuePair<int, int> kv in a)
        {
            if (!b.TryGetValue(kv.Key, out int other) || other != kv.Value) return false;
        }
        return true;
    }

    void Snapshot()
    {
        Copy(raysRight, prevRight);
        Copy(raysLeft, prevLeft);
        Copy(raysUp, prevUp);
        Copy(raysDown, prevDown);
    }

    static void Copy(Dictionary<int, int> from, Dictionary<int, int> to)
    {
        to.Clear();
        foreach (KeyValuePair<int, int> kv in from) to[kv.Key] = kv.Value;
    }

    void RebuildMesh()
    {
        ApplyColor();

        vertices.Clear();
        uvs.Clear();
        colors.Clear();
        triangles.Clear();

        float half = lineWidth * 0.5f;
        float margin = screenMarginCells * gridBuilder.spacing;

        // Ray length comes from what the camera can actually see, NOT from the grid
        // bounds. A small level is framed with a lot of empty space around it, so
        // grid-bound rays would visibly stop short of the screen edge.
        Vector3 camPos = targetCamera.transform.position;
        float halfH = targetCamera.orthographicSize + margin;
        float halfW = targetCamera.orthographicSize * targetCamera.aspect + margin;

        float screenLeft = camPos.x - halfW;
        float screenRight = camPos.x + halfW;
        float screenBottom = camPos.y - halfH;
        float screenTop = camPos.y + halfH;

        foreach (KeyValuePair<int, int> kv in raysRight)
        {
            Vector3 headPos = gridBuilder.GridToWorld(new Vector2Int(kv.Value, kv.Key));
            AddQuad(headPos.x, screenRight, headPos.y - half, headPos.y + half);
        }

        foreach (KeyValuePair<int, int> kv in raysLeft)
        {
            Vector3 headPos = gridBuilder.GridToWorld(new Vector2Int(kv.Value, kv.Key));
            AddQuad(screenLeft, headPos.x, headPos.y - half, headPos.y + half);
        }

        foreach (KeyValuePair<int, int> kv in raysUp)
        {
            Vector3 headPos = gridBuilder.GridToWorld(new Vector2Int(kv.Key, kv.Value));
            AddQuad(headPos.x - half, headPos.x + half, headPos.y, screenTop);
        }

        foreach (KeyValuePair<int, int> kv in raysDown)
        {
            Vector3 headPos = gridBuilder.GridToWorld(new Vector2Int(kv.Key, kv.Value));
            AddQuad(headPos.x - half, headPos.x + half, screenBottom, headPos.y);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
    }

    void ApplyColor()
    {
        Material shared = meshRenderer.sharedMaterial;

        if (shared == null)
        {
            useVertexColor = true;      // no material: vertex colour is all we have
            return;
        }

        if (shared.HasProperty("_Color"))
        {
            meshRenderer.material.color = lineColor;
            useVertexColor = false;
        }
        else if (shared.HasProperty("_BaseColor"))
        {
            meshRenderer.material.SetColor("_BaseColor", lineColor);
            useVertexColor = false;
        }
        else
        {
            useVertexColor = true;
        }
    }

    void AddQuad(float xMin, float xMax, float yMin, float yMax)
    {
        int v = vertices.Count;

        vertices.Add(new Vector3(xMin, yMin, 0f));
        vertices.Add(new Vector3(xMin, yMax, 0f));
        vertices.Add(new Vector3(xMax, yMax, 0f));
        vertices.Add(new Vector3(xMax, yMin, 0f));

        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(0f, 1f));
        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(1f, 0f));

        Color c = useVertexColor ? lineColor : Color.white;
        for (int i = 0; i < 4; i++)
            colors.Add(c);

        triangles.Add(v);     triangles.Add(v + 1); triangles.Add(v + 2);
        triangles.Add(v);     triangles.Add(v + 2); triangles.Add(v + 3);
    }
}

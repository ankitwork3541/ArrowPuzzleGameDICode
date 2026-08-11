using System.Collections.Generic;
using UnityEngine;

// Faint straight lines showing the lane each arrow travels along, toggled by the
// HUD's grid button.
//
// Every line lives in ONE procedurally built mesh, so the whole overlay costs a
// single draw call no matter how many lines there are. Unity's own docs state that
// runtime-generated geometry (lines, trails, particles) "submits one draw call for
// each mesh", so a LineRenderer per line would cost one draw call per line.
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class GuideLines : MonoBehaviour
{
    public GridBuilder gridBuilder;
    public LevelLoader levelLoader;

    [Tooltip("World-space thickness of a guide line. Arrow bodies are 0.25 of a cell.")]
    public float lineWidth = 0.04f;

    public Color lineColor = new Color(0.62f, 0.69f, 1f, 0.22f);

    [Tooltip("How far past the grid edge each line extends, in cells.")]
    public int overshootCells = 2;

    [Tooltip("Negative keeps the lines behind the arrows.")]
    public int sortingOrder = -1;

    [Tooltip("On: only arrows whose exit path is currently clear get a line (fewer lines, closer to the reference app). Off: every arrow.")]
    public bool onlyArrowsThatCanExit = false;

    private Mesh mesh;
    private MeshRenderer meshRenderer;

    // Every collection below is reused, so a visible overlay allocates nothing per frame.
    private readonly HashSet<int> rows = new HashSet<int>();
    private readonly HashSet<int> cols = new HashSet<int>();
    private readonly HashSet<int> prevRows = new HashSet<int>();
    private readonly HashSet<int> prevCols = new HashSet<int>();

    private readonly List<Vector3> vertices = new List<Vector3>();
    private readonly List<Vector2> uvs = new List<Vector2>();
    private readonly List<Color> colors = new List<Color>();
    private readonly List<int> triangles = new List<int>();

    private bool dirty = true;

    // Whichever colour path the assigned material actually honours — never both, or
    // the two would multiply and the lines would come out far too faint. Mirrors what
    // ArrowController.SetLineColor() already does for the arrow bodies.
    private bool useVertexColor;

    public bool IsVisible => meshRenderer != null && meshRenderer.enabled;

    void Awake()
    {
        mesh = new Mesh { name = "GuideLines" };
        mesh.MarkDynamic();                    // rebuilt as arrows move
        GetComponent<MeshFilter>().mesh = mesh;

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sortingOrder = sortingOrder;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
        meshRenderer.enabled = false;
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
        if (!meshRenderer.enabled) return;

        CollectLanes();

        // Scanning the arrows is cheap; rebuilding the mesh is not free, so only do
        // it when an arrow has actually moved into or out of a lane.
        if (!dirty && rows.SetEquals(prevRows) && cols.SetEquals(prevCols)) return;

        RebuildMesh();

        dirty = false;
        prevRows.Clear(); prevRows.UnionWith(rows);
        prevCols.Clear(); prevCols.UnionWith(cols);
    }

    void CollectLanes()
    {
        rows.Clear();
        cols.Clear();

        foreach (ArrowController arrow in levelLoader.SpawnedArrows)
        {
            if (arrow == null) continue;
            if (onlyArrowsThatCanExit && !arrow.CanExit()) continue;

            List<Vector2Int> path = arrow.path;
            int n = path.Count;
            if (n < 2) continue;

            Vector2Int head = path[n - 1];
            Vector2Int dir = head - path[n - 2];

            // The lane this arrow travels along: its head's row when it moves sideways,
            // its head's column when it moves vertically. Several arrows often share one
            // lane and the HashSet collapses those into a single line — which is why the
            // spacing looks irregular instead of like a uniform grid.
            if (dir.x != 0) rows.Add(head.y);
            else if (dir.y != 0) cols.Add(head.x);
        }
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

    void RebuildMesh()
    {
        ApplyColor();

        vertices.Clear();
        uvs.Clear();
        colors.Clear();
        triangles.Clear();

        float half = lineWidth * 0.5f;
        int o = overshootCells;

        foreach (int y in rows)
        {
            Vector3 a = gridBuilder.GridToWorld(new Vector2Int(-o, y));
            Vector3 b = gridBuilder.GridToWorld(new Vector2Int(gridBuilder.width - 1 + o, y));
            AddQuad(a.x, b.x, a.y - half, a.y + half);
        }

        foreach (int x in cols)
        {
            Vector3 a = gridBuilder.GridToWorld(new Vector2Int(x, -o));
            Vector3 b = gridBuilder.GridToWorld(new Vector2Int(x, gridBuilder.height - 1 + o));
            AddQuad(a.x - half, a.x + half, a.y, b.y);
        }

        mesh.Clear();
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetColors(colors);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
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

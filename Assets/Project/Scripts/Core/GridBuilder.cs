using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GridBuilder : MonoBehaviour
{
    public int width = 5;
    public int height = 5;
    public float spacing = 1f;
    public Vector2 origin; // grid na (0,0) cell ni world position — center karva mate calculate thay
    public Dot dotPrefab;


    private Dictionary<Vector2Int, GridCell> cells = new Dictionary<Vector2Int, GridCell>();
    private Dictionary<Vector2Int, Dot> dots = new Dictionary<Vector2Int, Dot>();
    private ObjectPool<Dot> dotPool;

    void Awake()
    {
        dotPool = new ObjectPool<Dot>(
            createFunc: () => Instantiate(dotPrefab, transform),
            actionOnGet: dot => { dot.gameObject.SetActive(true); dot.Show(); },
            actionOnRelease: dot => dot.gameObject.SetActive(false),
            // The null check is not paranoia: leaving Play mode makes Unity clear every
            // ObjectPool, but the scene's GameObjects are already gone by then, so this
            // callback runs on dots that no longer exist. Unity's overloaded == reports a
            // destroyed object as null, which is exactly what catches that case.
            actionOnDestroy: dot => { if (dot != null) Destroy(dot.gameObject); }
        );
        // BuildGrid() have Awake() mathi call nathi thato — LevelLoader/LevelPainter potej
        // Play/tool-start vakhte call kare che, jethi Splash/Home screen pachhad grid/dots
        // dekhaay nahi (jem arrows mate LevelLoader.Start() pehla fix karyu hatu tem j pattern).
    }

    public void BuildGrid(int newWidth, int newHeight)
    {
        width = newWidth;
        height = newHeight;
        cells.Clear();

        // Grid ne world (0,0) ni around center karo — motu ke chhotu, hamesha vachche j rahe
        origin = new Vector2(-(width - 1) * spacing / 2f, -(height - 1) * spacing / 2f);

        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < width; col++)
            {
                Vector2Int gridPos = new Vector2Int(col, row);
                cells[gridPos] = new GridCell(gridPos);
            }
        }

        // Dots are deliberately NOT spawned here: the level's footprint isn't known yet, and
        // LevelLoader owns their lifecycle anyway (it hides them for the entrance wave, then
        // spawns them once the arrows have finished drawing themselves in).
    }

    // Release every dot back to the pool. Called when a level ends, so the last
    // level's dot grid doesn't stay visible behind the Home screen.
    public void ClearDots()
    {
        foreach (var kvp in dots)
        {
            dotPool.Release(kvp.Value);
        }
        dots.Clear();
    }

    // Which cells actually belong to the level being played. The grid is only a BOUNDING BOX —
    // for a shape level (heart, teardrop) most of that box is outside the shape and has to stay
    // blank, or the silhouette is lost in a full rectangle of dots.
    private HashSet<Vector2Int> footprint;

    public void SetFootprint(HashSet<Vector2Int> cellsInLevel)
    {
        footprint = cellsInLevel;
    }

    public void SpawnDots()
    {
        ClearDots();
        if (footprint == null) return;   // no level loaded — nothing to draw

        foreach (Vector2Int pos in footprint)
        {
            GridCell cell = GetCell(pos);
            if (cell == null) continue;

            Dot dot = dotPool.Get();
            dot.transform.position = GridToWorld(pos);
            dot.GridPosition = pos;
            dots[pos] = dot;

            // Spawned after the arrows now, so respect whoever is already standing there.
            if (cell.Occupant != null) dot.Hide();
        }
    }

    // Who is standing on this cell, if anyone. This is how a tap finds its arrow — see
    // LevelLoader.Update(). Returns the GameObject rather than an ArrowController so the grid
    // stays unaware of what kinds of thing can occupy it.
    public GameObject GetOccupant(Vector2Int pos)
    {
        GridCell cell = GetCell(pos);
        return cell != null ? cell.Occupant : null;
    }

    // Declared grid size (BuildGrid) ni jagyaae, ACTUAL content (arrows) ni bounding box ne center karo.
    // Sparse/chhoti levels pan barobar screen-center thay, chahe declared grid size motu hoy.
    public void CenterOnContent(Vector2Int min, Vector2Int max)
    {
        float centerX = (min.x + max.x) / 2f;
        float centerY = (min.y + max.y) / 2f;
        origin = new Vector2(-centerX * spacing, -centerY * spacing);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(origin.x + gridPos.x * spacing, origin.y + gridPos.y * spacing, 0f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var kvp in cells)
        {
            Gizmos.DrawWireSphere(GridToWorld(kvp.Key), 0.1f);
        }
    }

    public GridCell GetCell(Vector2Int pos)
    {
        if (cells.ContainsKey(pos))
            return cells[pos];
        return null;
    }

    public bool IsCellOccupied(Vector2Int pos)
    {
        GridCell cell = GetCell(pos);
        if (cell == null) return false; // grid ni bahar nu coordinate = occupied nahi gaanay
        return cell.Occupant != null;
    }

    public void SetOccupant(Vector2Int pos, GameObject occupant)
    {
        GridCell cell = GetCell((pos));
        if (cell != null && cell.Occupant == null) // template ni jem — pehla thi occupied hoy to overwrite na karo
        {
            cell.Occupant = occupant;
            if (dots.ContainsKey(pos)) dots[pos].Hide();
        }
    }

    public void ClearOccupant(Vector2Int pos)
    {
        GridCell cell = GetCell(pos);
        if (cell != null)
        {
            cell.Occupant = null;
            if (dots.ContainsKey(pos)) dots[pos].Reveal();
        }
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - origin.x) / spacing);
        int y = Mathf.RoundToInt((worldPos.y - origin.y) / spacing);
        return new Vector2Int(x, y);
    }

}

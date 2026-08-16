using System.Collections.Generic;
using UnityEngine;

// Scatters a handful of huge, near-invisible arrow silhouettes behind the board and drifts them
// very slowly, so the backdrop reads as this game's rather than as a generic dark screen.
//
// The whole effect lives or dies on CONTRAST, not on content. Against the #050816 base the tint
// wants to sit only three or four percent brighter — far enough to register as texture, nowhere
// near enough to ask to be looked at, and nowhere near Bloom's 0.55 threshold either, so these
// never glow. If you can pick the arrows out from across the room, the tint is too high.
//
// Filled silhouettes, deliberately: the playable arrows are drawn as outlined LineRenderer paths,
// and the two shapes have to stay separable at a glance.
//
// Built in code rather than placed by hand because the copies have to be laid out against the
// camera's CURRENT view — a fixed scene layout would be correct on exactly one aspect ratio.
[ExecuteAlways]
public class BackdropArrows : MonoBehaviour
{
    [Tooltip("Straight / bend / zigzag variants. Each copy picks one at random.")]
    public Sprite[] arrowSprites;

    [Header("Layout")]
    public int count = 7;
    [Tooltip("Each arrow's height as a fraction of the screen's height.")]
    public float minSize = 0.18f;
    public float maxSize = 0.30f;

    // Placement is a jittered grid, not free random. Independent random positions clump: copies
    // land on top of each other, the overlap fuses them into shapes that no longer read as arrows,
    // and every overlapping pixel is paid for twice in alpha blending — the exact cost this
    // backdrop is otherwise careful about. One arrow per cell guarantees a spread instead.
    [Tooltip("Cells the view is divided into. One arrow per cell, so count must fit inside them.")]
    public int columns = 3;
    public int rows = 4;
    [Tooltip("How far an arrow may wander from its cell's centre, as a fraction of the cell.")]
    [Range(0f, 0.5f)] public float jitter = 0.32f;
    [Tooltip("Cells whose centre falls inside this box, in screen heights, are left empty. The "
             + "board sits there; keeping it calm costs nothing.")]
    public float centreKeepOut = 0.18f;
    [Tooltip("Changing this reshuffles the arrangement. Any fixed value gives a stable layout.")]
    public int seed = 20260816;

    [Header("Look")]
    // Roughly #0A0F22 — the base colour lifted about three percent. Start here and go DOWN if the
    // arrows draw the eye; the failure mode is always "too visible", never "too subtle".
    public Color tint = new Color(0.039f, 0.059f, 0.133f, 1f);
    public int sortingOrder = -100;

    [Header("Drift")]
    [Tooltip("How far each arrow wanders from its home spot, in screen heights.")]
    public float driftRadius = 0.03f;
    public float driftSpeed = 0.05f;
    public float spinDegreesPerSecond = 0.4f;

    private Camera cam;
    private Transform[] arrows;
    private SpriteRenderer[] renderers;

    // Per-arrow constants, all decided once from the seed, so the arrangement survives a domain
    // reload and never changes while the player is looking at it.
    private Vector2[] home;
    private float[] size;
    private float[] angle;
    private float[] phaseX, phaseY, spin;

    void OnEnable()
    {
        cam = GetComponentInParent<Camera>();

        if (cam == null)
            Debug.LogError("[BackdropArrows] needs to be a child of the Camera — that is what "
                           + "keeps the backdrop still while the player drags the board.");

        Build();
    }

    void OnDisable()
    {
        if (arrows == null) return;

        foreach (Transform t in arrows)
        {
            if (t == null) continue;
            if (Application.isPlaying) Destroy(t.gameObject);
            else DestroyImmediate(t.gameObject);
        }
        arrows = null;
    }

    void Build()
    {
        OnDisable();
        if (arrowSprites == null || arrowSprites.Length == 0 || count <= 0) return;

        System.Random rng = new System.Random(seed);

        List<Vector2> cells = FreeCells();
        if (cells.Count == 0) return;

        if (count > cells.Count)
        {
            Debug.LogWarning($"[BackdropArrows] {count} arrows asked for but only {cells.Count} "
                             + "cells are free. Raise columns/rows, or lower centreKeepOut.");
            count = cells.Count;
        }

        // Fisher-Yates, then take the first `count`. Picking cells at random WITH replacement
        // would put two arrows in one cell and undo the whole point of the grid.
        for (int i = cells.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (cells[i], cells[j]) = (cells[j], cells[i]);
        }

        arrows = new Transform[count];
        renderers = new SpriteRenderer[count];
        home = new Vector2[count];
        size = new float[count];
        angle = new float[count];
        phaseX = new float[count];
        phaseY = new float[count];
        spin = new float[count];

        float cellW = 1f / columns;
        float cellH = 1f / rows;

        for (int i = 0; i < count; i++)
        {
            GameObject go = new GameObject($"BackdropArrow_{i}");

            // Generated art, not authored art: without this the copies would be written into the
            // scene file on save and rebuilt alongside on the next load, doubling every time.
            go.hideFlags = HideFlags.HideAndDontSave;
            go.transform.SetParent(transform, false);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = arrowSprites[rng.Next(arrowSprites.Length)];
            sr.color = tint;
            sr.sortingOrder = sortingOrder;

            arrows[i] = go.transform;
            renderers[i] = sr;

            home[i] = cells[i] + new Vector2(
                ((float)rng.NextDouble() - 0.5f) * cellW * jitter * 2f,
                ((float)rng.NextDouble() - 0.5f) * cellH * jitter * 2f);

            size[i] = Mathf.Lerp(minSize, maxSize, (float)rng.NextDouble());

            // Off-axis on purpose. The board's arrows only ever run at 0/90/180/270 degrees, so a
            // tilted silhouette is read as scenery before it is read as anything tappable.
            angle[i] = 20f + (float)rng.NextDouble() * 320f;

            phaseX[i] = (float)rng.NextDouble() * Mathf.PI * 2f;
            phaseY[i] = (float)rng.NextDouble() * Mathf.PI * 2f;
            spin[i] = (float)rng.NextDouble() < 0.5 ? -1f : 1f;
        }
    }

    // Cell centres in normalised view space (-0.5 .. 0.5 on both axes), minus the ones over the
    // board. Uses cell CENTRES rather than the cells' full extent, so centreKeepOut stays a
    // readable "how big is the clear middle" dial instead of an all-or-nothing switch.
    List<Vector2> FreeCells()
    {
        List<Vector2> cells = new List<Vector2>();
        if (columns < 1 || rows < 1) return cells;

        for (int c = 0; c < columns; c++)
        {
            for (int r = 0; r < rows; r++)
            {
                Vector2 centre = new Vector2((c + 0.5f) / columns - 0.5f,
                                             (r + 0.5f) / rows - 0.5f);

                if (Mathf.Abs(centre.x) < centreKeepOut && Mathf.Abs(centre.y) < centreKeepOut)
                    continue;

                cells.Add(centre);
            }
        }
        return cells;
    }

    // LateUpdate so the camera has finished zooming for the frame. Running in Update would leave
    // the backdrop one frame behind and make it visibly lag during a pinch.
    void LateUpdate()
    {
        if (cam == null || arrows == null) return;

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;

        // Positions and scales are both derived from the CURRENT orthographicSize, which is what
        // cancels the camera's zoom: everything here is expressed in screen heights, so a pinch
        // changes the world numbers and leaves the on-screen result untouched.
        float t = Application.isPlaying ? Time.time : 0f;

        for (int i = 0; i < arrows.Length; i++)
        {
            if (arrows[i] == null || renderers[i].sprite == null) continue;

            // Two sine waves at different rates, rather than a wrapping scroll — a wandering
            // arrow never reaches an edge, so it never has to teleport back and pop.
            float dx = Mathf.Cos(t * driftSpeed + phaseX[i]) * driftRadius;
            float dy = Mathf.Sin(t * driftSpeed * 0.73f + phaseY[i]) * driftRadius;

            arrows[i].localPosition = new Vector3((home[i].x + dx) * worldWidth,
                                                  (home[i].y + dy) * worldHeight,
                                                  20f);   // clear of the near clip plane
            arrows[i].localRotation = Quaternion.Euler(0f, 0f,
                angle[i] + t * spinDegreesPerSecond * spin[i]);

            float spriteHeight = renderers[i].sprite.bounds.size.y;
            if (spriteHeight <= 0f) continue;

            float scale = size[i] * worldHeight / spriteHeight;
            arrows[i].localScale = new Vector3(scale, scale, 1f);

            // Kept in the loop so the tint stays live-tunable in the Inspector during play —
            // finding the right contrast is done by eye against the real board, not by maths.
            renderers[i].color = tint;
            renderers[i].sortingOrder = sortingOrder;
        }
    }

    void OnValidate()
    {
        // Layout inputs are baked at build time and need a rebuild to take effect. Colour and
        // sorting are reapplied every frame above and deliberately do not trigger one.
        if (isActiveAndEnabled && arrows != null && arrows.Length != count) Build();
    }
}

using System.Collections.Generic;
using UnityEngine;

// Scatters a handful of huge, near-invisible silhouettes behind the board and drifts them
// very slowly, so the backdrop reads as this game's rather than as a generic plain cream screen.
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
    [Tooltip("Faint warm beige a few percent off the cream camera background (#FFF8EC). Any more "
             + "contrast and the silhouettes start competing with the board.")]
    public Color tint = new Color(0.980f, 0.933f, 0.863f, 1f);
    public int sortingOrder = -100;

    [Header("Drift")]
    [Tooltip("How far each arrow wanders from its home spot, in screen heights.")]
    public float driftRadius = 0.03f;
    public float driftSpeed = 0.05f;
    public float spinDegreesPerSecond = 0.4f;

    private Camera cam;
    private Transform[] arrows;
    private SpriteRenderer[] renderers;

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
            angle[i] = 20f + (float)rng.NextDouble() * 320f;

            phaseX[i] = (float)rng.NextDouble() * Mathf.PI * 2f;
            phaseY[i] = (float)rng.NextDouble() * Mathf.PI * 2f;
            spin[i] = (float)rng.NextDouble() < 0.5 ? -1f : 1f;
        }
    }

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

    void LateUpdate()
    {
        if (cam == null || arrows == null) return;

        float worldHeight = cam.orthographicSize * 2f;
        float worldWidth = worldHeight * cam.aspect;

        float t = Application.isPlaying ? Time.time : 0f;

        for (int i = 0; i < arrows.Length; i++)
        {
            if (arrows[i] == null || renderers[i].sprite == null) continue;

            float dx = Mathf.Cos(t * driftSpeed + phaseX[i]) * driftRadius;
            float dy = Mathf.Sin(t * driftSpeed * 0.73f + phaseY[i]) * driftRadius;

            arrows[i].localPosition = new Vector3((home[i].x + dx) * worldWidth,
                                                  (home[i].y + dy) * worldHeight,
                                                  20f);
            arrows[i].localRotation = Quaternion.Euler(0f, 0f,
                angle[i] + t * spinDegreesPerSecond * spin[i]);

            float spriteHeight = renderers[i].sprite.bounds.size.y;
            if (spriteHeight <= 0f) continue;

            float scale = size[i] * worldHeight / spriteHeight;
            arrows[i].localScale = new Vector3(scale, scale, 1f);

            renderers[i].color = tint;
            renderers[i].sortingOrder = sortingOrder;
        }
    }

    void OnValidate()
    {
        if (!isActiveAndEnabled || arrows == null || arrows.Length == count) return;

#if UNITY_EDITOR
        // Build() creates GameObjects and AddComponents, which Unity forbids calling
        // synchronously from inside OnValidate (it uses SendMessage internally for this).
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null) Build();
        };
#else
        Build();
#endif
    }
}

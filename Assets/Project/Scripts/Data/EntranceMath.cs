using UnityEngine;

// How a level builds itself in. Three looks, picked by the config (`ui.entrance_style`):
//   Ink     - an ink-brush stroke draws each dragon from tail to head, the paint dries from ink to the
//             dragon's colour, the head pops when the stroke ends, dragons start centre-out in a ripple.
//   Fly     - each dragon slides in from just outside the board along its own heading (the way it exits),
//             nearest edge first, with a small overshoot settle.
//   Classic - the first look (a plain pen stroke, centre-out), kept only for comparing during review.
//
// Everything in here is a pure function of numbers, so the ordering, timing and skip rules can be tested
// without a scene. The scene code (StageLoader, DragonController) only applies what these return.
public enum EntranceStyle
{
    Classic,
    Ink,
    Fly
}

public static class EntranceMath
{
    public const EntranceStyle DefaultStyle = EntranceStyle.Ink;

    // ---------- style key ----------

    // Unknown, empty or null falls back to the default, so a typo in the API never breaks a level start.
    public static EntranceStyle ParseStyle(string key)
    {
        if (string.IsNullOrEmpty(key)) return DefaultStyle;

        switch (key.Trim().ToLowerInvariant())
        {
            case "ink": return EntranceStyle.Ink;
            case "fly": return EntranceStyle.Fly;
            case "classic": return EntranceStyle.Classic;
            default: return DefaultStyle;
        }
    }

    public static string StyleKey(EntranceStyle style)
    {
        switch (style)
        {
            case EntranceStyle.Fly: return "fly";
            case EntranceStyle.Classic: return "classic";
            default: return "ink";
        }
    }

    // ---------- timing ----------

    // How long ONE dragon takes from its first moment to its last.
    public const float PerDragonSeconds = 0.45f;

    // The window the starts are spread over. A small board (about 0.9 s in all) can afford a gentle ripple;
    // the biggest boards (200+ dragons) widen the window a little but never past 1.2 s in all, because the
    // dragons overlap heavily: a fixed gap per dragon would turn 300 dragons into many seconds.
    public const float StaggerSmall = 0.45f;
    public const float StaggerBig = 0.75f;
    public const int SmallBoardDragons = 20;
    public const int BigBoardDragons = 200;

    public static float StaggerWindow(int dragonCount)
    {
        float k = Mathf.Clamp01((dragonCount - SmallBoardDragons) / (float)(BigBoardDragons - SmallBoardDragons));
        return Mathf.Lerp(StaggerSmall, StaggerBig, k);
    }

    public static float TotalSeconds(int dragonCount)
    {
        return dragonCount <= 0 ? 0f : StaggerWindow(dragonCount) + PerDragonSeconds;
    }

    // Where the rank-th starter (0 = first) begins inside the window. Spread over a FIXED window, not a fixed
    // gap, so the total never depends on how many dragons there are beyond the cap above.
    public static float DelayFor(int rank, int count, float window)
    {
        return count > 1 ? Mathf.Clamp01(rank / (float)(count - 1)) * window : 0f;
    }

    // One dragon's own 0..1 clock.
    public static float LocalProgress(float elapsed, float delay, float perDragon)
    {
        return perDragon <= 0f ? 1f : Mathf.Clamp01((elapsed - delay) / perDragon);
    }

    public static float OutQuad(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - (1f - t) * (1f - t);
    }

    static float OutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        float u = 1f - t;
        return 1f - u * u * u;
    }

    static float InOutSine(float t)
    {
        return 0.5f - 0.5f * Mathf.Cos(Mathf.PI * Mathf.Clamp01(t));
    }

    // ---------- ordering ----------

    // Indices from the centre of the board outwards. `centres` are world positions with the board centred on
    // (0, 0). Ties break by index, so the order is deterministic.
    public static int[] CenterOutOrder(Vector2[] centres)
    {
        float[] keys = new float[centres.Length];
        for (int i = 0; i < centres.Length; i++) keys[i] = centres[i].sqrMagnitude;
        return AscendingOrder(keys);
    }

    // Indices with the smallest key first (keys are distances to the entry edge). Ties break by index.
    public static int[] NearestEdgeFirstOrder(float[] distances)
    {
        return AscendingOrder(distances);
    }

    static int[] AscendingOrder(float[] keys)
    {
        int n = keys.Length;
        int[] order = new int[n];
        for (int i = 0; i < n; i++) order[i] = i;

        // Insertion sort: stable, allocation-free, and boards have a few hundred dragons at most, once per
        // level start.
        for (int i = 1; i < n; i++)
        {
            int item = order[i];
            float key = keys[item];
            int j = i - 1;
            while (j >= 0 && keys[order[j]] > key)
            {
                order[j + 1] = order[j];
                j--;
            }
            order[j + 1] = item;
        }

        return order;
    }

    // ---------- ink ----------

    // The stroke is drawn over the first part of a dragon's clock; the rest is the head pop and the paint
    // drying.
    public const float InkDrawEnd = 0.8f;
    public const float InkPopScale = 0.15f;

    // Fraction of the body drawn at clock t (0 = nothing, 1 = the whole dragon).
    public static float InkStroke(float t)
    {
        return OutQuad(Mathf.Clamp01(t) / InkDrawEnd);
    }

    // 1 while the brush is moving, then it dries to 0 over the rest of the clock.
    public static float InkWet(float t)
    {
        if (t <= InkDrawEnd) return 1f;
        return 1f - Mathf.Clamp01((t - InkDrawEnd) / (1f - InkDrawEnd));
    }

    // Head scale: 1 -> 1.15 -> 1 across the pop phase, exactly 1 outside it.
    public static float InkPop(float t)
    {
        if (t <= InkDrawEnd || t >= 1f) return 1f;
        float u = (t - InkDrawEnd) / (1f - InkDrawEnd);
        return 1f + InkPopScale * Mathf.Sin(Mathf.PI * u);
    }

    // The faint paper unfold: a lattice dot flashes as the ripple passes it, a little ahead of the strokes.
    // radius01 is the dot's distance from the board centre, 0 (centre) to 1 (the farthest dot).
    public const float PaperFlashSeconds = 0.35f;
    public const float PaperMaxAlpha = 0.6f;

    public static float PaperStart(float radius01, float window)
    {
        return Mathf.Clamp01(radius01) * window * 0.8f;
    }

    public static float PaperWaveAlpha(float elapsed, float radius01, float window)
    {
        float u = (elapsed - PaperStart(radius01, window)) / PaperFlashSeconds;
        if (u <= 0f || u >= 1f) return 0f;
        return Mathf.Sin(Mathf.PI * u) * PaperMaxAlpha;
    }

    // The moment every lattice dot is finished, so the scene can release them.
    public static float PaperEnd(float window)
    {
        return PaperStart(1f, window) + PaperFlashSeconds;
    }

    // Where along the body (0 tail .. 1 head) the padlock sits, as the fraction of drawn length, so the
    // badge appears when the brush has passed it.
    public static float BadgeAppear(float stroke, float badgeFraction)
    {
        return Mathf.Clamp01((stroke - badgeFraction) / 0.12f);
    }

    // ---------- fly ----------

    public const float FlySlideEnd = 0.75f;      // the slide and settle are done by here; the badge pops after
    const float FlyArrive = 0.6f;                // the dragon reaches its overshoot point here

    // How many cells (a unit grid vector `dir` = the dragon's heading) behind the board edge the dragon starts, so
    // that the WHOLE dragon is outside the board. Uses the dragon's own cell bounds and the board's.
    public static float FlyStartCells(Vector2Int dir, Vector2Int dragonMin, Vector2Int dragonMax,
                                      Vector2Int boardMin, Vector2Int boardMax)
    {
        if (dir.x > 0) return dragonMax.x - boardMin.x + 1;
        if (dir.x < 0) return boardMax.x - dragonMin.x + 1;
        if (dir.y > 0) return dragonMax.y - boardMin.y + 1;
        return boardMax.y - dragonMin.y + 1;
    }

    // Signed distance along the heading, in cells, at clock t: -startCells before the dragon moves, a
    // small positive overshoot just after it arrives, exactly 0 when it has settled (t >= FlySlideEnd).
    public static float FlyOffset(float t, float startCells, float overshootCells)
    {
        t = Mathf.Clamp01(t);
        if (t >= FlySlideEnd) return 0f;

        if (t <= FlyArrive)
            return Mathf.Lerp(-startCells, overshootCells, OutCubic(t / FlyArrive));

        return Mathf.Lerp(overshootCells, 0f, InOutSine((t - FlyArrive) / (FlySlideEnd - FlyArrive)));
    }

    // The padlock badge after its dragon has landed: 0 before, a small pop (to 1.15 and back to 1) across the
    // last quarter, exactly 1 at the end.
    public static float FlyBadgeScale(float t)
    {
        if (t <= FlySlideEnd) return 0f;
        float u = Mathf.Clamp01((t - FlySlideEnd) / (1f - FlySlideEnd));
        return u < 0.6f
            ? Mathf.Lerp(0f, 1.15f, OutCubic(u / 0.6f))
            : Mathf.Lerp(1.15f, 1f, InOutSine((u - 0.6f) / 0.4f));
    }

    // The ink style pops the badge in once the brush has passed it.
    public static float InkBadgeScale(float stroke, float badgeFraction)
    {
        float u = BadgeAppear(stroke, badgeFraction);
        if (u <= 0f) return 0f;
        return u < 0.6f
            ? Mathf.Lerp(0f, 1.15f, OutCubic(u / 0.6f))
            : Mathf.Lerp(1.15f, 1f, InOutSine((u - 0.6f) / 0.4f));
    }

    // ---------- skip ----------

    // A level start in flight. Pure state, so "a tap skips to the end, and the end is announced once" can be
    // tested without a scene.
    public struct State
    {
        public bool Running;
        public bool Finished;
    }

    public static void Begin(ref State s)
    {
        s.Running = true;
        s.Finished = false;
    }

    // A board tap during the entrance skips it (and must not also reach a dragon).
    public static bool ShouldSkipOnTap(State s)
    {
        return s.Running && !s.Finished;
    }

    // True exactly once per Begin: the caller then snaps every dragon to its final state and announces the end.
    public static bool TryFinish(ref State s)
    {
        if (!s.Running) return false;

        s.Running = false;
        s.Finished = true;
        return true;
    }

    // The level was torn down mid-entrance (restart, leaving to Home): nothing is announced.
    public static void Abort(ref State s)
    {
        s.Running = false;
    }
}

using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;

// Turns a silhouette image into a carve mask, unattended, over as many source images as you like.
//
// Source images live OUTSIDE Assets (see SourceFolder). They are read as raw file bytes, so Unity
// never imports them: no .meta files, no import cost, and no way for them to reach a build.
//
// THE THING THAT MATTERS: how many cells a shape needs is a property OF THE SHAPE, not a number to
// pick by hand. What kills a silhouette at low resolution is not its size but its thinnest limb —
// anything narrower than about two cells disappears at Nyquist, which is why a chess knight loses
// its ears and muzzle at 13 across and reads fine at 33. So the shape is measured (Euclidean
// distance transform, 25th-percentile half-width) and the grid is sized from that measurement.
//
// The percentile is calibrated, not guessed. On the knight source p25 gives 32.5 cells, which lands
// on three independent anchors: our own observation that 13 fails and 26 works, Torralba's ~32
// px-per-side floor for reliable recognition (How many pixels make an image?, 2009), and the
// reference game's shaped levels all sitting at 33-40 cells square. p10 was tried first and is far
// too pessimistic (65 cells) because it measures the antialiased edge fringe, not a real feature.
public static class LevelShapeMask
{
    public const string SourceFolder = "LevelShapeSources";

    // Where the thin-feature measurement is taken. See the header: p10 measures the alpha fringe.
    const float WidthPercentile = 25f;

    // A feature is preserved when it spans at least this many cells.
    const float MinCellsPerFeature = 2f;

    // Area-preserving threshold search. 0.5 is correct for a straight edge but deletes everything
    // thinner than one cell; going below 0.35 starts closing concavities, which costs more
    // recognisability than the thin bits it saves.
    const float CoverageMin = 0.35f;
    const float CoverageMax = 0.50f;

    const int HardMaxCellsAcross = 60;

    static readonly Vector2Int[] Dirs =
    {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 0), new Vector2Int(-1, 0)
    };

    public static string FolderPath
    {
        get { return Path.Combine(Directory.GetParent(Application.dataPath).FullName, SourceFolder); }
    }

    public static string[] ListShapes()
    {
        if (!Directory.Exists(FolderPath)) return new string[0];

        List<string> found = new List<string>();
        foreach (string f in Directory.GetFiles(FolderPath))
        {
            string ext = Path.GetExtension(f).ToLowerInvariant();
            if (ext == ".png" || ext == ".jpg" || ext == ".jpeg") found.Add(f);
        }
        found.Sort();
        return found.ToArray();
    }

    // `requestedCells` is a suggestion. If the shape needs more to stay recognisable it gets more —
    // that is the whole point, and it is why this never needs a per-image hand-tuned size.
    public static HashSet<Vector2Int> Build(string imagePath, int requestedCells,
                                            out int width, out int height, out string report)
    {
        width = height = 0;
        report = null;

        bool[] solid;
        int tw, th;
        if (!LoadSolid(imagePath, out solid, out tw, out th, out report)) return null;

        CloseGaps(solid, tw, th);

        int minX, maxX, minY, maxY;
        if (!InkBounds(solid, tw, th, out minX, out maxX, out minY, out maxY))
        {
            report = "blank after thresholding";
            return null;
        }

        int cropW = maxX - minX + 1, cropH = maxY - minY + 1;

        // Thinnest-limb measurement drives the grid size.
        float halfWidth = FeatureHalfWidth(solid, tw, th, minX, maxX, minY, maxY);
        int required = Mathf.CeilToInt(cropW / Mathf.Max(0.5f, halfWidth * 2f / MinCellsPerFeature));

        int across = Mathf.Clamp(Mathf.Max(requestedCells, required), 4, HardMaxCellsAcross);
        width = across;
        height = Mathf.Max(3, Mathf.RoundToInt(across * cropH / (float)cropW));

        float[] coverage = CoverageField(solid, tw, th, minX, minY, cropW, cropH, width, height);

        // Match the source's ink area rather than fixing the threshold, so a shape made of thin
        // parts is allowed to fatten slightly instead of vanishing.
        int inkPixels = 0;
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                if (solid[y * tw + x]) inkPixels++;

        float wantFill = inkPixels / (float)(cropW * cropH);
        float threshold = AreaPreservingThreshold(coverage, wantFill);

        HashSet<Vector2Int> mask = new HashSet<Vector2Int>();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (coverage[y * width + x] >= threshold) mask.Add(new Vector2Int(x, y));

        int beforeRepair = mask.Count;

        // Order matters. Thicken first: it can reconnect pieces, so removing islands before it
        // would throw away parts that were about to rejoin. Holes last but one, because thickening
        // closes some of them for free. Largest-component last, once the shape is whole.
        Thicken(mask, width, height);
        FillHoles(mask, width, height);
        int dropped = KeepLargestRegion(mask, width, height);

        if (mask.Count < 8)
        {
            report = $"only {mask.Count} cells survive at {across} across — too detailed to use";
            return null;
        }

        float churn = beforeRepair > 0 ? Mathf.Abs(mask.Count - beforeRepair) / (float)beforeRepair : 0f;

        report = $"{width}x{height}  {mask.Count} cells  fill {mask.Count / (float)(width * height):F2}"
               + $"  needs {required} across (asked {requestedCells})"
               + $"  t={threshold:F2}  repair churn {churn:P0}"
               + (dropped > 0 ? $"  dropped {dropped} stray cell(s)" : "");

        return mask;
    }

    // How many cells this shape needs before its thinnest limb survives. Lets a caller place a
    // shape at a level whose board is big enough, instead of discovering the blob afterwards.
    public static int RequiredCellsAcross(string imagePath)
    {
        bool[] solid;
        int tw, th;
        string error;
        if (!LoadSolid(imagePath, out solid, out tw, out th, out error)) return int.MaxValue;

        CloseGaps(solid, tw, th);

        int minX, maxX, minY, maxY;
        if (!InkBounds(solid, tw, th, out minX, out maxX, out minY, out maxY)) return int.MaxValue;

        int cropW = maxX - minX + 1;
        float halfWidth = FeatureHalfWidth(solid, tw, th, minX, maxX, minY, maxY);
        return Mathf.CeilToInt(cropW / Mathf.Max(0.5f, halfWidth * 2f / MinCellsPerFeature));
    }

    // ---------- source decoding ----------

    static bool LoadSolid(string path, out bool[] solid, out int tw, out int th, out string error)
    {
        solid = null; tw = th = 0; error = null;

        byte[] bytes;
        try { bytes = File.ReadAllBytes(path); }
        catch (System.Exception e) { error = "could not read: " + e.Message; return false; }

        Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(tex, bytes))
        {
            Object.DestroyImmediate(tex);
            error = "not a decodable PNG/JPG";
            return false;
        }

        tw = tex.width; th = tex.height;
        Color32[] px = tex.GetPixels32();
        Object.DestroyImmediate(tex);

        // A cutout is a shape on transparency. A fully opaque image is instead ink on a background,
        // and its ink is not reliably near-black, so Otsu finds the split rather than assuming 128.
        bool hasAlpha = false;
        for (int i = 0; i < px.Length; i += 7)
            if (px[i].a < 250) { hasAlpha = true; break; }

        solid = new bool[tw * th];

        if (hasAlpha)
        {
            for (int i = 0; i < px.Length; i++) solid[i] = px[i].a > 127;
        }
        else
        {
            byte[] lum = new byte[px.Length];
            for (int i = 0; i < px.Length; i++)
                lum[i] = (byte)Mathf.Clamp(px[i].r * 0.299f + px[i].g * 0.587f + px[i].b * 0.114f, 0f, 255f);

            int cut = Otsu(lum);
            for (int i = 0; i < lum.Length; i++) solid[i] = lum[i] < cut;
        }

        return true;
    }

    static int Otsu(byte[] lum)
    {
        int[] hist = new int[256];
        foreach (byte v in lum) hist[v]++;

        float total = lum.Length;
        float sum = 0f;
        for (int i = 0; i < 256; i++) sum += i * hist[i];

        float sumB = 0f, wB = 0f, best = -1f;
        int cut = 128;

        for (int t = 0; t < 256; t++)
        {
            wB += hist[t];
            if (wB == 0f) continue;

            float wF = total - wB;
            if (wF == 0f) break;

            sumB += t * hist[t];
            float mB = sumB / wB;
            float mF = (sum - sumB) / wF;
            float between = wB * wF * (mB - mF) * (mB - mF);

            if (between > best) { best = between; cut = t; }
        }

        return cut;
    }

    static bool InkBounds(bool[] solid, int tw, int th,
                          out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = tw; maxX = -1; minY = th; maxY = -1;

        for (int y = 0; y < th; y++)
        {
            for (int x = 0; x < tw; x++)
            {
                if (!solid[y * tw + x]) continue;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        return maxX >= 0;
    }

    // ---------- measurement ----------

    // Two-pass chamfer distance transform, then the chosen percentile over ink pixels. Exact
    // Euclidean would be tidier but a chamfer is within a few percent and this only feeds a
    // percentile, which rounds the difference away.
    static float FeatureHalfWidth(bool[] solid, int tw, int th,
                                  int minX, int maxX, int minY, int maxY)
    {
        int w = maxX - minX + 1, h = maxY - minY + 1;
        float[] d = new float[w * h];
        const float INF = 1e9f;
        const float D1 = 1f, D2 = 1.414f;

        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                d[y * w + x] = solid[(minY + y) * tw + (minX + x)] ? INF : 0f;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                if (d[i] == 0f) continue;

                float best = d[i];
                if (y > 0)
                {
                    best = Mathf.Min(best, d[i - w] + D1);
                    if (x > 0) best = Mathf.Min(best, d[i - w - 1] + D2);
                    if (x < w - 1) best = Mathf.Min(best, d[i - w + 1] + D2);
                }
                if (x > 0) best = Mathf.Min(best, d[i - 1] + D1);
                d[i] = best;
            }
        }

        for (int y = h - 1; y >= 0; y--)
        {
            for (int x = w - 1; x >= 0; x--)
            {
                int i = y * w + x;
                if (d[i] == 0f) continue;

                float best = d[i];
                if (y < h - 1)
                {
                    best = Mathf.Min(best, d[i + w] + D1);
                    if (x > 0) best = Mathf.Min(best, d[i + w - 1] + D2);
                    if (x < w - 1) best = Mathf.Min(best, d[i + w + 1] + D2);
                }
                if (x < w - 1) best = Mathf.Min(best, d[i + 1] + D1);
                d[i] = best;
            }
        }

        List<float> inside = new List<float>();
        for (int i = 0; i < d.Length; i++)
            if (d[i] > 0f && d[i] < INF) inside.Add(d[i]);

        if (inside.Count == 0) return 1f;

        inside.Sort();
        int at = Mathf.Clamp(Mathf.RoundToInt(inside.Count * WidthPercentile / 100f), 0, inside.Count - 1);
        return Mathf.Max(0.5f, inside[at]);
    }

    static float[] CoverageField(bool[] solid, int tw, int th,
                                 int minX, int minY, int cropW, int cropH,
                                 int width, int height)
    {
        float[] coverage = new float[width * height];

        for (int cy = 0; cy < height; cy++)
        {
            for (int cx = 0; cx < width; cx++)
            {
                int x0 = minX + Mathf.FloorToInt(cx * cropW / (float)width);
                int x1 = minX + Mathf.CeilToInt((cx + 1) * cropW / (float)width);
                int y0 = minY + Mathf.FloorToInt(cy * cropH / (float)height);
                int y1 = minY + Mathf.CeilToInt((cy + 1) * cropH / (float)height);

                int hits = 0, total = 0;
                for (int y = y0; y < Mathf.Min(y1, th); y++)
                {
                    for (int x = x0; x < Mathf.Min(x1, tw); x++)
                    {
                        total++;
                        if (solid[y * tw + x]) hits++;
                    }
                }

                coverage[cy * width + cx] = total > 0 ? hits / (float)total : 0f;
            }
        }

        return coverage;
    }

    static float AreaPreservingThreshold(float[] coverage, float wantFill)
    {
        float lo = CoverageMin, hi = CoverageMax, best = CoverageMax;
        float bestError = float.MaxValue;

        for (int step = 0; step <= 15; step++)
        {
            float t = Mathf.Lerp(lo, hi, step / 15f);

            int filled = 0;
            foreach (float c in coverage) if (c >= t) filled++;

            float error = Mathf.Abs(filled / (float)coverage.Length - wantFill);
            if (error < bestError) { bestError = error; best = t; }
        }

        return best;
    }

    // ---------- repair ----------

    // Bridge thin gaps in the SOURCE art before it is sampled. Real icon art is full of them — the
    // grooves around a chess piece's base, the slot between a handle and a body. A slit only a few
    // pixels wide still lands near the coverage threshold and deletes a whole row of cells.
    static void CloseGaps(bool[] solid, int w, int h)
    {
        int radius = Mathf.Max(1, Mathf.RoundToInt(Mathf.Min(w, h) * 0.02f));
        bool[] tmp = new bool[solid.Length];
        Grow(solid, tmp, w, h, radius, true);
        Grow(solid, tmp, w, h, radius, false);
    }

    static void Grow(bool[] src, bool[] tmp, int w, int h, int radius, bool dilate)
    {
        for (int pass = 0; pass < 2; pass++)
        {
            System.Array.Copy(src, tmp, src.Length);

            for (int y = 0; y < h; y++)
            {
                int row = y * w;
                for (int x = 0; x < w; x++)
                {
                    bool value = tmp[row + x];

                    for (int k = 1; k <= radius; k++)
                    {
                        int a, b;
                        if (pass == 0)
                        {
                            a = row + Mathf.Max(0, x - k);
                            b = row + Mathf.Min(w - 1, x + k);
                        }
                        else
                        {
                            a = Mathf.Max(0, y - k) * w + x;
                            b = Mathf.Min(h - 1, y + k) * w + x;
                        }

                        if (dilate) value = value || tmp[a] || tmp[b];
                        else value = value && tmp[a] && tmp[b];
                    }

                    src[row + x] = value;
                }
            }
        }
    }

    // A 2x2 close in grid space: nothing one cell wide survives it, and near-touching parts join.
    static void Thicken(HashSet<Vector2Int> mask, int width, int height)
    {
        HashSet<Vector2Int> grown = new HashSet<Vector2Int>(mask);
        foreach (Vector2Int c in mask)
        {
            grown.Add(new Vector2Int(c.x + 1, c.y));
            grown.Add(new Vector2Int(c.x, c.y + 1));
            grown.Add(new Vector2Int(c.x + 1, c.y + 1));
        }

        mask.Clear();
        foreach (Vector2Int c in grown)
        {
            if (c.x < 0 || c.y < 0 || c.x >= width || c.y >= height) continue;
            if (grown.Contains(new Vector2Int(c.x - 1, c.y))
                && grown.Contains(new Vector2Int(c.x, c.y - 1))
                && grown.Contains(new Vector2Int(c.x - 1, c.y - 1)))
                mask.Add(c);
        }
    }

    // Flood the background in from the border; whatever it never reaches is an interior hole.
    static void FillHoles(HashSet<Vector2Int> mask, int width, int height)
    {
        HashSet<Vector2Int> outside = new HashSet<Vector2Int>();
        Stack<Vector2Int> frontier = new Stack<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            Push(mask, outside, frontier, new Vector2Int(x, 0));
            Push(mask, outside, frontier, new Vector2Int(x, height - 1));
        }
        for (int y = 0; y < height; y++)
        {
            Push(mask, outside, frontier, new Vector2Int(0, y));
            Push(mask, outside, frontier, new Vector2Int(width - 1, y));
        }

        while (frontier.Count > 0)
        {
            Vector2Int c = frontier.Pop();
            foreach (Vector2Int d in Dirs)
            {
                Vector2Int n = c + d;
                if (n.x < 0 || n.y < 0 || n.x >= width || n.y >= height) continue;
                Push(mask, outside, frontier, n);
            }
        }

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Vector2Int c = new Vector2Int(x, y);
                if (!mask.Contains(c) && !outside.Contains(c)) mask.Add(c);
            }
    }

    static void Push(HashSet<Vector2Int> mask, HashSet<Vector2Int> outside,
                     Stack<Vector2Int> frontier, Vector2Int c)
    {
        if (mask.Contains(c) || outside.Contains(c)) return;
        outside.Add(c);
        frontier.Push(c);
    }

    static int KeepLargestRegion(HashSet<Vector2Int> mask, int width, int height)
    {
        HashSet<Vector2Int> unvisited = new HashSet<Vector2Int>(mask);
        List<List<Vector2Int>> regions = new List<List<Vector2Int>>();
        Stack<Vector2Int> frontier = new Stack<Vector2Int>();

        while (unvisited.Count > 0)
        {
            Vector2Int start = default;
            foreach (Vector2Int c in unvisited) { start = c; break; }

            List<Vector2Int> region = new List<Vector2Int>();
            frontier.Clear();
            frontier.Push(start);
            unvisited.Remove(start);

            while (frontier.Count > 0)
            {
                Vector2Int c = frontier.Pop();
                region.Add(c);

                foreach (Vector2Int d in Dirs)
                    if (unvisited.Remove(c + d)) frontier.Push(c + d);
            }

            regions.Add(region);
        }

        if (regions.Count <= 1) return 0;

        int biggest = 0;
        for (int i = 1; i < regions.Count; i++)
            if (regions[i].Count > regions[biggest].Count) biggest = i;

        int dropped = 0;
        for (int i = 0; i < regions.Count; i++)
        {
            if (i == biggest) continue;
            foreach (Vector2Int c in regions[i]) mask.Remove(c);
            dropped += regions[i].Count;
        }

        return dropped;
    }

    // ---------- preview ----------

    public static string RenderMask(HashSet<Vector2Int> mask, int width, int height)
    {
        StringBuilder sb = new StringBuilder();
        for (int y = height - 1; y >= 0; y--)   // top row first, so it reads like the screen
        {
            for (int x = 0; x < width; x++)
                sb.Append(mask.Contains(new Vector2Int(x, y)) ? '#' : '.');
            sb.Append('\n');
        }
        return sb.ToString();
    }

    [MenuItem("ArrowPuzzle/Campaign/Preview Shape Masks")]
    static void Preview()
    {
        string[] files = ListShapes();
        if (files.Length == 0)
        {
            Debug.LogWarning($"[Shapes] no images in {FolderPath}\n"
                             + "Create that folder next to Assets/ and drop PNG silhouettes in it.");
            return;
        }

        foreach (string f in files)
        {
            int w, h; string report;

            // 0 = let the shape choose its own size, which is the size the campaign will use.
            HashSet<Vector2Int> mask = Build(f, 0, out w, out h, out report);

            if (mask == null)
            {
                Debug.LogError($"[Shapes] {Path.GetFileName(f)}: {report}");
                continue;
            }

            Debug.Log($"[Shapes] {Path.GetFileName(f)} — {report}\n" + RenderMask(mask, w, h));
        }
    }
}

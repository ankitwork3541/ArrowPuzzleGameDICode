using UnityEngine;
using UnityEngine.UIElements;

// A confetti burst drawn INSIDE the UI panel, as one custom-generated mesh.
//
// It lives in UI Toolkit rather than in a ParticleSystem because of where the panel renders. The
// project's PanelSettings use Overlay mode, which draws after the camera has finished — so any
// world-space particle, built-in or VFX Graph, would fall BEHIND the win card instead of in front
// of it. The documented ways around that are rendering particles to a RenderTexture or moving the
// panel to World Space, and both are large changes to a UI that already works. Drawing the
// confetti as part of the panel's own geometry sidesteps the question entirely: there is no
// ordering problem when the confetti IS the UI.
//
// It also costs almost nothing. One VisualElement, one mesh, ~120 quads rebuilt per frame, no new
// package, and no compute shaders — which matters, because VFX Graph needs them and low-end
// Android devices do not all have them.
public class ConfettiView : VisualElement
{
    private struct Piece
    {
        public Vector2 pos;
        public float fallSpeed;
        public float swayAmp, swayFreq, swayPhase;
        public float spin, spinSpeed;      // in-plane rotation
        public float flip, flipSpeed;      // fakes tumbling by squashing the width
        public float halfW, halfH;
        public float delay;                // staggered entry, so it rains rather than dumps
        public Color color;
    }

    private readonly Piece[] pieces;
    private float elapsed;

    // How long after the last piece has been released before the burst is declared over. Pieces
    // are killed individually as they leave the bottom, so this only bounds the worst case.
    private const float FallOutTime = 4.5f;

    public bool IsRunning { get; private set; }

    public ConfettiView(int count = 120)
    {
        pieces = new Piece[count];

        pickingMode = PickingMode.Ignore;    // never eat a tap meant for the buttons underneath
        style.position = Position.Absolute;
        style.left = 0;
        style.top = 0;
        style.right = 0;
        style.bottom = 0;

        generateVisualContent += OnGenerateVisualContent;
    }

    // Seeded so a replay of the same level looks the same, and so the burst is reproducible when
    // something about it needs tuning.
    public void Burst(int seed)
    {
        System.Random rng = new System.Random(seed);
        float w = Mathf.Max(1f, resolvedStyle.width);

        for (int i = 0; i < pieces.Length; i++)
        {
            float r0 = (float)rng.NextDouble();
            float r1 = (float)rng.NextDouble();
            float r2 = (float)rng.NextDouble();
            float r3 = (float)rng.NextDouble();

            pieces[i] = new Piece
            {
                // Started above the top edge by a random amount, so the leading edge of the burst
                // is ragged instead of a single straight line of confetti crossing the screen.
                pos = new Vector2(r0 * w, -60f - r1 * 400f),
                fallSpeed = Mathf.Lerp(620f, 1150f, r2),
                swayAmp = Mathf.Lerp(18f, 70f, r3),
                swayFreq = Mathf.Lerp(1.1f, 2.6f, (float)rng.NextDouble()),
                swayPhase = (float)rng.NextDouble() * Mathf.PI * 2f,
                spin = (float)rng.NextDouble() * Mathf.PI * 2f,
                spinSpeed = Mathf.Lerp(-4.5f, 4.5f, (float)rng.NextDouble()),
                flip = (float)rng.NextDouble() * Mathf.PI * 2f,
                flipSpeed = Mathf.Lerp(3f, 8f, (float)rng.NextDouble()),
                halfW = Mathf.Lerp(9f, 15f, (float)rng.NextDouble()),
                halfH = Mathf.Lerp(14f, 22f, (float)rng.NextDouble()),
                delay = (float)rng.NextDouble() * 1.1f,
                // Straight from the arrows' palette, so the celebration is made of the same
                // colours the player just finished routing off the board.
                color = ArrowPalette.Colors[rng.Next(ArrowPalette.Colors.Length)],
            };
        }

        elapsed = 0f;
        IsRunning = true;
        MarkDirtyRepaint();
    }

    public void Stop()
    {
        IsRunning = false;
    }

    // Driven from the controller's Update() rather than a UI scheduler, so the step matches the
    // rendered frame exactly. A scheduler ticking on its own interval double-advances on a slow
    // frame and stutters on a fast one.
    public void Advance(float dt)
    {
        if (!IsRunning) return;

        elapsed += dt;
        float height = resolvedStyle.height;

        bool anyAlive = false;
        for (int i = 0; i < pieces.Length; i++)
        {
            if (elapsed < pieces[i].delay) { anyAlive = true; continue; }

            // Already gone past the bottom — leave it there and stop paying for it.
            if (pieces[i].pos.y > height + 80f) continue;

            pieces[i].pos.y += pieces[i].fallSpeed * dt;
            pieces[i].pos.x += Mathf.Cos(elapsed * pieces[i].swayFreq + pieces[i].swayPhase)
                               * pieces[i].swayAmp * dt;
            pieces[i].spin += pieces[i].spinSpeed * dt;
            pieces[i].flip += pieces[i].flipSpeed * dt;

            anyAlive = true;
        }

        // generateVisualContent is NOT called every frame by itself — Unity only re-runs it when
        // the element is marked dirty, which is what makes this an animation rather than a
        // one-off drawing.
        MarkDirtyRepaint();

        if (!anyAlive || elapsed > FallOutTime + 1.1f)
        {
            IsRunning = false;
            MarkDirtyRepaint();     // one last pass to clear the leftovers
        }
    }

    private void OnGenerateVisualContent(MeshGenerationContext mgc)
    {
        float height = resolvedStyle.height;
        if (height <= 0f) return;

        // Two passes: count first, because Allocate needs the exact vertex and index count up
        // front and cannot be resized afterwards.
        int visible = 0;
        for (int i = 0; i < pieces.Length; i++)
            if (IsVisible(i, height)) visible++;

        if (visible == 0) return;

        MeshWriteData mesh = mgc.Allocate(visible * 4, visible * 6);

        ushort v = 0;
        for (int i = 0; i < pieces.Length; i++)
        {
            if (!IsVisible(i, height)) continue;

            // The tumble: squashing the width by |cos| reads as a flat rectangle turning edge-on
            // and back. Cheaper and steadier than actually rotating a quad through 3D, and at
            // confetti size the difference is invisible.
            float hw = pieces[i].halfW * Mathf.Abs(Mathf.Cos(pieces[i].flip));
            float hh = pieces[i].halfH;

            float c = Mathf.Cos(pieces[i].spin);
            float s = Mathf.Sin(pieces[i].spin);
            Vector2 p = pieces[i].pos;
            Color32 tint = pieces[i].color;

            // Corner order and winding follow Unity's own mesh-generation sample: bottom-left,
            // top-left, top-right, bottom-right, then 0-1-2 / 2-3-0.
            AddVertex(mesh, p, -hw, hh, c, s, tint);
            AddVertex(mesh, p, -hw, -hh, c, s, tint);
            AddVertex(mesh, p, hw, -hh, c, s, tint);
            AddVertex(mesh, p, hw, hh, c, s, tint);

            mesh.SetNextIndex(v);
            mesh.SetNextIndex((ushort)(v + 1));
            mesh.SetNextIndex((ushort)(v + 2));
            mesh.SetNextIndex((ushort)(v + 2));
            mesh.SetNextIndex((ushort)(v + 3));
            mesh.SetNextIndex(v);
            v += 4;
        }
    }

    private static void AddVertex(MeshWriteData mesh, Vector2 centre, float x, float y,
                                  float cos, float sin, Color32 tint)
    {
        mesh.SetNextVertex(new Vertex
        {
            position = new Vector3(centre.x + x * cos - y * sin,
                                   centre.y + x * sin + y * cos,
                                   Vertex.nearZ),
            tint = tint,
        });
    }

    private bool IsVisible(int i, float height)
    {
        return elapsed >= pieces[i].delay
               && pieces[i].pos.y > -120f
               && pieces[i].pos.y < height + 80f;
    }
}

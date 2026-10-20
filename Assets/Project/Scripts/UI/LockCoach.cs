using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// The one-time "locked dragon" coach overlay on the first padlock level (when it shows: LockTutorialRule).
// The elements live in PlayScreen.uxml (`lock-coach`); this only drives them: a ghost padlock and a pulsing
// ring drawn over the real padlock (the real one is a world object under the UI, so the scrim would dim it),
// a card with the rule in words, and a small "N > ... > 1 > OPEN" row. With Reduce Motion on, nothing pulses
// or cycles. GOT IT hides it and saves the "seen" flag; the overlay swallows every tap, so no dragon moves.
public class LockCoach
{
    private const float PlaceEverySeconds = 0.05f;
    private const long PulseMs = 650;
    private const long StepMs = 750;

    private readonly VisualElement coach;
    private readonly VisualElement ring;
    private readonly VisualElement ghost;
    private readonly VisualElement card;
    private readonly VisualElement steps;
    private readonly Label ghostCount;
    private readonly Label body;
    private readonly Button ok;

    private readonly List<VisualElement> stepElements = new List<VisualElement>();
    private IVisualElementScheduledItem placeTick;
    private IVisualElementScheduledItem animTick;
    private DragonController target;
    private int litStep;

    // True while the overlay is up. PlayScreenController uses it to keep taps and pans off the board.
    public bool Visible { get; private set; }

    public LockCoach(VisualElement root)
    {
        coach = root.Q<VisualElement>("lock-coach");
        if (coach == null) return;

        ring = coach.Q<VisualElement>("coach-ring");
        ghost = coach.Q<VisualElement>("coach-ghost");
        ghostCount = coach.Q<Label>("coach-ghost-count");
        card = coach.Q<VisualElement>("coach-card");
        body = coach.Q<Label>("coach-body");
        steps = coach.Q<VisualElement>("coach-steps");
        ok = coach.Q<Button>("coach-ok");

        // Never let a press fall through to the elements under the overlay.
        coach.RegisterCallback<PointerDownEvent>(evt => evt.StopPropagation());
        if (ok != null) ok.clicked += Dismiss;
    }

    public void Show(DragonController dragon)
    {
        if (coach == null || dragon == null || !dragon.IsLocked) return;

        target = dragon;
        int remaining = dragon.lockRemaining;

        body.text = LockTutorialRule.BodyText(remaining);
        ghostCount.text = remaining.ToString();
        BuildSteps(remaining);

        coach.RemoveFromClassList("coach--hidden");
        Visible = true;
        litStep = 0;
        Place();

        placeTick?.Pause();
        placeTick = coach.schedule.Execute(Place).Every((long)(PlaceEverySeconds * 1000f));

        animTick?.Pause();
        ApplyMotion();
    }

    // Hides without saving anything: a level restart or a new entrance must not count as "seen".
    public void Hide()
    {
        if (coach == null) return;

        placeTick?.Pause();
        animTick?.Pause();
        coach.AddToClassList("coach--hidden");
        ring?.RemoveFromClassList("coach__ring--big");
        Visible = false;
        target = null;
    }

    // GOT IT: save the "seen" flag and hide. Public so the dev-build evidence run can press it.
    public void Dismiss()
    {
        ProgressStore.Data.lockTutorialSeen = true;
        ProgressStore.Save();
        Hide();
    }

    // ---- placement ----

    // Puts the ghost and the ring over the real padlock (the camera may be panning) and keeps the card on
    // the side of the screen the padlock is not on.
    private void Place()
    {
        if (!Visible) return;

        Vector3 worldPos;
        float worldSize;
        if (target == null || !target.TryGetLockBadge(out worldPos, out worldSize))
        {
            Hide();
            return;
        }

        Camera cam = Camera.main;
        IPanel panel = coach.panel;
        if (cam == null || panel == null) return;

        Vector2 centre = RuntimePanelUtils.CameraTransformWorldToPanel(panel, worldPos, cam);
        Vector2 edge = RuntimePanelUtils.CameraTransformWorldToPanel(panel, worldPos + cam.transform.right * (worldSize * 0.5f), cam);
        float side = Mathf.Max(48f, Vector2.Distance(centre, edge) * 2f);

        SetBox(ghost, centre, side);
        SetBox(ring, centre, side * 1.28f);

        bool digitsTwo = ghostCount.text.Length > 1;
        ghostCount.style.fontSize = side * (digitsTwo ? 0.30f : 0.38f);

        // UI Toolkit's y grows downward, same as the panel coordinates above. The card goes on the side the
        // padlock is not on and starts just past the ring, so it can never cover the padlock.
        float panelHeight = panel.visualTree.layout.height;
        bool padlockLow = centre.y > panelHeight * 0.5f;
        card.EnableInClassList("coach__card--top", padlockLow);
        card.EnableInClassList("coach__card--bottom", !padlockLow);

        const float Gap = 28f;
        float ringHalf = side * 1.28f * 0.5f * 1.14f;   // the ring's biggest pulse
        if (padlockLow)
        {
            card.style.top = StyleKeyword.Auto;
            card.style.bottom = panelHeight - (centre.y - ringHalf) + Gap;
        }
        else
        {
            card.style.bottom = StyleKeyword.Auto;
            card.style.top = centre.y + ringHalf + Gap;
        }
    }

    private static void SetBox(VisualElement element, Vector2 centre, float side)
    {
        element.style.left = centre.x - side * 0.5f;
        element.style.top = centre.y - side * 0.5f;
        element.style.width = side;
        element.style.height = side;
    }

    // ---- the illustrative row ----

    // N > ... > 1 > OPEN, shortened when the count is small.
    private void BuildSteps(int remaining)
    {
        steps.Clear();
        stepElements.Clear();

        if (remaining >= 2)
        {
            AddStep(MiniLock(remaining));
            AddArrow();
        }

        if (remaining >= 3)
        {
            Label dots = new Label("...");
            dots.AddToClassList("coach__dots");
            dots.pickingMode = PickingMode.Ignore;
            AddStep(dots);
            AddArrow();
        }

        AddStep(MiniLock(1));
        AddArrow();
        AddStep(OpenPill());
    }

    private void AddStep(VisualElement content)
    {
        VisualElement step = new VisualElement();
        step.AddToClassList("coach__step");
        step.pickingMode = PickingMode.Ignore;
        step.Add(content);
        steps.Add(step);
        stepElements.Add(step);
    }

    private void AddArrow()
    {
        Label arrow = new Label(">");
        arrow.AddToClassList("coach__arrow");
        arrow.pickingMode = PickingMode.Ignore;
        steps.Add(arrow);
    }

    private static VisualElement MiniLock(int count)
    {
        VisualElement mini = new VisualElement();
        mini.AddToClassList("coach__mini");
        mini.pickingMode = PickingMode.Ignore;

        Label label = new Label(count.ToString());
        label.AddToClassList("coach__mini-count");
        label.pickingMode = PickingMode.Ignore;
        mini.Add(label);
        return mini;
    }

    private static VisualElement OpenPill()
    {
        VisualElement pill = new VisualElement();
        pill.AddToClassList("coach__open");
        pill.pickingMode = PickingMode.Ignore;

        Label label = new Label("OPEN");
        label.AddToClassList("coach__open-label");
        label.pickingMode = PickingMode.Ignore;
        pill.Add(label);
        return pill;
    }

    // ---- motion (off with Reduce Motion) ----

    private void ApplyMotion()
    {
        bool still = OptionsManager.ReduceMotionOn;

        foreach (VisualElement step in stepElements) step.EnableInClassList("coach__step--dim", false);

        if (still)
        {
            ring.RemoveFromClassList("coach__ring--big");
            return;
        }

        // One scheduled tick drives both the ring pulse and the step highlight.
        ring.AddToClassList("coach__ring--big");
        animTick = coach.schedule.Execute(AnimateStep).Every(StepMs);
        coach.schedule.Execute(PulseRing).Every(PulseMs).Until(() => !Visible);
        AnimateStep();
    }

    private void PulseRing()
    {
        if (!Visible || ring == null) return;

        bool big = ring.ClassListContains("coach__ring--big");
        ring.EnableInClassList("coach__ring--big", !big);
    }

    private void AnimateStep()
    {
        if (!Visible || stepElements.Count == 0) return;

        for (int i = 0; i < stepElements.Count; i++)
            stepElements[i].EnableInClassList("coach__step--dim", i > litStep);

        litStep = (litStep + 1) % (stepElements.Count + 1);
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// The level map on Home: a winding path of level nodes, newest level at the top, level 1 at the
// bottom, scrolled to the player's current level when it opens.
//
// A plain class, not a MonoBehaviour: it only exists inside Home's document, is built by
// HallScreenController when that document loads, and needs no scene wiring.
//
// Virtualised: a ListView with a fixed row height recycles a handful of MapRow elements however
// many levels there are. The 1,500 entries in itemsSource are plain ints; no VisualElement exists
// for a level until it scrolls into view.
public class TrailMapController
{
    private const float RowHeight = 210f;
    private const float NavClearance = 80f;

    // How many locked levels past the frontier the map shows. The map is a window onto the player's
    // progress, not the whole campaign: with every level listed, one fling carried the view thousands
    // of pixels up into locked levels and the beaten ones were out of reach. Also keeps the campaign
    // length off the screen.
    private const int LookaheadLevels = 12;

    // Lower than ScrollView's 0.135 default: a flick stops sooner, so the map stays controllable.
    private const float DecelerationRate = 0.05f;

    private readonly VisualElement slot;
    private readonly VisualElement homeContent;
    private readonly ListView list;
    private readonly Label progressLabel;
    private readonly StageLoader levelLoader;
    private readonly Action<int> onLevelChosen;

    // How long to keep trying to scroll to the current level after the map opens: 20 tries 50 ms
    // apart. A slow first layout can take several frames; past this the map is simply left at the top.
    private const int MaxScrollAttempts = 20;
    private const long ScrollRetryMs = 50;

    private readonly List<int> rows = new List<int>();
    private int total;
    private int shown;
    private int highest;

    private bool scrollPending;
    private int scrollAttempts;
    private IVisualElementScheduledItem scrollRetry;

    public bool IsOpen { get; private set; }

    // `slot` is the template instance holding the map, `homeContent` the Home column it replaces
    // while open. Both are toggled with classes defined in HomeScreen.uss.
    public TrailMapController(VisualElement slot, VisualElement homeContent, StageLoader levelLoader,
                              Action<int> onLevelChosen)
    {
        this.slot = slot;
        this.homeContent = homeContent;
        this.levelLoader = levelLoader;
        this.onLevelChosen = onLevelChosen;

        progressLabel = slot.Q<Label>("trail-progress");

        Button back = slot.Q<Button>("trail-return-button");
        TapSound.Attach(back);
        back.clicked += Close;

        list = slot.Q<ListView>("trail-list");
        list.fixedItemHeight = RowHeight;
        list.virtualizationMethod = CollectionVirtualizationMethod.FixedHeight;
        list.selectionType = SelectionType.None;
        list.horizontalScrollingEnabled = false;
        list.makeItem = () => new MapRow(OnNodeClicked);
        list.bindItem = (element, index) => ((MapRow)element).Bind(shown - index, shown, highest);
        list.itemsSource = rows;

        // The scroll bar is noise on a touch map; dragging and the wheel still work without it.
        ScrollView scroll = list.Q<ScrollView>();
        if (scroll != null)
        {
            scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
            scroll.touchScrollBehavior = ScrollView.TouchScrollBehavior.Elastic;
            scroll.scrollDecelerationRate = DecelerationRate;
        }
    }

    public void Open()
    {
        IsOpen = true;
        Refresh();
        homeContent.AddToClassList("stage-body--hidden");
        slot.AddToClassList("trail-slot--open");

        // Not scrollable until the list has been laid out, which has not happened yet if this is
        // the first time the map is shown, and on a slow device that can take longer than any single
        // fixed delay. So: try on every layout change AND on a short timer, until one of them lands
        // (or MaxScrollAttempts runs out).
        scrollPending = true;
        scrollAttempts = 0;
        list.RegisterCallback<GeometryChangedEvent>(OnListGeometryChanged);
        scrollRetry?.Pause();
        scrollRetry = list.schedule.Execute(TryScrollToCurrent).Every(ScrollRetryMs).Until(() => !scrollPending);
    }

    public void Close()
    {
        FinishScroll();
        IsOpen = false;
        slot.RemoveFromClassList("trail-slot--open");
        homeContent.RemoveFromClassList("stage-body--hidden");
    }

    // Re-reads progress. Cheap to call whenever Home becomes visible: a beaten level moves the
    // frontier, and the rows already on screen need rebinding to show it.
    public void Refresh()
    {
        total = Mathf.Max(0, levelLoader.TotalLevels);
        highest = Mathf.Clamp(ProgressStore.Data.highestLevelCompleted, 0, total);

        // Only the frontier plus a few locked levels are listed (see LookaheadLevels).
        shown = Mathf.Min(total, highest + 1 + LookaheadLevels);

        // One extra row past level 1: an empty spacer that lets the lowest node scroll up clear of
        // the bottom nav, which is drawn over the map. Its level is 0, so it draws nothing.
        if (rows.Count != shown + 1)
        {
            rows.Clear();
            for (int i = 0; i <= shown; i++) rows.Add(i);
        }

        progressLabel.text = highest + " CLEARED";
        list.Rebuild();
    }

    // The frontier is the first level not yet beaten; once the whole campaign is done it is the
    // last one.
    private int CurrentLevel => Mathf.Clamp(highest + 1, 1, Mathf.Max(1, total));

    private void OnListGeometryChanged(GeometryChangedEvent evt)
    {
        TryScrollToCurrent();
    }

    // One attempt. Done as soon as the scroll lands, or when the attempts run out.
    private void TryScrollToCurrent()
    {
        if (!scrollPending) return;

        scrollAttempts++;
        if (ScrollToCurrent() || scrollAttempts >= MaxScrollAttempts) FinishScroll();
    }

    private void FinishScroll()
    {
        scrollPending = false;
        list.UnregisterCallback<GeometryChangedEvent>(OnListGeometryChanged);
        scrollRetry?.Pause();
    }

    // Returns true when there is nothing left to do (scrolled, or nothing to scroll), false when the
    // layout is not ready yet and the caller should try again.
    private bool ScrollToCurrent()
    {
        if (shown == 0) return true;

        // ListView owns its ScrollView; offset lives on that, not on the list itself.
        ScrollView scroll = list.Q<ScrollView>();
        if (scroll == null) return true;

        float viewport = list.contentRect.height;
        if (float.IsNaN(viewport) || viewport <= 0f) return false;

        // The content has to be taller than the view before an offset can stick; until the rows are
        // laid out the scroll view would clamp any offset back to the top.
        float content = scroll.contentContainer.layout.height;
        if (float.IsNaN(content) || content <= viewport) return false;

        // Level n sits in row (shown - n); put that row's centre in the middle of the view.
        int row = shown - CurrentLevel;
        // Aim a little above the true centre so the current node and the two below it sit clear of
        // the floating nav dock under the list.
        float centred = Mathf.Max(0f, row * RowHeight + RowHeight * 0.5f - viewport * 0.5f + NavClearance * 0.5f);
        scroll.scrollOffset = new Vector2(0f, centred);

        // Landed only if the offset stuck. Near the bottom of the list the target can exceed the
        // maximum scroll, in which case the retries simply stop at MaxScrollAttempts.
        return Mathf.Abs(scroll.scrollOffset.y - centred) < 2f;
    }

    private void OnNodeClicked(int level)
    {
        // Beaten levels and the frontier open; anything beyond it is locked. A locked node gets the plain
        // UI click so the tap is still acknowledged.
        if (level < 1 || level > highest + 1 || level > total)
        {
            if (SoundDirector.Instance != null) SoundDirector.Instance.PlayUIClick();
            return;
        }

        if (SoundDirector.Instance != null) SoundDirector.Instance.PlayMapNodeTap();
        Close();
        onLevelChosen(level);
    }

    // One level per row. The row draws its own slice of the path (a curve in from the level above
    // and out to the level below) and carries the node on top, so a recycled row never needs to
    // know about its neighbours.
    private class MapRow : VisualElement
    {
        private const float PathWidth = 18f;
        private static readonly Color DoneColor = new Color(0.910f, 0.549f, 0f);
        private static readonly Color TodoColor = new Color(0.227f, 0.141f, 0.094f, 0.18f);

        private readonly Button node;
        private readonly Label number;
        private int level;
        private int total;
        private int highest;

        public MapRow(Action<int> onClick)
        {
            AddToClassList("trail-row");
            generateVisualContent += DrawPath;
            RegisterCallback<GeometryChangedEvent>(_ => MarkDirtyRepaint());

            node = new Button();
            node.AddToClassList("trail-node");
            // No TapSound here: OnNodeClicked plays the map-node tick (or the plain click for a locked node).
            node.clicked += () => onClick(level);

            number = new Label();
            number.AddToClassList("trail-node__number");
            number.pickingMode = PickingMode.Ignore;

            VisualElement star = new VisualElement();
            star.AddToClassList("trail-node__star");
            star.pickingMode = PickingMode.Ignore;

            VisualElement lockIcon = new VisualElement();
            lockIcon.AddToClassList("trail-node__lock");
            lockIcon.pickingMode = PickingMode.Ignore;

            node.Add(number);
            node.Add(star);
            node.Add(lockIcon);
            Add(node);
        }

        public void Bind(int level, int total, int highest)
        {
            this.level = level;
            this.total = total;
            this.highest = highest;

            node.style.display = level < 1 ? DisplayStyle.None : DisplayStyle.Flex;
            number.text = level.ToString();
            node.style.left = new Length(PathX(level) * 100f, LengthUnit.Percent);

            bool done = level <= highest;
            bool current = level == highest + 1;
            node.EnableInClassList("trail-node--done", done);
            node.EnableInClassList("trail-node--current", current);
            node.EnableInClassList("trail-node--locked", !done && !current);

            MarkDirtyRepaint();
        }

        // Where the node of a level sits across the row, 0..1. A sine of the level number gives the
        // winding path; amplitude 0.27 keeps the widest node clear of both screen edges.
        private static float PathX(int level)
        {
            return 0.5f + 0.27f * Mathf.Sin(level * 0.9f);
        }

        private static float Mid(int a, int b)
        {
            return (PathX(a) + PathX(b)) * 0.5f;
        }

        private void DrawPath(MeshGenerationContext mgc)
        {
            if (level < 1) return;

            Rect rect = contentRect;
            if (rect.width <= 0f || rect.height <= 0f) return;

            Painter2D painter = mgc.painter2D;
            painter.lineWidth = PathWidth;
            painter.lineCap = LineCap.Round;
            painter.lineJoin = LineJoin.Round;

            float w = rect.width;
            float h = rect.height;
            Vector2 centre = new Vector2(PathX(level) * w, h * 0.5f);

            // Both halves meet their neighbour's row at the x of Mid(), heading straight up or
            // down, so the curve is continuous across the seam between two recycled rows.
            if (level < total)
            {
                Vector2 top = new Vector2(Mid(level, level + 1) * w, 0f);
                Curve(painter, top, centre, level + 1 <= highest);
            }

            if (level > 1)
            {
                Vector2 bottom = new Vector2(Mid(level - 1, level) * w, h);
                Curve(painter, centre, bottom, level <= highest);
            }
        }

        private static void Curve(Painter2D painter, Vector2 from, Vector2 to, bool done)
        {
            float midY = (from.y + to.y) * 0.5f;

            painter.strokeColor = done ? DoneColor : TodoColor;
            painter.BeginPath();
            painter.MoveTo(from);
            painter.BezierCurveTo(new Vector2(from.x, midY), new Vector2(to.x, midY), to);
            painter.Stroke();
        }
    }
}

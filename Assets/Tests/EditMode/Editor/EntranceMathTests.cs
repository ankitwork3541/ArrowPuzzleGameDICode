using NUnit.Framework;
using UnityEngine;

// The level-entrance rules (EntranceMath) and the `ui.entrance_style` config switch. All pure numbers: the
// ordering, the timing budget, the shape of each style's curve and the skip state are checked here; how it
// looks is judged from the Windows player frames (production/qa/evidence/entrance).
public class EntranceMathTests
{
    const float Eps = 1e-4f;

    [SetUp]
    public void SetUp()
    {
        GameConfigService.ResetToDefaults();
    }

    [TearDown]
    public void TearDown()
    {
        GameConfigService.ResetToDefaults();
    }

    // ---------- style key ----------

    [TestCase("ink", EntranceStyle.Ink)]
    [TestCase("fly", EntranceStyle.Fly)]
    [TestCase("classic", EntranceStyle.Classic)]
    [TestCase("FLY", EntranceStyle.Fly)]
    [TestCase("  Ink  ", EntranceStyle.Ink)]
    public void ParseStyle_KnownKey_GivesThatStyle(string key, EntranceStyle expected)
    {
        Assert.AreEqual(expected, EntranceMath.ParseStyle(key));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("spin")]
    [TestCase("ink fly")]
    public void ParseStyle_UnknownOrEmptyKey_FallsBackToInk(string key)
    {
        Assert.AreEqual(EntranceStyle.Ink, EntranceMath.ParseStyle(key));
        Assert.AreEqual(EntranceMath.DefaultStyle, EntranceMath.ParseStyle(key));
    }

    [Test]
    public void StyleKey_RoundTripsEveryStyle()
    {
        foreach (EntranceStyle style in new[] { EntranceStyle.Classic, EntranceStyle.Ink, EntranceStyle.Fly })
            Assert.AreEqual(style, EntranceMath.ParseStyle(EntranceMath.StyleKey(style)));
    }

    // ---------- config ----------

    [Test]
    public void Config_Defaults_UseInk()
    {
        Assert.AreEqual("ink", GameConfig.Defaults().ui.entrance_style);
        Assert.AreEqual(EntranceStyle.Ink, GameConfigService.EntranceStyle);
    }

    [TestCase("fly", "fly", EntranceStyle.Fly)]
    [TestCase("classic", "classic", EntranceStyle.Classic)]
    [TestCase("Fly ", "fly", EntranceStyle.Fly)]
    [TestCase("spin", "ink", EntranceStyle.Ink)]
    public void Config_Parse_NormalisesTheStyle(string raw, string expectedKey, EntranceStyle expected)
    {
        string json = "{ \"code\": 1, \"config_version\": 2, \"ui\": { \"entrance_style\": \"" + raw + "\" } }";

        Assert.IsTrue(GameConfigParser.TryParse(json, out GameConfig c, out string reason), reason);
        Assert.AreEqual(expectedKey, c.ui.entrance_style);

        GameConfigService.Set(c);
        Assert.AreEqual(expected, GameConfigService.EntranceStyle);
    }

    [Test]
    public void Config_ParseWithoutTheKey_KeepsInk()
    {
        Assert.IsTrue(GameConfigParser.TryParse("{ \"code\": 1, \"config_version\": 2, \"ui\": { \"confetti\": false } }",
            out GameConfig c, out string reason), reason);
        Assert.AreEqual("ink", c.ui.entrance_style);
    }

    // ---------- timing ----------

    [Test]
    public void StaggerWindow_SmallBoard_IsTheSmallWindowAndTotalsAboutNineTenths()
    {
        Assert.AreEqual(EntranceMath.StaggerSmall, EntranceMath.StaggerWindow(1), Eps);
        Assert.AreEqual(EntranceMath.StaggerSmall, EntranceMath.StaggerWindow(EntranceMath.SmallBoardDragons), Eps);
        Assert.AreEqual(0.9f, EntranceMath.TotalSeconds(10), 0.001f);
    }

    [Test]
    public void StaggerWindow_BiggestBoards_StayUnderTwelveTenths()
    {
        Assert.AreEqual(EntranceMath.StaggerBig, EntranceMath.StaggerWindow(EntranceMath.BigBoardDragons), Eps);
        Assert.AreEqual(EntranceMath.StaggerBig, EntranceMath.StaggerWindow(338), Eps);   // far past the cap
        Assert.LessOrEqual(EntranceMath.TotalSeconds(338), 1.2f + Eps);
        Assert.LessOrEqual(EntranceMath.TotalSeconds(10000), 1.2f + Eps);
    }

    [Test]
    public void StaggerWindow_GrowsWithTheBoard_NeverShrinks()
    {
        float previous = 0f;
        for (int count = 1; count <= 400; count++)
        {
            float window = EntranceMath.StaggerWindow(count);
            Assert.GreaterOrEqual(window, previous - Eps, "count " + count);
            previous = window;
        }
    }

    [Test]
    public void TotalSeconds_NoDragons_IsZero()
    {
        Assert.AreEqual(0f, EntranceMath.TotalSeconds(0), Eps);
    }

    [Test]
    public void DelayFor_FirstStartsAtOnceAndLastAtTheWindowEnd()
    {
        Assert.AreEqual(0f, EntranceMath.DelayFor(0, 50, 0.6f), Eps);
        Assert.AreEqual(0.6f, EntranceMath.DelayFor(49, 50, 0.6f), Eps);
        Assert.AreEqual(0f, EntranceMath.DelayFor(0, 1, 0.6f), Eps);   // a single dragon has no stagger
    }

    [Test]
    public void LocalProgress_ClampsBeforeAndAfterTheDragonsTurn()
    {
        Assert.AreEqual(0f, EntranceMath.LocalProgress(0.1f, 0.3f, 0.45f), Eps);
        Assert.AreEqual(0.5f, EntranceMath.LocalProgress(0.525f, 0.3f, 0.45f), Eps);
        Assert.AreEqual(1f, EntranceMath.LocalProgress(5f, 0.3f, 0.45f), Eps);
    }

    // ---------- ordering ----------

    [Test]
    public void CenterOutOrder_SortsByDistanceFromTheCentre()
    {
        Vector2[] centres =
        {
            new Vector2(3f, 0f),   // 9
            new Vector2(0f, 0f),   // 0
            new Vector2(1f, 1f),   // 2
            new Vector2(0f, 1f)    // 1
        };

        CollectionAssert.AreEqual(new[] { 1, 3, 2, 0 }, EntranceMath.CenterOutOrder(centres));
    }

    [Test]
    public void CenterOutOrder_EqualDistances_KeepIndexOrder()
    {
        Vector2[] centres =
        {
            new Vector2(1f, 0f),
            new Vector2(0f, 1f),
            new Vector2(-1f, 0f),
            new Vector2(0f, -1f)
        };

        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, EntranceMath.CenterOutOrder(centres));
    }

    [Test]
    public void NearestEdgeFirstOrder_SmallestDistanceFirst_TiesByIndex()
    {
        CollectionAssert.AreEqual(new[] { 1, 2, 0, 3 }, EntranceMath.NearestEdgeFirstOrder(new[] { 5f, 2f, 2f, 9f }));
    }

    [Test]
    public void Orders_EmptyInput_GiveEmptyOrders()
    {
        Assert.AreEqual(0, EntranceMath.CenterOutOrder(new Vector2[0]).Length);
        Assert.AreEqual(0, EntranceMath.NearestEdgeFirstOrder(new float[0]).Length);
    }

    [Test]
    public void Orders_ArePermutationsOfEveryIndex()
    {
        float[] keys = new float[60];
        Vector2[] centres = new Vector2[60];
        for (int i = 0; i < 60; i++)
        {
            keys[i] = (i * 37) % 11;
            centres[i] = new Vector2((i * 7) % 13, (i * 5) % 9);
        }

        foreach (int[] order in new[] { EntranceMath.NearestEdgeFirstOrder(keys), EntranceMath.CenterOutOrder(centres) })
        {
            bool[] seen = new bool[60];
            foreach (int index in order) seen[index] = true;
            foreach (bool one in seen) Assert.IsTrue(one);
        }
    }

    // ---------- ink ----------

    [Test]
    public void InkStroke_StartsAtNothing_FinishesTheBodyAtTheDrawEnd_AndNeverGoesBackwards()
    {
        Assert.AreEqual(0f, EntranceMath.InkStroke(0f), Eps);
        Assert.AreEqual(1f, EntranceMath.InkStroke(EntranceMath.InkDrawEnd), Eps);
        Assert.AreEqual(1f, EntranceMath.InkStroke(1f), Eps);

        float previous = 0f;
        for (float t = 0f; t <= 1f; t += 0.01f)
        {
            float stroke = EntranceMath.InkStroke(t);
            Assert.GreaterOrEqual(stroke, previous - Eps, "t " + t);
            previous = stroke;
        }
    }

    [Test]
    public void InkWet_IsFullWhileTheBrushMoves_ThenDriesToNothing()
    {
        Assert.AreEqual(1f, EntranceMath.InkWet(0f), Eps);
        Assert.AreEqual(1f, EntranceMath.InkWet(EntranceMath.InkDrawEnd), Eps);
        Assert.AreEqual(0f, EntranceMath.InkWet(1f), Eps);
        Assert.Less(EntranceMath.InkWet(0.9f), 1f);
        Assert.Greater(EntranceMath.InkWet(0.9f), 0f);
    }

    [Test]
    public void InkPop_IsExactlyOneOutsideThePop_AndPeaksAtOnePointOneFive()
    {
        Assert.AreEqual(1f, EntranceMath.InkPop(0f), Eps);
        Assert.AreEqual(1f, EntranceMath.InkPop(EntranceMath.InkDrawEnd), Eps);
        Assert.AreEqual(1f, EntranceMath.InkPop(1f), Eps);

        float mid = (EntranceMath.InkDrawEnd + 1f) * 0.5f;
        Assert.AreEqual(1.15f, EntranceMath.InkPop(mid), Eps);

        for (float t = 0f; t <= 1f; t += 0.01f)
            Assert.LessOrEqual(EntranceMath.InkPop(t), 1.15f + Eps);
    }

    [Test]
    public void PaperWave_CentreStartsAtOnce_EdgeStartsLast_AndEveryDotEndsBeforeTheEntranceDoes()
    {
        float window = EntranceMath.StaggerWindow(100);

        Assert.AreEqual(0f, EntranceMath.PaperStart(0f, window), Eps);
        Assert.Greater(EntranceMath.PaperStart(1f, window), EntranceMath.PaperStart(0.5f, window));

        // Nothing before a dot's start, nothing after its flash.
        Assert.AreEqual(0f, EntranceMath.PaperWaveAlpha(0.2f, 1f, window), Eps);
        Assert.AreEqual(0f, EntranceMath.PaperWaveAlpha(window + 1f, 0.3f, window), Eps);

        // A flash rises to the cap and no higher.
        float peak = 0f;
        for (float e = 0f; e <= window + EntranceMath.PerDragonSeconds; e += 0.005f)
            peak = Mathf.Max(peak, EntranceMath.PaperWaveAlpha(e, 0.5f, window));
        Assert.Greater(peak, 0.5f * EntranceMath.PaperMaxAlpha);
        Assert.LessOrEqual(peak, EntranceMath.PaperMaxAlpha + Eps);

        Assert.Less(EntranceMath.PaperEnd(window), EntranceMath.TotalSeconds(100));
    }

    // ---------- fly ----------

    [Test]
    public void FlyStartCells_PutsTheWholeDragonJustOutsideTheBoard_ForEveryHeading()
    {
        Vector2Int boardMin = new Vector2Int(0, 0);
        Vector2Int boardMax = new Vector2Int(9, 7);
        Vector2Int dragonMin = new Vector2Int(3, 2);
        Vector2Int dragonMax = new Vector2Int(5, 4);

        // Heading +x: it arrives from the left, so its right end starts one cell left of the board.
        float right = EntranceMath.FlyStartCells(Vector2Int.right, dragonMin, dragonMax, boardMin, boardMax);
        Assert.AreEqual(boardMin.x - 1, dragonMax.x - right, Eps);

        float left = EntranceMath.FlyStartCells(Vector2Int.left, dragonMin, dragonMax, boardMin, boardMax);
        Assert.AreEqual(boardMax.x + 1, dragonMin.x + left, Eps);

        float up = EntranceMath.FlyStartCells(Vector2Int.up, dragonMin, dragonMax, boardMin, boardMax);
        Assert.AreEqual(boardMin.y - 1, dragonMax.y - up, Eps);

        float down = EntranceMath.FlyStartCells(Vector2Int.down, dragonMin, dragonMax, boardMin, boardMax);
        Assert.AreEqual(boardMax.y + 1, dragonMin.y + down, Eps);
    }

    [Test]
    public void FlyOffset_StartsBehindTheBoard_AndSettlesOnItsSpot()
    {
        Assert.AreEqual(-12f, EntranceMath.FlyOffset(0f, 12f, 0.1f), Eps);
        Assert.AreEqual(0f, EntranceMath.FlyOffset(EntranceMath.FlySlideEnd, 12f, 0.1f), Eps);
        Assert.AreEqual(0f, EntranceMath.FlyOffset(1f, 12f, 0.1f), Eps);
    }

    [Test]
    public void FlyOffset_NeverOvershootsByMoreThanTheGivenAmount_AndNeverGoesBeyondTheStart()
    {
        const float overshoot = 0.1f;   // the brief's limit is 0.12 cells

        float peak = float.MinValue;
        for (float t = 0f; t <= 1f; t += 0.005f)
        {
            float offset = EntranceMath.FlyOffset(t, 12f, overshoot);
            peak = Mathf.Max(peak, offset);
            Assert.GreaterOrEqual(offset, -12f - Eps, "t " + t);
        }

        Assert.LessOrEqual(peak, 0.12f);
        Assert.Greater(peak, 0f);                      // there is an overshoot, not a plain stop
        Assert.AreEqual(overshoot, peak, 0.001f);
    }

    [Test]
    public void FlyOffset_ApproachesTheSpotWithoutGoingBackwardsBeforeTheOvershoot()
    {
        float previous = float.MinValue;
        for (float t = 0f; t <= 0.6f; t += 0.01f)
        {
            float offset = EntranceMath.FlyOffset(t, 20f, 0.1f);
            Assert.GreaterOrEqual(offset, previous - Eps, "t " + t);
            previous = offset;
        }
    }

    [Test]
    public void FlyBadgeScale_IsHiddenUntilTheDragonLands_ThenPopsToExactlyOne()
    {
        Assert.AreEqual(0f, EntranceMath.FlyBadgeScale(0f), Eps);
        Assert.AreEqual(0f, EntranceMath.FlyBadgeScale(EntranceMath.FlySlideEnd), Eps);
        Assert.AreEqual(1f, EntranceMath.FlyBadgeScale(1f), Eps);

        float peak = 0f;
        for (float t = 0f; t <= 1f; t += 0.005f)
            peak = Mathf.Max(peak, EntranceMath.FlyBadgeScale(t));
        Assert.LessOrEqual(peak, 1.15f + Eps);
        Assert.Greater(peak, 1f);
    }

    [Test]
    public void InkBadgeScale_AppearsOnlyOnceTheBrushHasPassedIt()
    {
        Assert.AreEqual(0f, EntranceMath.InkBadgeScale(0.3f, 0.5f), Eps);
        Assert.AreEqual(1f, EntranceMath.InkBadgeScale(1f, 0.5f), Eps);
        Assert.Greater(EntranceMath.InkBadgeScale(0.55f, 0.5f), 0f);
    }

    // ---------- skip ----------

    [Test]
    public void Skip_NothingRunning_DoesNotSkipOrFinish()
    {
        EntranceMath.State s = default;

        Assert.IsFalse(EntranceMath.ShouldSkipOnTap(s));
        Assert.IsFalse(EntranceMath.TryFinish(ref s));
    }

    [Test]
    public void Skip_RunningEntrance_AFinishIsAnnouncedExactlyOnce()
    {
        EntranceMath.State s = default;
        EntranceMath.Begin(ref s);

        Assert.IsTrue(EntranceMath.ShouldSkipOnTap(s));
        Assert.IsTrue(EntranceMath.TryFinish(ref s));     // the skip (or the tween running out)
        Assert.IsFalse(EntranceMath.TryFinish(ref s));    // the other one arriving late is ignored
        Assert.IsFalse(EntranceMath.ShouldSkipOnTap(s));  // and a tap after the end is a normal board tap
    }

    [Test]
    public void Skip_AbortedEntrance_AnnouncesNothing()
    {
        EntranceMath.State s = default;
        EntranceMath.Begin(ref s);
        EntranceMath.Abort(ref s);

        Assert.IsFalse(EntranceMath.ShouldSkipOnTap(s));
        Assert.IsFalse(EntranceMath.TryFinish(ref s));
    }

    [Test]
    public void Skip_NextLevelStartsFresh()
    {
        EntranceMath.State s = default;
        EntranceMath.Begin(ref s);
        EntranceMath.TryFinish(ref s);

        EntranceMath.Begin(ref s);

        Assert.IsTrue(EntranceMath.ShouldSkipOnTap(s));
        Assert.IsTrue(EntranceMath.TryFinish(ref s));
    }
}

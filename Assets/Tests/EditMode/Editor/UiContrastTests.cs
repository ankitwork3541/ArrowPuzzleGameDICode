using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

// Accessibility (design/accessibility-requirements.md, Standard tier): text is at least 4.5:1 on its
// background and meaningful non-text shapes at least 3:1. The USS colours below are the ones the
// stylesheets use; the lint test fails if a stylesheet drifts back to a weaker text colour.
public class UiContrastTests
{
    static readonly Color Cream = new Color32(255, 248, 236, 255);
    static readonly Color Card = new Color32(255, 252, 244, 255);
    static readonly Color Ink = new Color32(58, 36, 24, 255);
    static readonly Color DarkGold = new Color32(150, 92, 0, 255);
    static readonly Color GoldOrange = new Color32(232, 140, 0, 255);
    static readonly Color LockedFill = new Color32(239, 232, 220, 255);

    const float TextRatio = 4.5f;
    const float NonTextRatio = 3f;

    // WCAG 2.x relative luminance, same formula as DragonPaletteTests.
    static float Linear(float c)
    {
        return c <= 0.04045f ? c / 12.92f : Mathf.Pow((c + 0.055f) / 1.055f, 2.4f);
    }

    static float Luminance(Color c)
    {
        return 0.2126f * Linear(c.r) + 0.7152f * Linear(c.g) + 0.0722f * Linear(c.b);
    }

    static float Contrast(Color a, Color b)
    {
        float la = Luminance(a), lb = Luminance(b);
        return (Mathf.Max(la, lb) + 0.05f) / (Mathf.Min(la, lb) + 0.05f);
    }

    // A colour drawn at `alpha` over an opaque background, as the panel composites it.
    static Color Over(Color fg, float alpha, Color bg)
    {
        return new Color(fg.r * alpha + bg.r * (1f - alpha),
                         fg.g * alpha + bg.g * (1f - alpha),
                         fg.b * alpha + bg.b * (1f - alpha), 1f);
    }

    [Test]
    public void InkText_OnCreamAndCard_MeetsTextRatio()
    {
        Assert.GreaterOrEqual(Contrast(Ink, Cream), TextRatio);
        Assert.GreaterOrEqual(Contrast(Ink, Card), TextRatio);
    }

    [Test]
    public void MutedInkText_AtSeventyTwoPercent_MeetsTextRatioOnCreamAndCard()
    {
        Assert.GreaterOrEqual(Contrast(Over(Ink, 0.72f, Cream), Cream), TextRatio);
        Assert.GreaterOrEqual(Contrast(Over(Ink, 0.72f, Card), Card), TextRatio);
    }

    [Test]
    public void MutedInkText_AtSixtyPercent_FailsTextRatio_SoItMustNotComeBack()
    {
        // The old muted text colour. Kept as a guard: if this ever passes, the 0.72 raise was not needed.
        Assert.Less(Contrast(Over(Ink, 0.6f, Cream), Cream), TextRatio);
    }

    [Test]
    public void DarkGoldText_OnCreamAndCard_MeetsTextRatio()
    {
        Assert.GreaterOrEqual(Contrast(DarkGold, Cream), TextRatio);
        Assert.GreaterOrEqual(Contrast(DarkGold, Card), TextRatio);
    }

    [Test]
    public void InkOnGoldOrange_MeetsTextRatio()
    {
        Assert.GreaterOrEqual(Contrast(Ink, GoldOrange), TextRatio);
    }

    [Test]
    public void CreamOnGoldOrange_StaysUnderNonTextRatio_SoGoldFillsCarryInk()
    {
        Assert.Less(Contrast(Cream, GoldOrange), NonTextRatio);
    }

    [Test]
    public void ProgressTrackRing_AtFiftyFivePercentInk_MeetsNonTextRatio()
    {
        Assert.GreaterOrEqual(Contrast(Over(Ink, 0.55f, Cream), Cream), NonTextRatio);
    }

    [Test]
    public void LockedNodePadlock_AtSixtyPercentInk_MeetsNonTextRatioOnItsFill()
    {
        Assert.GreaterOrEqual(Contrast(Over(Ink, 0.6f, LockedFill), LockedFill), NonTextRatio);
    }

    // Lint over the real stylesheets: a `color:` (text) declaration must not use ink below 70% alpha
    // or the old dark gold. Background, border and tint declarations are not text and are skipped.
    [Test, Category("Lint")]
    public void Stylesheets_TextColours_DoNotUseWeakInkOrOldDarkGold()
    {
        string folder = Path.Combine(Application.dataPath, "Project", "Themes");
        Assume.That(Directory.Exists(folder), "Themes folder not found: " + folder);

        var inkText = new Regex(@"^\s*color:\s*rgba\(\s*58,\s*36,\s*24,\s*([0-9.]+)\s*\)", RegexOptions.Multiline);
        var oldGoldText = new Regex(@"^\s*color:\s*rgb\(\s*176,\s*112,\s*0\s*\)", RegexOptions.Multiline);

        foreach (string path in Directory.GetFiles(folder, "*.uss"))
        {
            string css = File.ReadAllText(path);

            foreach (Match m in inkText.Matches(css))
            {
                float alpha = float.Parse(m.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
                Assert.GreaterOrEqual(alpha, 0.7f, Path.GetFileName(path) + ": ink text at alpha " + alpha + " is under 4.5:1");
            }

            Assert.IsFalse(oldGoldText.IsMatch(css), Path.GetFileName(path) + ": text still uses rgb(176,112,0) (3.85:1)");
        }
    }
}

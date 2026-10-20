using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

// Story 008 (light paper theme): the dragon palette must read on the cream board, stay clear of the
// two feedback colours, and the dealer must hand colours out deterministically without repeats.
public class ArrowPaletteTests
{
    // Camera / HUD background, #FFF8EC.
    static readonly Color Cream = new Color32(255, 248, 236, 255);

    // WCAG 2.x relative luminance. The palette values are sRGB, as authored from the hex codes.
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
        float lighter = Mathf.Max(la, lb), darker = Mathf.Min(la, lb);
        return (lighter + 0.05f) / (darker + 0.05f);
    }

    static List<Color> Deal(int seed, int count)
    {
        ArrowPalette.Dealer dealer = new ArrowPalette.Dealer(seed);
        List<Color> dealt = new List<Color>(count);
        for (int i = 0; i < count; i++) dealt.Add(dealer.Next());
        return dealt;
    }

    // ---------- the palette itself ----------

    [Test]
    public void Colors_Always_HasSevenDistinctEntries()
    {
        Assert.AreEqual(7, ArrowPalette.Colors.Length);
        Assert.AreEqual(7, new HashSet<Color>(ArrowPalette.Colors).Count);
    }

    [Test]
    public void Colors_EveryEntry_HasAtLeastThreeToOneContrastOnCream()
    {
        for (int i = 0; i < ArrowPalette.Colors.Length; i++)
        {
            float ratio = Contrast(ArrowPalette.Colors[i], Cream);
            Assert.GreaterOrEqual(ratio, 3f, $"colour {i} {ColorUtility.ToHtmlStringRGB(ArrowPalette.Colors[i])} is {ratio:0.00}:1");
        }
    }

    [Test]
    public void Colors_EveryEntry_StaysOutOfTheHintBlueRange()
    {
        // The hint pulse is a strong blue (about 200 degrees); a resting dragon must never wear it.
        foreach (Color c in ArrowPalette.Colors)
        {
            float h, s, v;
            Color.RGBToHSV(c, out h, out s, out v);
            float degrees = h * 360f;
            Assert.IsFalse(degrees >= 195f && degrees <= 235f,
                $"{ColorUtility.ToHtmlStringRGB(c)} sits at {degrees:0} degrees, inside the hint blue range");
        }
    }

    [Test]
    public void Colors_EveryEntry_IsSaturatedEnoughNotToReadAsInk()
    {
        // The bump flash is near-black ink, so no dragon may be dark and grey.
        foreach (Color c in ArrowPalette.Colors)
        {
            float h, s, v;
            Color.RGBToHSV(c, out h, out s, out v);
            Assert.GreaterOrEqual(v, 0.5f, ColorUtility.ToHtmlStringRGB(c) + " is too dark");
            Assert.GreaterOrEqual(s, 0.5f, ColorUtility.ToHtmlStringRGB(c) + " is too grey");
        }
    }

    [Test]
    public void Colors_EveryEntry_IsLdr()
    {
        // LineRenderer vertex colours are 8-bit and clamp at 1.0, so HDR values would be thrown away.
        foreach (Color c in ArrowPalette.Colors)
        {
            Assert.LessOrEqual(Mathf.Max(c.r, Mathf.Max(c.g, c.b)), 1f);
            Assert.GreaterOrEqual(Mathf.Min(c.r, Mathf.Min(c.g, c.b)), 0f);
        }
    }

    // ---------- the dealer ----------

    [Test]
    public void Dealer_SameSeed_DealsTheSameSequence()
    {
        CollectionAssert.AreEqual(Deal(42, 50), Deal(42, 50));
    }

    [Test]
    public void Dealer_DifferentSeeds_DoNotAllDealTheSameSequence()
    {
        List<Color> reference = Deal(1, 21);

        bool anyDifferent = false;
        for (int seed = 2; seed <= 10 && !anyDifferent; seed++)
            anyDifferent = !SameSequence(Deal(seed, 21), reference);

        Assert.IsTrue(anyDifferent);
    }

    static bool SameSequence(List<Color> a, List<Color> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
            if (a[i] != b[i]) return false;
        return true;
    }

    [Test]
    public void Dealer_FirstDeck_HasNoRepeatedColour()
    {
        for (int seed = 0; seed < 25; seed++)
        {
            List<Color> deck = Deal(seed, ArrowPalette.Colors.Length);
            Assert.AreEqual(ArrowPalette.Colors.Length, new HashSet<Color>(deck).Count, "seed " + seed);
        }
    }

    [Test]
    public void Dealer_EveryDeck_SpendsEveryColourOnce()
    {
        int n = ArrowPalette.Colors.Length;
        List<Color> dealt = Deal(7, n * 12);

        for (int deck = 0; deck < 12; deck++)
        {
            HashSet<Color> inDeck = new HashSet<Color>(dealt.GetRange(deck * n, n));
            Assert.AreEqual(n, inDeck.Count, "deck " + deck);
        }
    }

    [Test]
    public void Dealer_AcrossDeckBoundaries_NeverRepeatsBackToBack()
    {
        for (int seed = 0; seed < 25; seed++)
        {
            List<Color> dealt = Deal(seed, ArrowPalette.Colors.Length * 20);
            for (int i = 1; i < dealt.Count; i++)
                Assert.AreNotEqual(dealt[i - 1], dealt[i], $"seed {seed}, position {i}");
        }
    }

    [Test]
    public void Dealer_EveryDealtColour_IsFromThePalette()
    {
        HashSet<Color> palette = new HashSet<Color>(ArrowPalette.Colors);
        foreach (Color c in Deal(3, 100))
            Assert.IsTrue(palette.Contains(c));
    }
}

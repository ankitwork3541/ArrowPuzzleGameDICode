using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// Where the result sheet's native ad goes. The plugin lays an x,y-placed template out in whatever
// height is left under its top edge, and the sheet's ad box ends at the screen's bottom edge, so an
// x,y placement can never measure taller than the room already reserved for it.
public class PromoPlacementTests
{
    [Test]
    public void ForSheet_ContentFitsScreen_AnchorsToBottom()
    {
        PromoNativePlacement p = PromoNativePlacement.ForSheet(0f, 0, 500);

        Assert.IsTrue(p.anchored);
        Assert.AreEqual(GoogleMobileAds.Api.AdPosition.Bottom, p.anchor);
    }

    [Test]
    public void ForSheet_ScrollRangeNotLaidOutYet_AnchorsToBottom()
    {
        Assert.IsTrue(PromoNativePlacement.ForSheet(float.NaN, 0, 500).anchored);
    }

    [Test]
    public void ForSheet_ContentScrolls_FollowsTheBox()
    {
        PromoNativePlacement p = PromoNativePlacement.ForSheet(40f, 3, 500);

        Assert.IsFalse(p.anchored);
        Assert.AreEqual(3, p.x);
        Assert.AreEqual(500, p.y);
    }

    // The card slides in from a 100% translate. UI Toolkit counts that transform into the scroller's
    // own range, so the sheet looked scrollable exactly while its ad was being placed.
    [TestCase(200f, 0f)]
    [TestCase(1000f, 500f)]
    public void ScrollRange_CardTranslatedOffScreen_CountsLayoutOnly(float cardHeight, float expected)
    {
        EditorWindow window = ScriptableObject.CreateInstance<EditorWindow>();
        try
        {
            window.ShowUtility();

            ScrollView scroll = new ScrollView { verticalScrollerVisibility = ScrollerVisibility.Hidden };
            scroll.style.width = 360f;
            scroll.style.height = 500f;
            scroll.style.flexShrink = 0f;
            scroll.contentContainer.style.flexGrow = 1f;
            scroll.contentContainer.style.flexShrink = 0f;
            scroll.contentContainer.style.justifyContent = Justify.FlexEnd;

            VisualElement card = new VisualElement();
            card.style.height = cardHeight;
            card.style.flexShrink = 0f;
            card.style.translate = new Translate(0f, Length.Percent(100f));
            scroll.Add(card);
            window.rootVisualElement.Add(scroll);

            MethodInfo layout = scroll.panel.GetType().GetMethod(
                "ValidateLayout", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            for (int i = 0; i < 4; i++)
            {
                layout.Invoke(scroll.panel, null);
            }

            Assert.AreEqual(expected, SheetScreenController.ScrollRange(scroll), 0.5f);
        }
        finally
        {
            window.Close();
        }
    }
}

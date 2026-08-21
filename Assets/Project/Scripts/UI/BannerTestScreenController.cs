using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
using UnityEngine;
using UnityEngine.UIElements;

// A demo screen that puts all three candidate banner formats on screen at once, so the sizes can be
// compared on a real device before deciding where banners go in the game. Nothing here is production
// code: the game's own banner is BannerAdUnit, and this screen does not touch it.
//
// The two slots are exactly what backend digits 1 and 2 map to, so what the client approves here is
// what the game will show. Sizes come from BannerAdUnit so the demo cannot drift from production.
public class BannerTestScreenController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;

    [Tooltip("Where to go when Close is tapped. Leave empty to just deactivate this screen.")]
    [SerializeField] private GameObject returnToRoot;

    private VisualElement root;
    private readonly List<Slot> slots = new List<Slot>();
    private Coroutine placeRoutine;

    // One banner plus the UI that describes it.
    private class Slot
    {
        public string title;
        public AdSize size;
        public int initialHeightDp;
        public string spaceName;
        public string resultName;

        public BannerView banner;
        public bool loaded;
        public string error;
        public VisualElement space;
        public Vector2Int placedAtDp = new Vector2Int(int.MinValue, int.MinValue);
    }

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);

        if (placeRoutine != null)
        {
            StopCoroutine(placeRoutine);
            placeRoutine = null;
        }

        DestroyBanners();
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        root = rootElement;

        Button close = root.Q<Button>("close-button");
        if (close != null)
        {
            close.clicked += Close;
        }

        ShowDeviceInfo();
        Build();
    }

    private void ShowDeviceInfo()
    {
        Label info = root.Q<Label>("device-info");
        if (info == null)
        {
            return;
        }

        float density = NativeAdSpace.DisplayDensity;

        info.text = Screen.width + " x " + Screen.height + " px"
                    + "   |   " + WidthDp + " x " + HeightDp + " dp"
                    + "   |   density " + density;
    }

    // dp is the unit every AdSize is expressed in, and the unit smart banner's height rule uses.
    private static int WidthDp => BannerAdUnit.ScreenWidthDp;

    private static int HeightDp =>
        Mathf.RoundToInt(Screen.height / NativeAdSpace.DisplayDensity);

    private void Build()
    {
        DestroyBanners();
        slots.Clear();

        // Anchored adaptive: hand it the screen's real dp width and the SDK returns the height it
        // considers right for this device. Shared with BannerAdUnit so the two cannot drift — see
        // the note there about why AdSize.FullWidth must not be used.
        slots.Add(new Slot
        {
            title = "adaptive",
            size = BannerAdUnit.AnchoredAdaptiveSize(),
            initialHeightDp = 50,
            spaceName = "ad-space-1",
            resultName = "result-1"
        });

        slots.Add(new Slot
        {
            title = "large adaptive",
            size = BannerAdUnit.LargeAnchoredAdaptiveSize(),
            initialHeightDp = 100,
            spaceName = "ad-space-2",
            resultName = "result-2"
        });

        SetFooter();

        AdsManager manager = AdsManager.Instance;
        if (manager == null)
        {
            SetAllResults("No AdsManager in the scene.");
            return;
        }

        if (manager.Ready)
        {
            CreateBanners();
        }
        else
        {
            SetAllResults("Waiting for the Google Mobile Ads SDK...");
            manager.OnReady += OnAdsReady;
        }
    }

    private void OnAdsReady()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.OnReady -= OnAdsReady;
        }

        CreateBanners();
    }

    private void CreateBanners()
    {
        AdSettings settings = AdsManager.Instance.Settings;

        if (!settings.AdsEnabled)
        {
            SetAllResults("Ads are switched off by the backend (ad_enable is not 1), so there is "
                          + "nothing to request.");
            return;
        }

        string unitId = settings.BannerId;
        if (string.IsNullOrEmpty(unitId))
        {
            SetAllResults("No banner ad unit ID available from the backend or the baked config.");
            return;
        }

        Debug.Log("[BannerTest] Requesting three banners from " + unitId
                  + " (test units: " + settings.UsingTestIds + ").");

        foreach (Slot slot in slots)
        {
            // Per slot, so one size the SDK rejects still leaves the other two to compare.
            try
            {
                CreateBanner(slot, unitId);
            }
            catch (System.Exception e)
            {
                slot.error = e.Message;
                Debug.LogError("[BannerTest] " + slot.title + " could not be created: " + e);
                UpdateResult(slot);
            }
        }

        if (placeRoutine != null)
        {
            StopCoroutine(placeRoutine);
        }

        placeRoutine = StartCoroutine(PlaceBanners());
    }

    private void CreateBanner(Slot slot, string unitId)
    {
        // Built off the bottom of the screen on purpose. A BannerView shows itself the moment it
        // loads, and a banner that appears before its slot has been given room lands on top of
        // whatever is underneath — the same accidental-click problem the native ad avoids by
        // reserving space first. It is moved into its slot once the layout is settled.
        int offscreenYdp = HeightDp + 400;

        slot.banner = new BannerView(unitId, slot.size, 0, offscreenYdp);

        BannerView created = slot.banner;

        created.OnBannerAdLoaded += () => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (slot.banner != created)
            {
                return;
            }

            slot.loaded = true;
            slot.error = null;
            Debug.Log("[BannerTest] " + slot.title + " loaded: "
                      + created.GetWidthInPixels() + " x " + created.GetHeightInPixels() + " px.");
            UpdateResult(slot);
        });

        created.OnBannerAdLoadFailed += error => MobileAdsEventExecutor.ExecuteInUpdate(() =>
        {
            if (slot.banner != created)
            {
                return;
            }

            slot.loaded = false;
            slot.error = error != null ? error.GetMessage() : "unknown error";
            Debug.LogWarning("[BannerTest] " + slot.title + " failed: " + slot.error);
            UpdateResult(slot);
        });

        created.LoadAd(new AdRequest());
    }

    // Opens each slot at its expected height first, then corrects from the banner's real size and
    // moves the banner onto the slot. This keeps running while the screen is up rather than placing
    // once: the ScrollView moves the slots, and a banner that does not follow ends up floating over
    // the wrong part of the screen. OnDisable stops it.
    private IEnumerator PlaceBanners()
    {
        float density = NativeAdSpace.DisplayDensity;

        foreach (Slot slot in slots)
        {
            slot.space = root.Q<VisualElement>(slot.spaceName);
            NativeAdSpace.Apply(slot.space, slot.initialHeightDp * density);
        }

        while (true)
        {
            yield return null;

            foreach (Slot slot in slots)
            {
                if (slot.space == null || !slot.loaded || slot.banner == null)
                {
                    continue;
                }

                float heightPx = slot.banner.GetHeightInPixels();
                if (heightPx > 0f)
                {
                    NativeAdSpace.Apply(slot.space, heightPx);
                }

                if (!NativeAdSpace.TryTopLeftDp(slot.space, out Vector2Int topLeftDp))
                {
                    continue;
                }

                // Only when it actually moved. SetPosition crosses into Java every call.
                if (topLeftDp != slot.placedAtDp)
                {
                    slot.banner.SetPosition(topLeftDp.x, topLeftDp.y);
                    slot.placedAtDp = topLeftDp;
                    UpdateResult(slot);
                }
            }
        }
    }

    private void UpdateResult(Slot slot)
    {
        Label label = root != null ? root.Q<Label>(slot.resultName) : null;
        if (label == null)
        {
            return;
        }

        if (slot.error != null)
        {
            label.text = "Failed to load: " + slot.error;
            return;
        }

        if (!slot.loaded || slot.banner == null)
        {
            label.text = "Loading...";
            return;
        }

        float density = NativeAdSpace.DisplayDensity;
        float widthPx = slot.banner.GetWidthInPixels();
        float heightPx = slot.banner.GetHeightInPixels();

        // An anchored adaptive AdSize reports height 0 until the SDK has resolved it, so printing
        // the raw number would read as "asked for zero height".
        string requestedHeight = slot.size.Height > 0 ? slot.size.Height + " dp" : "auto";

        label.text = "Requested " + slot.size.Width + " dp x " + requestedHeight
                     + "   ->   actual " + Mathf.RoundToInt(widthPx) + " x " + Mathf.RoundToInt(heightPx) + " px"
                     + "  (" + Mathf.RoundToInt(widthPx / density) + " x " + Mathf.RoundToInt(heightPx / density) + " dp)";
    }

    private void SetAllResults(string message)
    {
        if (root == null)
        {
            return;
        }

        foreach (Slot slot in slots)
        {
            Label label = root.Q<Label>(slot.resultName);
            if (label != null)
            {
                label.text = message;
            }
        }
    }

    private void SetFooter()
    {
        Label footer = root != null ? root.Q<Label>("footer") : null;
        if (footer == null)
        {
            return;
        }

        footer.text =
            "These are the two banner options the backend can ask for: digit 1 for the standard "
            + "height, digit 2 for the taller one. Both are ANCHORED ADAPTIVE - they take this "
            + "device's real width and let Google choose the height, so the creative fills the box "
            + "instead of sitting in the middle of it.\n\n"
            + "Smart banner is deliberately not shown. Google has deprecated it, and it centres a "
            + "standard creative inside a screen-wide box, which leaves visible bars down both "
            + "sides.\n\n"
            + "The backend digit also chooses natives: 3 for the small native, 4 for the big one, "
            + "and 0 for no ad at all. Those two are already live in the game itself.\n\n"
            + "This device is " + WidthDp + " x " + HeightDp + " dp.";
    }

    private void DestroyBanners()
    {
        if (AdsManager.Instance != null)
        {
            AdsManager.Instance.OnReady -= OnAdsReady;
        }

        foreach (Slot slot in slots)
        {
            if (slot.banner != null)
            {
                slot.banner.Destroy();
                slot.banner = null;
            }

            slot.loaded = false;
        }
    }

    private void Close()
    {
        gameObject.SetActive(false);

        if (returnToRoot != null)
        {
            returnToRoot.SetActive(true);
        }
    }
}

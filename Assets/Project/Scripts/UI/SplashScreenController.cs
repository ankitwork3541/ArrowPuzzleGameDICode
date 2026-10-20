using UnityEngine;
using UnityEngine.UIElements;
using PrimeTween;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private float splashDuration = 2f;
    [SerializeField] private GameObject homeRoot;

    private ProgressBar progressBar;

    // Versioned overload of the reload callback (the plain one is obsolete). The lambda only drops the
    // version: these handlers rebuild their bindings from scratch on every reload, so a repeat of the
    // same version is harmless. Kept in a field so OnDisable removes the very same delegate.
    private PanelRenderer.VersionedUIReloadCallback reloadCallback;

    private void OnEnable()
    {
        reloadCallback = (reloaded, rootElement, version) => OnUIReload(reloaded, rootElement);
        panelRenderer.RegisterUIReloadCallback(reloadCallback);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(reloadCallback);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        progressBar = rootElement.Q<ProgressBar>("loading-bar");

        Tween.Custom(this, 0f, 1f, duration: splashDuration, onValueChange: (controller, t) =>
        {
            progressBar.value = t * 100f;
            progressBar.title = Mathf.RoundToInt(t * 100f) + "%";
        }).OnComplete(this, controller => controller.GoToHome());
    }

    private void GoToHome()
    {
        gameObject.SetActive(false);
        homeRoot.SetActive(true);
    }
}

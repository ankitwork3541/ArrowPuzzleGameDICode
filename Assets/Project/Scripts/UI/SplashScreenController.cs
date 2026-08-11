using UnityEngine;
using UnityEngine.UIElements;
using PrimeTween;

public class SplashScreenController : MonoBehaviour
{
    [SerializeField] private PanelRenderer panelRenderer;
    [SerializeField] private float splashDuration = 2f;

    private ProgressBar progressBar;

    private void OnEnable()
    {
        panelRenderer.RegisterUIReloadCallback(OnUIReload);
    }

    private void OnDisable()
    {
        panelRenderer.UnregisterUIReloadCallback(OnUIReload);
    }

    private void OnUIReload(PanelRenderer renderer, VisualElement rootElement)
    {
        progressBar = rootElement.Q<ProgressBar>("loading-bar");

        Tween.Custom(0f, 1f, duration: splashDuration, onValueChange: t =>
        {
            progressBar.value = t * 100f;
            progressBar.title = Mathf.RoundToInt(t * 100f) + "%";
        });
    }
}

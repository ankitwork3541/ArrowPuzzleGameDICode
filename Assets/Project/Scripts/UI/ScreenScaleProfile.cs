using UnityEngine;
using UnityEngine.UIElements;

public class ScreenScaleProfile : MonoBehaviour
{
    [SerializeField] private PanelSettings panelSettings;
    [SerializeField] private float tabletDiagonalInchThreshold = 7f;
    [SerializeField] private float tabletScaleMultiplier = 1.4f;

    private void Awake()
    {
        float dpi = Screen.dpi > 0 ? Screen.dpi : 160f;
        float diagonalInches = Mathf.Sqrt(Screen.width * Screen.width + Screen.height * Screen.height) / dpi;

        panelSettings.scale = diagonalInches >= tabletDiagonalInchThreshold ? tabletScaleMultiplier : 1f;
    }
}

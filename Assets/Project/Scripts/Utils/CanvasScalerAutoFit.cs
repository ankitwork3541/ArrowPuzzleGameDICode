using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerAutoFit : MonoBehaviour
{
    [SerializeField] private CanvasScaler canvasScaler;

    private void Awake()
    {
        float referenceAspect = canvasScaler.referenceResolution.x / canvasScaler.referenceResolution.y;
        float deviceAspect = (float)Screen.width / Screen.height;

        canvasScaler.matchWidthOrHeight = deviceAspect < referenceAspect ? 1f : 0f;
    }
}

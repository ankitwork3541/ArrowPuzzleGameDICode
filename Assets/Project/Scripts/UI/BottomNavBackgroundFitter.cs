using UnityEngine;

public class BottomNavBackgroundFitter : MonoBehaviour
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform navContent;
    [SerializeField] private Canvas canvas;

    private void Update()
    {
        float bottomInset = Screen.safeArea.yMin / canvas.scaleFactor;
        background.sizeDelta = new Vector2(background.sizeDelta.x, navContent.rect.height + bottomInset);
    }
}

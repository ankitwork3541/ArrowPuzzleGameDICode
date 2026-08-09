using UnityEngine;
using PrimeTween;

public class Dot : MonoBehaviour
{
    public Vector2Int GridPosition;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }

    // Instant show, no animation — level-load spawn ane pool-reuse mate (dar dot mate animation joie nahi)
    public void Show()
    {
        spriteRenderer.enabled = true;
        transform.localScale = originalScale;
    }

    // Animated pop-reveal — khali tyare j vaparo jyare khas kari ne "arrow e cell vacate kari" (real reveal moment)
    public void Reveal()
    {
        spriteRenderer.enabled = true;
        transform.localScale = Vector3.zero;

        Vector3 overshootScale = originalScale * 1.5f;
        Sequence.Create()
            .Chain(Tween.Scale(transform, overshootScale, duration: 0.15f, ease: Ease.OutQuad))
            .Chain(Tween.Scale(transform, originalScale, duration: 0.15f, ease: Ease.InOutSine));
    }

    public void Hide()
    {
        spriteRenderer.enabled = false;
    }
}

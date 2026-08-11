using UnityEngine;
using PrimeTween;

public class Dot : MonoBehaviour
{
    public Vector2Int GridPosition;

    [Header("Reveal")]
    public Color revealColor = new Color(0f, 0.898f, 1f);   // Neon Cyan #00E5FF
    public float popTime = 0.08f;                           // one half of the scale pop
    // Cyan back to the resting colour. Deliberately much longer than one movement step: at top
    // speed (~0.028s per cell) this keeps roughly 20 dots lit at once, so the trail reads as a
    // glowing LINE behind the arrow rather than a single dot blinking on and off.
    public float coolDownTime = 0.6f;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private Color normalColor;

    private Sequence popSequence;
    private Tween colorTween;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        normalColor = spriteRenderer.color;
    }

    // Instant show, no animation — level-load spawn ane pool-reuse mate (dar dot mate animation joie nahi)
    public void Show()
    {
        StopAnimations();
        spriteRenderer.enabled = true;
        transform.localScale = originalScale;
        spriteRenderer.color = normalColor;
    }

    // Animated pop-reveal — khali tyare j vaparo jyare khas kari ne "arrow e cell vacate kari"
    // (real reveal moment). Pops in bright cyan and cools back down, so a moving arrow reads
    // as laying a glowing trail behind itself.
    public void Reveal()
    {
        StopAnimations();

        spriteRenderer.enabled = true;
        transform.localScale = Vector3.zero;
        spriteRenderer.color = revealColor;

        Vector3 overshootScale = originalScale * 1.5f;
        popSequence = Sequence.Create()
            .Chain(Tween.Scale(transform, overshootScale, duration: popTime, ease: Ease.OutQuad))
            .Chain(Tween.Scale(transform, originalScale, duration: popTime, ease: Ease.InOutSine));

        // Deliberately not part of the sequence: the colour cools over the full duration while
        // the scale does its two-stage pop underneath it. Ease.InQuad, not OutQuad — the dot has
        // to HOLD its cyan and drop off late, or the trail is one bright dot and a grey smear.
        colorTween = Tween.Color(spriteRenderer, normalColor, duration: coolDownTime, ease: Ease.InQuad);
    }

    public void Hide()
    {
        spriteRenderer.enabled = false;
    }

    // A pooled dot can be released mid-reveal and handed straight back out, so a leftover
    // tween would fight whatever the next owner sets.
    void StopAnimations()
    {
        popSequence.Stop();
        colorTween.Stop();
    }
}

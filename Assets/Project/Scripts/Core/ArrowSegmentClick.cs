using UnityEngine;
using UnityEngine.InputSystem;

public class ArrowSegmentClick : MonoBehaviour
{
    public ArrowController owner;
    Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (col.OverlapPoint(worldPos))
            {
                owner.TryAdvance();
            }
        }
    }
}

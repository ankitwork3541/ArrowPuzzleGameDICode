using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class ArrowController : MonoBehaviour
{
    public List<Vector2Int> path = new List<Vector2Int>();
    public GridBuilder gridBuilder; // Inspector ma GridBuilder object drag karo

    public float spacing = 1f;

    private LineRenderer line;
    private EdgeCollider2D edgeCollider;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        edgeCollider = GetComponent<EdgeCollider2D>();
    }

    void Start()
    {
        DrawPath();
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            if (edgeCollider.OverlapPoint(worldPos))
            {
                if (edgeCollider.OverlapPoint(worldPos))
                {
                    TryAdvance();
                }

            }
        }
    }

    void DrawPath()
    {
        line.positionCount = path.Count;
        Vector2[] colliderPoints = new Vector2[path.Count];

        for (int i = 0; i < path.Count; i++)
        {
            Vector3 worldPos = new Vector3(path[i].x * spacing, path[i].y * spacing, 0f);
            line.SetPosition(i, worldPos);
            colliderPoints[i] = worldPos;
        }

        edgeCollider.points = colliderPoints;
        edgeCollider.edgeRadius = 0.15f; // line jetli jaadi che, e pramane
    }
    void TryAdvance()
    {
        int n = path.Count;
        if (n < 2) return;

        Vector2Int head = path[n - 1];
        Vector2Int prev = path[n - 2];
        Vector2Int dir = head - prev;
        Vector2Int nextPos = head + dir;

        GridCell nextCell = gridBuilder.GetCell(nextPos);

        if (nextCell == null)
        {
            Debug.Log(gameObject.name + ": grid ni bahar → EXIT karvanu");
        }
        else if (gridBuilder.IsCellOccupied(nextPos))
        {
            Debug.Log(gameObject.name + ": BLOCKED at " + nextPos);
        }
        else
        {
            Debug.Log(gameObject.name + ": CLEAR, move to " + nextPos);
        }
    }

}

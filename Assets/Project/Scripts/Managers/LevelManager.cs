using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public GameFlowManager gameFlowManager;

    public int TotalArrows { get; private set; }
    public int RemainingArrows { get; private set; }

    // The HUD subscribes to this instead of polling every frame.
    public event System.Action<int, int> OnArrowCountChanged; // (remaining, total)

    void Awake()
    {
        Instance = this;
    }

    public void SetArrowCount(int count)
    {
        TotalArrows = count;
        RemainingArrows = count;
        OnArrowCountChanged?.Invoke(RemainingArrows, TotalArrows);
    }

    public void ArrowExited()
    {
        RemainingArrows--;
        OnArrowCountChanged?.Invoke(RemainingArrows, TotalArrows);

        if (RemainingArrows <= 0)
        {
            gameFlowManager.ShowWin();
        }
    }
}

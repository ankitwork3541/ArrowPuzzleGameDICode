using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public GameFlowManager gameFlowManager;

    public int TotalArrows { get; private set; }
    public int RemainingArrows { get; private set; }

    // The HUD subscribes to this instead of polling every frame.
    public event System.Action<int, int> OnArrowCountChanged; // (remaining, total)

    // Fired once for every arrow that leaves the board. Locked arrows listen to this to count
    // themselves down, so LevelManager never needs a list of arrows.
    public event System.Action OnArrowLeft;

    // False while no level is being played. Guards against an arrow that was already
    // mid-exit when the player left the level reporting a win afterwards.
    private bool levelActive;

    void Awake()
    {
        Instance = this;
    }

    public void SetArrowCount(int count)
    {
        levelActive = true;
        TotalArrows = count;
        RemainingArrows = count;
        OnArrowCountChanged?.Invoke(RemainingArrows, TotalArrows);
    }

    public void ClearLevel()
    {
        levelActive = false;
        TotalArrows = 0;
        RemainingArrows = 0;
        OnArrowCountChanged?.Invoke(RemainingArrows, TotalArrows);
    }

    public void ArrowExited()
    {
        if (!levelActive) return;

        RemainingArrows--;
        OnArrowCountChanged?.Invoke(RemainingArrows, TotalArrows);
        OnArrowLeft?.Invoke();

        if (RemainingArrows <= 0)
        {
            gameFlowManager.ShowWin();
        }
    }
}

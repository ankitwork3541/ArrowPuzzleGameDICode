using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;
    public FlowDirector gameFlowManager;

    public int TotalDragons { get; private set; }
    public int RemainingDragons { get; private set; }

    // The HUD subscribes to this instead of polling every frame.
    public event System.Action<int, int> OnDragonCountChanged; // (remaining, total)

    // Fired once for every arrow that leaves the board. Locked arrows listen to this to count
    // themselves down, so StageManager never needs a list of arrows.
    public event System.Action OnDragonLeft;

    // False while no level is being played. Guards against an arrow that was already
    // mid-exit when the player left the level reporting a win afterwards.
    private bool levelActive;

    void Awake()
    {
        Instance = this;
    }

    public void SetDragonCount(int count)
    {
        levelActive = true;
        TotalDragons = count;
        RemainingDragons = count;
        OnDragonCountChanged?.Invoke(RemainingDragons, TotalDragons);
    }

    public void ClearLevel()
    {
        levelActive = false;
        TotalDragons = 0;
        RemainingDragons = 0;
        OnDragonCountChanged?.Invoke(RemainingDragons, TotalDragons);
    }

    public void DragonExited()
    {
        if (!levelActive) return;

        RemainingDragons--;
        OnDragonCountChanged?.Invoke(RemainingDragons, TotalDragons);
        OnDragonLeft?.Invoke();

        if (RemainingDragons <= 0)
        {
            gameFlowManager.ShowWin();
        }
    }
}

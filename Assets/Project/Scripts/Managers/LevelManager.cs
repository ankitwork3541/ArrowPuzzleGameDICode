using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;
    public GameFlowManager gameFlowManager;

    private int remainingArrows;

    void Awake()
    {
        Instance = this;
    }

    public void SetArrowCount(int count)
    {
        remainingArrows = count;
    }

    public void ArrowExited()
    {
        remainingArrows--;

        if (remainingArrows <= 0)
        {
            gameFlowManager.ShowWin();
        }
    }
}

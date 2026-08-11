using UnityEngine;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;

    public int maxLives = 3;
    public GameFlowManager gameFlowManager;

    public int lives { get; private set; }

    // The HUD subscribes to this instead of LivesManager holding UI references itself.
    public event System.Action<int, int> OnLivesChanged; // (lives, maxLives)

    void Awake()
    {
        Instance = this;
        lives = maxLives;
    }

    public void LoseLife()
    {
        if (lives <= 0) return;

        lives--;
        OnLivesChanged?.Invoke(lives, maxLives);

        if (lives <= 0)
        {
            gameFlowManager.ShowLose();
        }
    }

    public void ResetLives()
    {
        lives = maxLives;
        OnLivesChanged?.Invoke(lives, maxLives);
    }
}

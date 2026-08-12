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

    // True between the losing tap and the end of that tap's bump-and-rewind animation.
    private bool gameOverPending;

    public void LoseLife()
    {
        if (lives <= 0) return;

        lives--;
        OnLivesChanged?.Invoke(lives, maxLives);

        // Game over is deliberately NOT fired here. ShowLose() clears the level, which destroys
        // the very arrow that is still mid-bump, so the player would never see the bump or the
        // rewind that explains why they lost — the board would just vanish on a red flash.
        // The arrow calls ResolveBlockedTap() once its animation has finished playing.
        if (lives <= 0) gameOverPending = true;
    }

    // Called by the arrow when its blocked bump + rewind has finished.
    public void ResolveBlockedTap()
    {
        if (!gameOverPending) return;

        gameOverPending = false;
        gameFlowManager.ShowLose();
    }

    public void ResetLives()
    {
        lives = maxLives;
        gameOverPending = false;   // a level abandoned mid-rewind must not lose on its first tap
        OnLivesChanged?.Invoke(lives, maxLives);
    }
}

using UnityEngine;

public class EmberManager : MonoBehaviour
{
    public static EmberManager Instance;

    public int maxLives = 3;
    public FlowDirector gameFlowManager;

    public int lives { get; private set; }

    // The HUD subscribes to this instead of EmberManager holding UI references itself.
    public event System.Action<int, int> OnLivesChanged; // (lives, maxLives)

    void Awake()
    {
        Instance = this;
        maxLives = ConfiguredMaxLives();
        lives = maxLives;
    }

    // game.lives.max from the game config. The Inspector value only matters until a config (cached or
    // fresh) says otherwise; the change lands at the next level start (ResetLives), never mid-level.
    static int ConfiguredMaxLives()
    {
        return Mathf.Max(1, GameConfigService.Current.game.lives.max);
    }

    // True between the losing tap and the end of that tap's bump-and-rewind animation.
    private bool gameOverPending;

    public void LoseLife()
    {
        if (lives <= 0) return;

        lives--;
        OnLivesChanged?.Invoke(lives, maxLives);

        // Follows the bump by a beat (SoundDirector schedules it), so the two never blur together.
        if (SoundDirector.Instance != null) SoundDirector.Instance.PlayHeartLost();

        // Game over is deliberately NOT fired here. ShowLose() drops the Lose card over the board
        // and freezes input, so firing it mid-bump would cut off the bump and the rewind that
        // explain why the player lost — the card would land on a red flash instead of on a
        // finished move. The arrow calls ResolveBlockedTap() once its animation has played out.
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
        maxLives = ConfiguredMaxLives();
        lives = maxLives;
        gameOverPending = false;   // a level abandoned mid-rewind must not lose on its first tap
        OnLivesChanged?.Invoke(lives, maxLives);
    }
}

using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    [Header("UI Toolkit panel roots")]
    public GameObject homeRoot;

    [Header("Systems")]
    public LevelLoader levelLoader;
    public LivesManager livesManager;

    public void OnPlayButtonClicked()
    {
        homeRoot.SetActive(false);
        levelLoader.LoadLevel();
    }

    // TODO: Win panel is not rebuilt in UI Toolkit yet — for now the flow just
    // advances a level and returns to Home so the loop stays playable.
    public void ShowWin()
    {
        Debug.Log("LEVEL WON (Win panel not rebuilt in UI Toolkit yet)");
        livesManager.ResetLives();
        levelLoader.GoToNextLevel();
        homeRoot.SetActive(true);
    }

    // TODO: Lose panel is not rebuilt in UI Toolkit yet — same temporary behaviour.
    public void ShowLose()
    {
        Debug.Log("LEVEL LOST (Lose panel not rebuilt in UI Toolkit yet)");
        livesManager.ResetLives();
        levelLoader.LoadLevel();
        homeRoot.SetActive(true);
    }

    public void OnHintButtonClicked()
    {
        ArrowController hintArrow = levelLoader.FindHintableArrow();
        if (hintArrow != null)
            hintArrow.PlayHintPulse();
    }

    public static string GetDifficultyLabel(LevelData level)
    {
        int arrowCount = level.arrows.Count;
        if (arrowCount < 15) return "EASY";
        if (arrowCount < 50) return "MEDIUM";
        if (arrowCount < 150) return "HARD";
        return "HARDEST";
    }
}

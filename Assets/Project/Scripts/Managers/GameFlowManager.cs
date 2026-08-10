using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PrimeTween;

public class GameFlowManager : MonoBehaviour
{
    public GameObject splashPanel;
    public GameObject homePanel;
    public GameObject gameplayPanel;

    public GameObject winPanel;
    public GameObject losePanel;
    public LevelLoader levelLoader;
    public LivesManager livesManager;
    public TMP_Text levelText;
    public TMP_Text difficultyText;
    public Image progressBarFill;
    public TMP_Text loadingPercentText;
    public float splashDuration = 2f;

    void Start()
    {
        splashPanel.SetActive(true);
        homePanel.SetActive(false);
        gameplayPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        progressBarFill.fillAmount = 0f;
        Tween.Custom(0f, 1f, duration: splashDuration, onValueChange: t =>
        {
            progressBarFill.fillAmount = t;
            loadingPercentText.text = Mathf.RoundToInt(t * 100f) + "%";
        }).OnComplete(ShowHome);
    }

    void ShowHome()
    {
        splashPanel.SetActive(false);
        homePanel.SetActive(true);
        levelText.text = "Level " + levelLoader.CurrentLevelNumber();
        difficultyText.text = GetDifficultyLabel(levelLoader.CurrentLevel());
    }

    string GetDifficultyLabel(LevelData level)
    {
        int arrowCount = level.arrows.Count;
        if (arrowCount < 15) return "EASY";
        if (arrowCount < 50) return "MEDIUM";
        if (arrowCount < 150) return "HARD";
        return "HARDEST";
    }

    public void OnPlayButtonClicked()
    {
        homePanel.SetActive(false);
        gameplayPanel.SetActive(true);
        levelLoader.LoadLevel();
    }

    public void ShowWin()
    {
        gameplayPanel.SetActive(false);
        winPanel.SetActive(true);
    }

    public void ShowLose()
    {
        gameplayPanel.SetActive(false);
        losePanel.SetActive(true);
    }

    public void OnRestartButtonClicked()
    {
        losePanel.SetActive(false);
        gameplayPanel.SetActive(true);
        livesManager.ResetLives();
        levelLoader.LoadLevel();
    }

    public void OnNextLevelButtonClicked()
    {
        winPanel.SetActive(false);
        gameplayPanel.SetActive(true);
        livesManager.ResetLives();
        levelLoader.GoToNextLevel();
    }

    public void OnHintButtonClicked()
    {
        ArrowController hintArrow = levelLoader.FindHintableArrow();
        if (hintArrow != null)
            hintArrow.PlayHintPulse();
    }
}

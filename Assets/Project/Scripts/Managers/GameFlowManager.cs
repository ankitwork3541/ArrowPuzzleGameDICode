using UnityEngine;
using TMPro;

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
    public float splashDuration = 2f;

    void Start()
    {
        splashPanel.SetActive(true);
        homePanel.SetActive(false);
        gameplayPanel.SetActive(false);
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        Invoke(nameof(ShowHome), splashDuration);
    }

    void ShowHome()
    {
        splashPanel.SetActive(false);
        homePanel.SetActive(true);
        levelText.text = "Level " + levelLoader.CurrentLevelNumber();
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
}

using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public GameObject splashPanel;
    public GameObject homePanel;
    public GameObject gameplayPanel;
    public GameObject winPanel;
    public GameObject losePanel;
    public LevelLoader levelLoader;
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
}

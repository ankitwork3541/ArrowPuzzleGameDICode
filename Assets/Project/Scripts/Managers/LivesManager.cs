using UnityEngine;
using UnityEngine.UI;

public class LivesManager : MonoBehaviour
{
    public static LivesManager Instance;

    public int lives = 3;
    public Image[] heartImages; // Inspector ma Heart1, Heart2, Heart3 drag karo (order ma)

    void Awake()
    {
        Instance = this;
    }

    public void LoseLife()
    {
        if (lives <= 0) return;

        lives--;
        heartImages[lives].color = Color.gray;

        if (lives <= 0)
        {
            Debug.Log("GAME OVER — level restart karo");
        }
    }
}

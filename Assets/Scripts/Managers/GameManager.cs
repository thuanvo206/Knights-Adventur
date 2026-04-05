using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro; // Thêm dòng này để xài TextMeshPro

public class GameManager : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI coinText; // Thêm dòng này để chứa UI Text của Xu

    bool isGamePaused = false;
    public GameObject pauseGame;
    public GameObject endingGame;

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1.0f;
    }

    public void PauseGame()
    {
        isGamePaused = !isGamePaused;
        Time.timeScale = isGamePaused ? 0.0f : 1.0f;
        if (pauseGame != null) pauseGame.SetActive(isGamePaused);
    }
}
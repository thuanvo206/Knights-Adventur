using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    Player player;
    public Slider healthBar;
    bool isGamePaused = false;
    public GameObject pauseGame;
    public GameObject endingGame;

    void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            if (player != null) healthBar.maxValue = player.maxPlayerHealth;
            return; 
        }

        if (player.isDead) Invoke("RestartGame", 0.4f);
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (player != null)
        {
            healthBar.value = player.currentPlayerHealth;
            if (player.currentPlayerHealth <= 0) healthBar.value = 0;
        }
    }

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
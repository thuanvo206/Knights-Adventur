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

    void Start()
    {
        // Không tìm player ở đây nữa vì lúc Start game online, player chưa Spawn xong
    }

    void Update()
    {
        // 1. Kiểm tra nếu chưa có player thì đi tìm liên tục
        if (player == null)
        {
            player = FindObjectOfType<Player>();
            
            // Nếu đã tìm thấy player, thiết lập thanh máu ban đầu
            if (player != null)
            {
                healthBar.maxValue = player.maxPlayerHealth;
            }
            
            // Nếu vẫn chưa thấy player (đang chờ kết nối), thoát Update để tránh lỗi các dòng dưới
            return; 
        }

        // 2. Logic xử lý khi đã có player
        if (player.isDead)
        {
            Invoke("RestartGame", 0.4f);
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        // Luôn kiểm tra player để chắc chắn không truy cập vào biến rỗng
        if (player != null)
        {
            healthBar.value = player.currentPlayerHealth;
            
            if (player.currentPlayerHealth <= 0)
                healthBar.minValue = 0;
        }
    }

    public void RestartGame()
    {
        // Lưu ý: Trong Fusion, LoadScene kiểu này sẽ làm mất kết nối nếu không xử lý đúng.
        // Nhưng tạm thời để sửa lỗi Crash của bạn:
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
        Time.timeScale = 1.0f;
    }

    public void PauseGame()
    {
        isGamePaused = !isGamePaused;

        if (isGamePaused == true)
        {
            Time.timeScale = 0.0f;
            if (pauseGame != null) pauseGame.SetActive(true);
        }
        else
        {
            Time.timeScale = 1.0f;
            if (pauseGame != null) pauseGame.SetActive(false);
        }     
    }

    public void ExitGame()
    {
        SceneManager.LoadScene("Scenes/OpeningScene");
        Time.timeScale = 1.0f;
    }
}
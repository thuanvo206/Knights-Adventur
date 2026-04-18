using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement; // Thêm thư viện này

public class Portal : NetworkBehaviour 
{
    GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        // Chỉ Host mới có quyền ra lệnh đổi Scene
        if (!HasStateAuthority) return;

        if (collider.CompareTag("Player"))
        {
            // Cách lấy tên Scene hiện tại chuẩn và không bị lỗi đỏ
            string currentSceneName = SceneManager.GetActiveScene().name;

            if (currentSceneName == "GameScene")
            {
                // Đồng bộ chuyển cảnh cho tất cả người chơi
                Runner.LoadScene(SceneRef.FromIndex(2)); 
            }
            else if (currentSceneName == "GameScene-2")
            {
                // Gọi RPC để hiện bảng kết thúc cho tất cả mọi người
                RPC_FinishGame();
            }
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_FinishGame()
    {
        if (gameManager != null && gameManager.endingGame != null)
        {
            gameManager.endingGame.SetActive(true);
            // Lưu ý: Không dùng Time.timeScale = 0 trong Multiplayer vì sẽ làm treo Network
        }
    }
}
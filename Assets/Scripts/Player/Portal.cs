using UnityEngine;
using Fusion;
using UnityEngine.SceneManagement;

public class Portal : NetworkBehaviour
{
    GameManager gameManager;

    // BUG FIX: NetworkBehaviour nên dùng Spawned() thay vì Start()
    // Start() có thể chạy trước khi network object được khởi tạo xong
    // → gameManager có thể null hoặc tìm sai object
    public override void Spawned()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    void OnTriggerEnter2D(Collider2D collider)
    {
        if (!HasStateAuthority) return;

        if (collider.CompareTag("Player"))
        {
            string currentSceneName = SceneManager.GetActiveScene().name;

            if (currentSceneName == "GameScene")
            {
                Runner.LoadScene(SceneRef.FromIndex(3));
            }
            else if (currentSceneName == "GameScene-2")
            {
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
        }
    }
}
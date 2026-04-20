using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    // Tạo Singleton để ChatUI có thể gọi thông qua PlayerDataManager.Instance
    public static PlayerDataManager Instance { get; private set; }

    [SerializeField] private string playerName = "Player123";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Hàm trả về tên người chơi mà ChatUI đang cần gọi
    public string GetPlayerName()
    {
        return playerName;
    }
}
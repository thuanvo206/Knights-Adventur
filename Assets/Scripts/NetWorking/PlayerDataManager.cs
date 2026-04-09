using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    
    [SerializeField] private string playerName = "Player";
    [SerializeField] private string playerId = "";
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Load player name from PlayerPrefs if saved
        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerName = PlayerPrefs.GetString("PlayerName");
        }
        
        if (PlayerPrefs.HasKey("PlayerId"))
        {
            playerId = PlayerPrefs.GetString("PlayerId");
        }
    }
    
    /// <summary>
    /// Lấy tên của người chơi hiện tại
    /// </summary>
    public string GetPlayerName()
    {
        return playerName;
    }
    
    /// <summary>
    /// Đặt tên cho người chơi và lưu vào PlayerPrefs
    /// </summary>
    public void SetPlayerName(string name)
    {
        if (!string.IsNullOrEmpty(name))
        {
            playerName = name;
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
            Debug.Log($"Player name set to: {playerName}");
        }
    }
    
    /// <summary>
    /// Lấy ID của người chơi
    /// </summary>
    public string GetPlayerId()
    {
        return playerId;
    }
    
    /// <summary>
    /// Đặt ID cho người chơi
    /// </summary>
    public void SetPlayerId(string id)
    {
        if (!string.IsNullOrEmpty(id))
        {
            playerId = id;
            PlayerPrefs.SetString("PlayerId", playerId);
            PlayerPrefs.Save();
            Debug.Log($"Player ID set to: {playerId}");
        }
    }
    
    /// <summary>
    /// Reset tất cả dữ liệu người chơi
    /// </summary>
    public void ResetPlayerData()
    {
        playerName = "Player";
        playerId = "";
        PlayerPrefs.DeleteKey("PlayerName");
        PlayerPrefs.DeleteKey("PlayerId");
        PlayerPrefs.Save();
        Debug.Log("Player data has been reset");
    }
}

using TMPro;
using UnityEngine;

public class ChatUI : MonoBehaviour
{
    public GameObject chatPanel;
    public GameObject messagePrefab;
    public TMP_InputField inputField;
    public Transform messageContainer;
    public BasicSpawner spawner;
    
    private void Start()
    {
        // Tìm BasicSpawner nếu không được gán
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<BasicSpawner>();
        }
        
        // Đợi ChatManager được khởi tạo
        if (ChatManager.Instance == null)
        {
            Debug.LogWarning("[ChatUI] ChatManager not found yet, will retry...");
            Invoke(nameof(Start), 0.1f);
            return;
        }
        
        if (messagePrefab == null)
        {
            Debug.LogError("[ChatUI] Message prefab not assigned!");
            return;
        }
        
        if (messageContainer == null && chatPanel != null)
        {
            messageContainer = chatPanel.transform;
        }
        
        if (messageContainer == null)
        {
            Debug.LogError("[ChatUI] Message container not assigned!");
            return;
        }
        
        if (inputField == null)
        {
            Debug.LogError("[ChatUI] Input field not assigned!");
            return;
        }
        
        // Đăng ký sự kiện
        ChatManager.Instance.OnMessageReceived += AddMessage;
        ChatManager.Instance.OnPrivateMessageReceived += AddPrivateMessage;
        
        inputField.onSubmit.AddListener(OnSubmit);
        
        Debug.Log("[ChatUI] Initialized successfully");
    }
    
    private void OnDestroy()
    {
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.OnMessageReceived -= AddMessage;
            ChatManager.Instance.OnPrivateMessageReceived -= AddPrivateMessage;
        }
    }
    
    private void OnSubmit(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;
        
        // Lấy tên player từ PlayerDataManager
        string playerName = "Player";
        if (PlayerDataManager.Instance != null)
        {
            playerName = PlayerDataManager.Instance.GetPlayerName();
        }
        
        string content = message.Trim();
        
        // Kiểm tra là tin nhắn riêng
        if (content.StartsWith("/w "))
        {
            string[] parts = content.Split(' ', 3);
            if (parts.Length >= 3)
            {
                string target = parts[1];
                string msg = parts[2];
                ChatManager.Instance.SendPrivateMessage(playerName, msg, target);
            }
            else
            {
                Debug.LogWarning("Syntax: /w [player_name] [message]");
            }
        }
        else
        {
            // Gửi tin nhắn chung
            ChatManager.Instance.SendMessage(playerName, content);
        }
        
        inputField.text = "";
        inputField.ActivateInputField();
    }
    
    public void AddMessage(string sender, string message)
    {
        if (messagePrefab == null || messageContainer == null)
        {
            Debug.LogError("[AddMessage] Message prefab or container is null!");
            return;
        }
        
        try
        {
            GameObject newMessage = Instantiate(messagePrefab, messageContainer);
            TextMeshProUGUI messageText = newMessage.GetComponent<TextMeshProUGUI>();
            if (messageText != null)
            {
                messageText.text = $"<b>{sender}:</b> {message}";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AddMessage] Error: {ex.Message}");
        }
    }
    
    public void AddPrivateMessage(string sender, string message, string target)
    {
        if (messagePrefab == null || messageContainer == null)
            return;
        
        string localPlayerName = "Player";
        if (PlayerDataManager.Instance != null)
        {
            localPlayerName = PlayerDataManager.Instance.GetPlayerName();
        }
        
        // Chỉ hiển thị nếu là người gửi hoặc nhẫn
        if (localPlayerName != sender && localPlayerName != target)
            return;
        
        try
        {
            GameObject newMessage = Instantiate(messagePrefab, messageContainer);
            TextMeshProUGUI messageText = newMessage.GetComponent<TextMeshProUGUI>();
            if (messageText != null)
            {
                messageText.text = $"<color=yellow>[Private]</color> <b>{sender}</b> → <b>{target}:</b> {message}";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[AddPrivateMessage] Error: {ex.Message}");
        }
    }
}

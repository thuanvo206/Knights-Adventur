using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ChatUI : MonoBehaviour
{
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private GameObject messagePrefab;
    [SerializeField] private TMP_InputField chatInput;
    [SerializeField] private Transform messageContainer; // Panel chứa các tin nhắn
    [SerializeField] private BasicSpawner spawner; // Tham chiếu tới BasicSpawner

    private void Start()
    {
        // Tìm spawner nếu không được gán trong Inspector
        if (spawner == null)
        {
            spawner = FindFirstObjectByType<BasicSpawner>();
        }
        
        if (ChatManager.Instance == null)
        {
            Debug.LogError("ChatManager instance not found!");
            return;
        }
        
        // Đăng ký sự kiện khi nhận tin nhắn
        ChatManager.Instance.OnMessageReceived += AddMessage;
        ChatManager.Instance.OnPrivateMessageReceived += AddPrivateMessage;

        // Bắt sự kiện khi nhấn Enter trong ô Chat
        if (chatInput != null)
        {
            chatInput.onEndEdit.AddListener(delegate { OnSubmit(); });
        }
    }

    private void OnSubmit()
    {
        if (chatInput == null || string.IsNullOrEmpty(chatInput.text)) 
            return;

        // Lấy tên player từ PlayerDataManager
        string playerName = "Player";
        if (PlayerDataManager.Instance != null)
        {
            playerName = PlayerDataManager.Instance.GetPlayerName();
        }

        string content = chatInput.text.Trim();

        // Kiểm tra nếu là tin nhắn riêng (ví dụ cú pháp: /w [tên] [nội dung])
        if (content.StartsWith("/w "))
        {
            string[] parts = content.Split(' ', 3);
            if (parts.Length >= 3)
            {
                string target = parts[1];
                string msg = parts[2];
                if (ChatManager.Instance != null)
                {
                    ChatManager.Instance.SendPrivateMessageRpc(playerName, msg, target);
                }
            }
        }
        else if (!string.IsNullOrEmpty(content))
        {
            if (ChatManager.Instance != null)
            {
                ChatManager.Instance.SendMessageRpc(playerName, content);
            }
        }

        chatInput.text = ""; // Xóa ô nhập sau khi gửi
        chatInput.ActivateInputField(); // Giữ focus vào ô chat
    }

    public void AddMessage(string sender, string message)
    {
        if (messagePrefab == null || messageContainer == null)
        {
            Debug.LogError("Message prefab or container is not assigned!");
            return;
        }
        
        GameObject newMsg = Instantiate(messagePrefab, messageContainer);
        TMP_Text textComponent = newMsg.GetComponent<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = $"<b>{sender}:</b> {message}";
        }
    }

    public void AddPrivateMessage(string sender, string message, string target)
    {
        if (messagePrefab == null || messageContainer == null)
        {
            Debug.LogError("Message prefab or container is not assigned!");
            return;
        }
        
        string localPlayerName = "Player";
        if (PlayerDataManager.Instance != null)
        {
            localPlayerName = PlayerDataManager.Instance.GetPlayerName();
        }
        
        // Chỉ hiển thị nếu mình là người gửi hoặc người nhận
        if (localPlayerName == sender || localPlayerName == target)
        {
            GameObject newMsg = Instantiate(messagePrefab, messageContainer);
            TMP_Text textComponent = newMsg.GetComponent<TMP_Text>();
            if (textComponent != null)
            {
                textComponent.text = $"<color=magenta>[Riêng tư] {sender} -> {target}: {message}</color>";
            }
        }
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh lỗi bộ nhớ
        if (ChatManager.Instance != null)
        {
            ChatManager.Instance.OnMessageReceived -= AddMessage;
            ChatManager.Instance.OnPrivateMessageReceived -= AddPrivateMessage;
        }
    }
}
using System;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public static ChatManager Instance { get; private set; }
    
    public Action<string, string> OnMessageReceived;
    public Action<string, string, string> OnPrivateMessageReceived;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[ChatManager] Initialized");
    }
    
    public void SendMessage(string sender, string message)
    {
        if (string.IsNullOrEmpty(sender) || string.IsNullOrEmpty(message))
        {
            Debug.LogWarning("[ChatManager] Sender or message is empty!");
            return;
        }
        
        // Hiển thị tin nhắn cục bộ ngay lập tức
        OnMessageReceived?.Invoke(sender, message);
        Debug.Log($"[Chat] {sender}: {message}");
    }
    
    public void SendPrivateMessage(string sender, string message, string target)
    {
        if (string.IsNullOrEmpty(sender) || string.IsNullOrEmpty(message) || string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("[ChatManager] Sender, message, or target is empty!");
            return;
        }
        
        // Hiển thị tin nhắn riêng cục bộ
        OnPrivateMessageReceived?.Invoke(sender, message, target);
        Debug.Log($"[Private] {sender} -> {target}: {message}");
    }
}

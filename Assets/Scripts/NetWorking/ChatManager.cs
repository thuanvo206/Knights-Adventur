using System;
using Fusion;
using UnityEngine;

public class ChatManager : NetworkBehaviour
{
    public static ChatManager Instance { get; private set; }

    // Action để thông báo khi nhận tin nhắn: (người gửi, nội dung)
    public Action<string, string> OnMessageReceived;
    // Action cho tin nhắn riêng: (người gửi, nội dung, người nhận)
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
    }

    // RPC gửi tin nhắn chung cho tất cả mọi người
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void SendMessageRpc(string senderName, string message)
    {
        OnMessageReceived?.Invoke(senderName, message);
    }

    // RPC gửi tin nhắn riêng (Private Message)
    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    public void SendPrivateMessageRpc(string senderName, string message, string targetName)
    {
        OnPrivateMessageReceived?.Invoke(senderName, message, targetName);
    }
}
using Fusion;
using UnityEngine;

public class LobbyRunner : MonoBehaviour
{
    public BasicSpawner _BasicSpawner;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        await _BasicSpawner.StartLobbyAndRunner();
    }

    public async void CreateRoom(string roomName)
    {
        Debug.Log($" >>>>>>> Creating room: {roomName}");
        var scene = SceneRef.FromIndex(1); // index 1 là scene game chính
        await _BasicSpawner.StartHost(roomName, scene);
    }
    
    public async void JoinRoom(SessionInfo sessionInfo)
    {
        Debug.Log($" >>>>>>> Joining room: {sessionInfo.Name}");
        await _BasicSpawner.StartClient(sessionInfo.Name);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class BasicSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    private NetworkRunner _runner;
    public LobbyUI lobbyUI;
    public Transform spawnPoint;

    public void Awake()
    {
        if (_runner == null)
            _runner = gameObject.AddComponent<NetworkRunner>();

        DontDestroyOnLoad(gameObject);
    }

    public async Task StartLobbyAndRunner()
    {
        if (_runner == null) _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
        var res = await _runner.JoinSessionLobby(SessionLobby.ClientServer, "Game Lobby");
        if (res.Ok) Debug.Log(" >>>>>>> Joined lobby successfully");
        else Debug.LogError($" >>>>>>> Failed to join lobby: {res.ShutdownReason}");
    }

    public async Task StartHost(string roomName, SceneRef scene)
    {
        var res = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = roomName,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });
        if (res.Ok) Debug.Log(" >>>>>>> Host started successfully");
        else Debug.LogError($" >>>>>>> Failed to start host: {res.ShutdownReason}");
    }

    public async Task StartClient(string roomName)
    {
        var res = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Client,
            SessionName = roomName,
        });
        if (res.Ok) Debug.Log(" >>>>>>> Client started successfully");
        else Debug.LogError($" >>>>>>> Failed to start client: {res.ShutdownReason}");
    }

    public NetworkPrefabRef playerPrefab;
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters = new Dictionary<PlayerRef, NetworkObject>();

    // Scene 1 load lần đầu: OnSceneLoadDone chạy TRƯỚC OnPlayerJoined
    // → nếu không có flag này, OnSceneLoadDone spawn player trước,
    //   rồi OnPlayerJoined spawn thêm lần nữa → ra 2 con player
    private bool _initialSceneLoaded = false;

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            SpawnPlayer(runner, player);
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            if (_spawnedCharacters.TryGetValue(player, out var networkPlayerObject))
            {
                runner.Despawn(networkPlayerObject);
                _spawnedCharacters.Remove(player);
            }
        }
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        if (!runner.IsServer) return;

        // Bỏ qua lần đầu tiên (Scene 1) vì OnPlayerJoined sẽ tự spawn
        if (!_initialSceneLoaded)
        {
            _initialSceneLoaded = true;
            return;
        }

        // Từ Scene 2 trở đi: tìm SpawnPoint trong scene mới bằng Tag
        // Chỉ cần gắn tag "SpawnPoint" cho GameObject SpawnPoint trong mỗi scene
        GameObject spawnObj = GameObject.FindWithTag("SpawnPoint");
        if (spawnObj != null)
            spawnPoint = spawnObj.transform;
        else
            Debug.LogWarning("Không tìm thấy GameObject nào có tag 'SpawnPoint' trong scene này!");

        // Re-spawn các player (OnPlayerJoined không được gọi lại khi đổi scene)
        foreach (PlayerRef player in runner.ActivePlayers)
        {
            bool needsRespawn = !_spawnedCharacters.TryGetValue(player, out var existingObj)
                                || existingObj == null
                                || !existingObj.IsValid;

            if (needsRespawn)
            {
                SpawnPlayer(runner, player);
            }
        }
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        Vector2 spawnPosition = spawnPoint != null ? (Vector2)spawnPoint.position : Vector2.zero;
        var networkPlayerObject = runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player);
        _spawnedCharacters[player] = networkPlayerObject;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
        data.move = new Vector2(Input.GetAxisRaw("Horizontal"), 0);
        data.jumpPressed = Input.GetButton("Jump");
        input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (lobbyUI != null)
            lobbyUI.BuildRoomList(sessionList);
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
    public NetworkBool jumpPressed;
}
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
    // khai báo một NetworkRunner để quản lý kết nối mạng
    private NetworkRunner _runner;
    public LobbyUI lobbyUI; // tham chiếu đến LobbyUI để cập nhật danh sách phòng khi có thay đổi

    public void Awake()
    {
        // tạo một NetworkRunner mới và gán cho biến _runner
        if (_runner == null)
        {
            _runner = gameObject.AddComponent<NetworkRunner>();
        }

        // giữ cho object này không bị hủy khi load scene mới
        DontDestroyOnLoad(gameObject);
    }

    public async Task StartLobbyAndRunner()
    {
        if (_runner == null) _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true; // cho phép cung cấp input từ client
        _runner.AddCallbacks(this);
        // tham gia vào lobby
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

    // tham chiếu đến prefab của player để tạo ra khi có người chơi tham gia vào mạng
    public NetworkPrefabRef playerPrefab;

    // tạo 1 dictionary để lưu trữ các player đã tham gia vào mạng, với key là PlayerRef và value là NetworkObject
    private Dictionary<PlayerRef, NetworkObject> _spawnedCharacters =
        new Dictionary<PlayerRef, NetworkObject>();

    // player tham gia vào mạng
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($" >>>>>>> Player joined: {player}");
        // chỉ host mới có quyền tạo nhân vật cho player
        if (runner.IsServer)
        {
            // ngẫu nhiên 1 vị trí spawn cho player
            var spawnPosition = new Vector2(Random.Range(-5f, 5f), Random.Range(-5f, 5f));
            // tạo nhân vật tại vị trí spawn và gán cho player
            var networkPlayerObject = runner.Spawn(
                playerPrefab,
                spawnPosition,
                Quaternion.identity,
                player);
            // lưu trữ nhân vật đã tạo vào dictionary
            _spawnedCharacters[player] = networkPlayerObject;
            Debug.Log($" >>>>>>> Spawned player object for {player} at {spawnPosition}");
            Debug.Log($" >>>>>>> Current spawned characters: {_spawnedCharacters.Count}");
        }
    }

    // player rời khỏi mạng
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($" >>>>>>> Player left: {player}");
        // chỉ host mới có quyền hủy nhân vật của player
        if (runner.IsServer)
        {
            // kiểm tra nếu player có nhân vật đã tạo
            if (_spawnedCharacters.TryGetValue(player, out var networkPlayerObject))
            {
                // hủy nhân vật của player
                runner.Despawn(networkPlayerObject);
                // xóa player khỏi dictionary
                _spawnedCharacters.Remove(player);
                Debug.Log($" >>>>>>> Despawned player object for {player}");
                Debug.Log($" >>>>>>> Current spawned characters: {_spawnedCharacters.Count}");
            }
        }
    }

    // hàm thực hiện khi có input từ người chơi,
    // nhận vào một NetworkInput để xử lý
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var data = new NetworkInputData();
    data.move = new Vector2(Input.GetAxisRaw("Horizontal"), 0);

    // Kiểm tra phím Space (hoặc nút Jump mặc định)
    data.jumpPressed = Input.GetButton("Jump"); 

    input.Set(data);
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        // cập nhật danh sách phòng trên giao diện lobby khi có thay đổi
        if (lobbyUI != null)
        {
            lobbyUI.BuildRoomList(sessionList);
        }
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }
}

public struct NetworkInputData : INetworkInput
{
    public Vector2 move;
    public NetworkBool jumpPressed; // Thêm dòng này để truyền trạng thái nhảy
}
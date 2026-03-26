using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    public LobbyRunner
        lobbyRunner; // tham chiếu đến LobbyRunner để gọi phương thức CreateRoom khi người chơi nhấn vào nút tạo phòng

    public TMP_InputField roomNameInput; // tham chiếu đến TMP_InputField để người chơi nhập tên phòng khi tạo phòng
    public Button createRoomButton; // tham chiếu đến Button để người chơi có thể nhấn vào để tạo phòng

    public Transform roomListContainer; // tham chiếu đến Transform để chứa danh sách các phòng hiện có
    public RoomListItem roomListItemPrefab; // tham chiếu đến prefab của RoomListItem

    readonly List<RoomListItem>
        _roomListItems = new(); // danh sách các RoomListItem hiện có để quản lý và xóa khi cần thiết

    public void BuildRoomList(List<SessionInfo> sessionInfos)
    {
        // xóa tất cả các RoomListItem hiện có trước khi xây dựng lại danh sách phòng
        foreach (var item in _roomListItems)
        {
            Destroy(item.gameObject);
        }

        _roomListItems.Clear();

        // tạo một RoomListItem mới cho mỗi SessionInfo trong danh sách sessionInfos và thêm vào container
        foreach (var info in sessionInfos)
        {
            var item = Instantiate(roomListItemPrefab, roomListContainer);
            item.InitItem(info, lobbyRunner); // khởi tạo RoomListItem với thông tin phòng và tham chiếu đến LobbyRunner
            _roomListItems.Add(item); // thêm RoomListItem mới vào danh sách quản lý
        }
    }

    void Start()
    {
        createRoomButton.onClick.AddListener(
            OnCreateRoomButtonClicked); // thêm sự kiện khi người chơi nhấn vào nút tạo phòng sẽ gọi phương thức OnCreateRoomButtonClicked
    }
    
    void OnCreateRoomButtonClicked()
    {
        var roomName = roomNameInput.text; // lấy tên phòng từ input field
        if (!string.IsNullOrEmpty(roomName))
        {
            lobbyRunner.CreateRoom(roomName); // gọi phương thức CreateRoom của LobbyRunner để tạo phòng mới với tên đã nhập
        }
    }
}
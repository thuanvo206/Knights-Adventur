using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomListItem : MonoBehaviour
{
    public TextMeshProUGUI roomNameText; // tham chiếu đến TextMeshProUGUI để hiển thị tên phòng
    public Button joinButton; // tham chiếu đến Button để người chơi có thể nhấn vào để tham gia phòng
    
    SessionInfo sessionInfo; // lưu trữ thông tin về phòng này để sử dụng khi người chơi nhấn vào nút join
    LobbyRunner lobbyRunner; // tham chiếu đến LobbyRunner để gọi phương thức JoinRoom khi người chơi nhấn vào nút join
    
    public void InitItem(SessionInfo info, LobbyRunner runner)
    {
        sessionInfo = info; // gán thông tin phòng cho biến sessionInfo
        lobbyRunner = runner; // gán tham chiếu đến LobbyRunner cho biến lobbyRunner
        roomNameText.text = info.Name; // hiển thị tên phòng trên giao diện
        joinButton.interactable = true; // cho phép người chơi nhấn vào nút join
        joinButton.onClick.AddListener(OnJoinButtonClicked); // thêm sự kiện khi người chơi nhấn vào nút join sẽ gọi phương thức OnJoinButtonClicked
    }
    
    void OnJoinButtonClicked()
    {
        if (lobbyRunner != null && sessionInfo != null)
        {
            lobbyRunner.JoinRoom(sessionInfo); // gọi phương thức JoinRoom của LobbyRunner để tham gia vào phòng này
        }
    }
}

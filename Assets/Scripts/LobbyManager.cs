using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("InputFields")] 
    [SerializeField] private InputField createInput;
    [SerializeField] private InputField joinInput;

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createInput.text , new RoomOptions() { MaxPlayers = 4 , IsOpen = true , IsVisible = true} , TypedLobby.Default , null);
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }
    public void JoinRoomInList(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }
    public override void OnJoinedRoom()
    {
        PhotonNetwork.LoadLevel("Level");
    }
}

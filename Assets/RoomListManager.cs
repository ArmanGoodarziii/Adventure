using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class RoomListManager : MonoBehaviourPunCallbacks
{
    public GameObject roomPrefab;
    public GameObject[] AllRooms;
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        for(int i = 0 ; i < AllRooms.Length ; i++)
        {
            if(AllRooms[i] != null)
                Destroy(AllRooms[i]);
        }

        AllRooms = new GameObject[roomList.Count];

        for(int i = 0 ; i < roomList.Count ; i++)
        {
            if(roomList[i].IsOpen && roomList[i].IsVisible && roomList[i].PlayerCount >= 1)
            {
                GameObject Room = Instantiate(roomPrefab , Vector3.zero , Quaternion.identity , GameObject.Find("Content").transform);
                Room.GetComponent<RoomButton>().nameText.text = roomList[i].Name;

                AllRooms[i] = Room;
            }
        }
    }
}

using UnityEngine;
using Photon.Pun;

public class Spawner : MonoBehaviour
{
    void Start()
    {
        PhotonNetwork.Instantiate("PlayerPrefab" , new Vector3(Random.Range(-5,-4.5f), 2 , 0) , Quaternion.identity);
    }
}

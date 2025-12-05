using UnityEngine;
using Photon.Pun;

public class DeathZone : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            PhotonView pv = player.GetComponent<PhotonView>();

            // فقط صاحب محلی (کلاینت مربوطه) باید اعلام باخت کند
            if (pv != null && pv.IsMine)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RequestPlayerLose(pv.OwnerActorNr);
                    player.Die();
                }
            }
        }
    }
}

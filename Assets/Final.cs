using UnityEngine;
using Photon.Pun;

public class Final : MonoBehaviour
{
    [SerializeField] private GameObject collectedEffect;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        PhotonView pv = collision.GetComponent<PhotonView>();
        if (pv == null)
            return;

        // فقط بازیکن صاحب (Owner) اجازه ارسال درخواست برد را دارد
        if (pv.IsMine)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RequestPlayerWin(pv.OwnerActorNr);
            }
        }

        // نمایش افکت (لوکال)
        if (collectedEffect != null)
            Instantiate(collectedEffect, transform.position, Quaternion.identity);

        // حذف آیتم (اگر شبکه‌ای باشد Destroy این هم کافی است)
        Destroy(gameObject);
    }
}

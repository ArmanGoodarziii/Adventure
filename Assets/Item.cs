using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private GameObject collectedEffect;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Instantiate(collectedEffect , transform.position , Quaternion.identity);
            Destroy(gameObject);
        }
    }
}

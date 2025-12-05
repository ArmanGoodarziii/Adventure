using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Rigidbody2D rb;
    private float xScale;
    private float faceDir = -1;
    private bool faceRight;
    [Header("Movement")]
    [SerializeField] private float speed;

    [Header("Collision")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private Transform checkNoGroundTransform;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask layerGround;

    private bool isGrounded;
    private bool checkNoGround;
    private bool isWall;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        xScale = transform.localScale.x;
    }
    void Update()
    {
        HandleCollision();
        HandleFlip();
    }
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(speed * faceDir, rb.linearVelocity.y);
    }
    private void HandleFlip()
    {
        if (isGrounded)
        {
            if(isWall || !checkNoGround)
            {
                faceRight = !faceRight;
            }
        }
        if (faceRight)
        {
            faceDir = 1;
            transform.localScale = new Vector3(-xScale , transform.localScale.y , transform.localScale.z);
        }
        if (!faceRight)
        {
            faceDir = -1;
            transform.localScale = new Vector3(xScale , transform.localScale.y , transform.localScale.z);
        }
    }
    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position , Vector2.down , groundCheckDistance , layerGround);
        checkNoGround = Physics2D.Raycast(checkNoGroundTransform.position , Vector2.down , groundCheckDistance , layerGround);
        isWall = Physics2D.Raycast(transform.position , Vector2.right * faceDir , wallCheckDistance , layerGround);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position , new Vector2(transform.position.x , transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(checkNoGroundTransform.position , new Vector2(checkNoGroundTransform.position.x , checkNoGroundTransform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position , new Vector2(transform.position.x + (faceDir * wallCheckDistance) , transform.position.y));
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player>() != null)
        {
            collision.GetComponent<Player>().Hit();
        }
    }
}

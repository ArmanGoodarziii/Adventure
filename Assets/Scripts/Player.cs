using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviourPun
{
    private InputSystem_Actions actions;
    private PhotonView pv;
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Health")]
    private float health = 3;

    [Header("Flip")]
    private float xScale;
    private bool faceRight = true;
    private float faceDir = 1;

    [Header("Movement")]
    private float move;
    [SerializeField] private float speed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float wallJump;

    [Header("Collision")]
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private float wallCheckDistance;
    [SerializeField] private LayerMask layerGround;
    
    public GameObject cameraObject;
    public GameObject canvasObject;
    public float yPos;
    public Text username;

    private bool isGrounded;
    private bool isWall;
    private bool isWallJumping;
    private bool isHit;
    private bool dontHit;
    

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        
        if (pv.IsMine)
        {
            cameraObject.SetActive(true);
            actions = new InputSystem_Actions();
        }
        else
        {
            cameraObject.SetActive(false);
        }
    }
    void OnEnable()
    {
        if (pv.IsMine)
        {
            actions.Player.Enable();
            actions.Player.Move.performed += Movement;
            actions.Player.Jump.performed += Jumping;
            actions.Player.Move.canceled += Movement;
            actions.Player.Jump.canceled += Jumping;
        }
        
    }
    void OnDisable()
    {
        if (pv.IsMine)
        {
            actions.Player.Disable();
            actions.Player.Move.performed -= Movement;
            actions.Player.Jump.performed -= Jumping;
        }
    }
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        xScale = transform.localScale.x;

        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 60;

        username.text = GetComponent<PhotonView>().Controller.NickName;
    }

    void Update()
    {
        if(health <= 0)
        {
            // قبلی: GetComponent<GameManager>().GameOver();
            if (pv != null && pv.IsMine)
            {
                GameManager.Instance.RequestPlayerLose(pv.OwnerActorNr);
            }
            // اگر pv.IsMine نیست، نخواهم کرد چون باید فقط مالک درخواست بفرسته
        }   
        if(!pv.IsMine) return;

        if (isWall)
        {
            rb.linearDamping = 8;
        }
        else
        {
            rb.linearDamping = 0;
        }

        HandleCollision();
        HandleFlip(move);
        HandleAnimation();

        cameraObject.transform.position = Vector3.Lerp(cameraObject.transform.position , new Vector3(transform.position.x , transform.position.y , cameraObject.transform.position.z) , Time.deltaTime);
    }
    void FixedUpdate()
    {
        if(!pv.IsMine) return;

        if(isWallJumping) return;
        if(isHit) return;

        rb.linearVelocity = new Vector2(move * speed , rb.linearVelocity.y);

    }
    void LateUpdate()
    {
        canvasObject.transform.position = new Vector3(transform.position.x , transform.position.y + yPos , transform.position.z);
    }
    private void Movement(InputAction.CallbackContext ctx)
    {
        move = ctx.ReadValue<Vector2>().x;
    }
    private void Jumping(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
        else if (!isGrounded && isWall)
        {
            Flip(!faceRight);
            rb.linearVelocity = new Vector2(faceRight ? wallJump : -wallJump, jumpForce);
        }
        StopAllCoroutines();
        GetComponent<SpriteRenderer>().enabled = true;
        dontHit = false;
        StartCoroutine(WallJumping());
    }

    private IEnumerator WallJumping()
    {
        isWallJumping = true;
        yield return new WaitForSeconds(0.7f);
        isWallJumping = false;
    }
    public void Hit()
    {
        if(isHit) return;
        if(dontHit) return;

        health --; 
        animator.SetTrigger("isHit");

        isWallJumping = false;

        if (faceRight)
        {
            rb.linearVelocity = new Vector2(-4 , rb.linearVelocity.y);
        }
        else if (!faceRight)
        {
            rb.linearVelocity = new Vector2(4 , rb.linearVelocity.y);
        }
        StopAllCoroutines();
        StartCoroutine(Hitting());
    }
    private IEnumerator Hitting()
    {
        isHit = true;
        yield return new WaitForSeconds(0.3f);
        isHit = false;
        StartCoroutine(HittingAnimation());
    }
    private IEnumerator HittingAnimation()
    {
        dontHit = true;
        GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().enabled = true;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().enabled = true;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().enabled = true;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().enabled = false;
        yield return new WaitForSeconds(0.1f);
        GetComponent<SpriteRenderer>().enabled = true;
        yield return new WaitForSeconds(0.1f);
        dontHit = false;
    }

    private void HandleFlip(float direction)
    {
        if (direction > 0 && !faceRight)
            Flip(true);
        else if (direction < 0 && faceRight)
            Flip(false);
    }

    private void Flip(bool right)
    {
        faceRight = right;
        faceDir = right ? 1f : -1f;

        transform.localScale = new Vector3(
            right ? xScale : -xScale,
            transform.localScale.y,
            transform.localScale.z
        );
    }
    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position , Vector2.down , groundCheckDistance , layerGround);
        isWall = Physics2D.Raycast(transform.position , Vector2.right * faceDir , wallCheckDistance , layerGround);
    }
    private void HandleAnimation()
    {
        animator.SetFloat("xVelocity" , rb.linearVelocity.x);
        animator.SetFloat("yVelocity" , rb.linearVelocity.y);
        animator.SetBool("isGrounded" , isGrounded);
        animator.SetBool("isWall" , isWall);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position , new Vector2(transform.position.x , transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (faceDir * wallCheckDistance) , transform.position.y));
    }
        public void Die()
    {
        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);

            if(canvasObject != null)
                Destroy(canvasObject);
        }
    }
}

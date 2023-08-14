using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Player player;
    private CharacterController cc;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Animator anim;

    public float moveSpeed = 5;
    public Vector2 playerDirectionalInputs;
    [SerializeField] private Vector3 moveDir;
    [SerializeField] float gravity = -9.81f;
    Vector3 lastFacingDirection = Vector3.right;
    float lastXDirection = 1;


    [Header("Sliding")]
    public float slideSpeed = 10;
    [SerializeField] private Vector3 slideDir;
    [SerializeField] private float slideCooldown = 1f;

    [Header("Ground")]
    public float playerHeight;
    public float groundDrag;
    public LayerMask groundMask;

    [Header("Jumping")]   
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float jumpCooldown = 1f;
    bool wasJumpPressed;
    float initialJumpVelocity;
    [SerializeField] private float maxJumpHeight = 1f;
    float maxJumpTime = 0.5f;

    public bool isGrounded;
    public bool isJumping;
    public bool isSliding;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        player = GetComponent<Player>();
    }

    // Start is called before the first frame update
    void Start()
    {
        // Set up variables for jumping
        float apexJumpTime = maxJumpTime / 2;
        gravity = (-2 * maxJumpHeight) / Mathf.Pow(apexJumpTime, 2);
        initialJumpVelocity = (2 * maxJumpHeight) / apexJumpTime;
    }

    // Update is called once per frame
    void Update()
    {
        ReadInputs();
        GroundCheck();
        MovePlayer();
        HandleGravity();
        HandleJumping();

        UpdateAnimations();    
    }

    public void ForceFlip(bool tf)
    {
        sr.flipX = tf;
    }

    void UpdateAnimations()
    {
        if (player.disabled)
        {
            anim.SetBool("isMoving", false);
            return;
        }    

        GetFacingDirection();

        if (lastXDirection < 0 || player.gameOver)
            sr.flipX = false;
        else
            sr.flipX = true;

        anim.SetBool("isMoving", playerDirectionalInputs.x != 0 || !isGrounded);
        anim.SetBool("isSliding", isSliding);

        float yDir;
        // Diagonal
        if (playerDirectionalInputs.x != 0 && playerDirectionalInputs.y > 0)
            yDir = 0.5f;
        // Looking down
        else if (playerDirectionalInputs.y < 0)
            yDir = 0;
        else
            yDir = playerDirectionalInputs.y;

        anim.SetFloat("yDirection", yDir);
    }

    void ReadInputs()
    {
        // Don't do anything if player is dead (aside from falling)
        if (!player.alive || player.disabled)
            return;

        playerDirectionalInputs = new Vector3(Input.GetAxisRaw("Horizontal_" + player.inputKeyIdentifier), Input.GetAxisRaw("Vertical_" + player.inputKeyIdentifier));   

        if (Input.GetKeyDown(player.jumpKey) && isGrounded)
            wasJumpPressed = true;
        else
            wasJumpPressed = false;

        if (Input.GetKeyDown(player.slideKey) && !isSliding && isGrounded && playerDirectionalInputs.x != 0)
        {
            //transform.localScale = new Vector3(transform.localScale.x, 0.5f, transform.localScale.z);
            isSliding = true;
            slideDir.x = playerDirectionalInputs.x * slideSpeed;
            Invoke(nameof(StopSliding), slideCooldown);
        }
        else
            moveDir.x = playerDirectionalInputs.x * moveSpeed;

        moveDir.z = 0;
    }

    void StopSliding()
    {
        //transform.localScale = new Vector3(transform.localScale.x, 1f, transform.localScale.z);
        isSliding = false;
    }

    void MovePlayer()
    {
        // Don't do anything if player is dead (aside from falling)
        if (player.disabled)
            return;

        if (!player.alive)
            moveDir.x = 0;  

        if (!isSliding)
            cc.Move(moveDir * Time.deltaTime);
        else
            cc.Move(slideDir * Time.deltaTime);
    }

    void HandleGravity()
    {
        // Using CharacterController's isGrounded because it's less reliable
        // CheckSphere ground check would kill all y velocity if the user goes through a 1-way platform
        if (cc.isGrounded && !isJumping)
            moveDir.y = 0;
        else
            moveDir.y += gravity * Time.deltaTime;
    }

    void HandleJumping()
    {
        if (isGrounded && wasJumpPressed && !isJumping)
        {
            isJumping = true;
            moveDir.y = initialJumpVelocity * jumpForce;
        }
        else if(isGrounded && !wasJumpPressed && isJumping)
        {
            isJumping = false;
        }
    }

    private void GroundCheck()
    {
        //isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight / 2 + 0.2f, groundMask);
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - 0.6f, transform.position.z);
        isGrounded = Physics.CheckSphere(spherePosition, 0.45f, groundMask, QueryTriggerInteraction.Ignore);
    }

    public Vector3 GetFacingDirection()
    {
        
        if (playerDirectionalInputs != Vector2.zero)
        {
            if (playerDirectionalInputs.x != 0)
                lastXDirection = playerDirectionalInputs.x;

            lastFacingDirection = new Vector3(playerDirectionalInputs.x, playerDirectionalInputs.y, 0);
        }
        else
            lastFacingDirection = new Vector3(lastXDirection, 0, 0);


        return lastFacingDirection;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.yellow;
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - 0.6f, transform.position.z);
        Gizmos.DrawSphere(spherePosition, 0.45f);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    // animation stuff
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    //phsyics stuff
    private Rigidbody2D rigidBody;
    public Transform feet;
    public Transform front;
    public Transform back;
    public LayerMask whatIsGround;
    public float checkRadius;
    public float ground_acceleration;
    public float ground_deceleration;
    public float air_acceleration;
    public float air_deceleration;
    public float maxSpeed;
    public float jumpSpeed;
    public float wallSlidingSpeed;
    public float wallJumpTime;
    public float xWallForce;
    public float yWallForce;
    private float dt;
    private bool wallSliding;
    private bool wallJumping;

    void Start()
    {
        // get rigidbody component and freeze rotation on it
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        dt = Time.fixedDeltaTime;
        wallSliding = false;
        wallJumping = false;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }


    void Move()
    {
        // keeps track of new velocity to set as rigidBody velocity
        Vector2 movement = Vector2.zero;

        //checks to see if grounded
        bool isGrounded = Physics2D.OverlapCircle(feet.position, checkRadius, whatIsGround);

        float acceleration = isGrounded ? ground_acceleration : air_acceleration;
        float deceleration = isGrounded ? ground_deceleration : air_deceleration;

        // Handles walking physics
        float x_input = Input.GetAxisRaw("Horizontal");
        if (x_input > 0) // handles acceleration
        {
            movement.x = rigidBody.velocity.x + maxSpeed * dt * acceleration;
            spriteRenderer.flipX = false; // flips sprite
        }
        else if (x_input < 0)
        {
            movement.x = rigidBody.velocity.x + -maxSpeed * dt * acceleration;
            spriteRenderer.flipX = true; // flips sprite
        }
        else // handles deceleration
        {
            if (rigidBody.velocity.x > 0)
            {
                movement.x = rigidBody.velocity.x - maxSpeed * dt * deceleration;
                if (movement.x < 0)
                {
                    movement.x = 0;
                }
            }
            else if (rigidBody.velocity.x < 0)
            {
                movement.x = rigidBody.velocity.x + maxSpeed * dt * deceleration;
                if (movement.x > 0)
                {
                    movement.x = 0;
                }
            }
            else
            {
                movement.x = 0;
            }
        }

        // caps velocity by maxSpeed
        if (movement.x > maxSpeed)
        {
            movement.x = maxSpeed;
        }
        else if (movement.x < -maxSpeed)
        {
            movement.x = -maxSpeed;
        }

        // handles jumping
        if (Input.GetButton("Jump") && isGrounded)
        {
            movement.y = jumpSpeed;
        }
        else
        {
            movement.y = rigidBody.velocity.y;
        }

        // handles wall jumping/sliding
        bool isWalledFront = Physics2D.OverlapCircle(front.position, checkRadius, whatIsGround);
        bool isWalledBack = Physics2D.OverlapCircle(back.position, checkRadius, whatIsGround);

        // determines whether wall sliding or not
        if(((isWalledFront && x_input > 0) || (isWalledBack && x_input < 0)) && !isGrounded)
        {
            wallSliding = true;
        }
        else
        {
            wallSliding = false;
        }

        // if wall sliding, cap velocity
        if(wallSliding)
        {
            movement = new Vector2(movement.x, Mathf.Clamp(movement.y, -wallSlidingSpeed, float.MaxValue));
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        // handle wall jump
        if(wallSliding && Input.GetButton("Jump"))
        {
            wallJumping = true;
            Invoke("SetWallJumpingToFalse", wallJumpTime);
        }

        if(wallJumping)
        {
            movement.x = xWallForce * -x_input;
            movement.y = yWallForce;
        }




        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("Speed", Mathf.Abs(Input.GetAxis("Horizontal")));

        // applies new velocity to player Rigidbody2D
        rigidBody.velocity = movement;
    }

    void SetWallJumpingToFalse()
    {
        wallJumping = false;
    }


}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{

    public GameObject DeadUI;

    // Constants for heart rate
    public float UNSTRESSED_GROUND_ACCELERATION;
    public float UNSTRESSED_GROUND_DECELERATION;
    public float UNSTRESSED_AIR_ACCELERATION;
    public float UNSTRESSED_AIR_DECELERATION;
    public float UNSTRESSED_MAX_SPEED;
    public float UNSTRESSED_JUMP_SPEED;
    public float UNSTRESSED_WALL_SLIDING_SPEED;
    public float UNSTRESSED_X_WALL_FORCE;
    public float UNSTRESSED_Y_WALL_FORCE;

    public float STRESSED_GROUND_ACCELERATION;
    public float STRESSED_GROUND_DECELERATION;
    public float STRESSED_AIR_ACCELERATION;
    public float STRESSED_AIR_DECELERATION;
    public float STRESSED_MAX_SPEED;
    public float STRESSED_JUMP_SPEED;
    public float STRESSED_WALL_SLIDING_SPEED;
    public float STRESSED_X_WALL_FORCE;
    public float STRESSED_Y_WALL_FORCE;

    // Player States
    private bool isStressed;
    private bool Alive;

    // animation stuff
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    //phsyics stuff
    private Rigidbody2D rigidBody;
    public Transform feet;
    public Transform front;
    public Transform back;
    public Transform topLeft;
    public Transform bottomRight;
    public LayerMask whatIsGround;
    public LayerMask whatIsEnemy;
    public float checkRadius;
    private float ground_acceleration;
    private float ground_deceleration;
    private float air_acceleration;
    private float air_deceleration;
    private float maxSpeed;
    private float jumpSpeed;
    private float wallSlidingSpeed;
    private float xWallForce;
    private float yWallForce;
    private bool wallSliding;
    private bool wallJumping;
    public float wallJumpTime;
    private float dt;

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
        isStressed = true;
        Alive = true;
        Destress();
    }

    // Update is called once per frame
    void Update()
    {
        if (Alive)
        {
            Move();
            Dead();
        }
    }


    void Move()
    {

        Vector2 movement;
        bool isGrounded = Physics2D.OverlapCircle(feet.position, checkRadius, whatIsGround);

        float acceleration = isGrounded ? ground_acceleration : air_acceleration;
        float deceleration = isGrounded ? ground_deceleration : air_deceleration;

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

        bool isWalledFront = Physics2D.OverlapCircle(front.position, checkRadius, whatIsGround);
        bool isWalledBack = Physics2D.OverlapCircle(back.position, checkRadius, whatIsGround);

        // determines whether wall sliding or not
        if (((isWalledFront && x_input > 0) || (isWalledBack && x_input < 0)) && !isGrounded)
        {
            wallSliding = true;
        }
        else
        {
            wallSliding = false;
        }

        // if wall sliding, cap velocity
        if (wallSliding)
        {
            movement = new Vector2(movement.x, Mathf.Clamp(movement.y, -wallSlidingSpeed, float.MaxValue));
            spriteRenderer.flipX = !spriteRenderer.flipX;
        }

        // handle wall jump
        if (wallSliding && Input.GetButton("Jump"))
        {
            wallJumping = true;
            Invoke("SetWallJumpingToFalse", wallJumpTime);
        }

        if (wallJumping)
        {
            movement.x = xWallForce * -x_input;
            movement.y = yWallForce;
        }

        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("Speed", Mathf.Abs(Input.GetAxis("Horizontal")));

        rigidBody.velocity = movement;
    }



    void Dead()
    {
        bool colliding = Physics2D.OverlapArea(topLeft.position, bottomRight.position, whatIsEnemy);
        if (colliding)
        {
            Alive = false;
            DeadUI.GetComponent<DeathMenu>().OpenDeathMenu();
        }
    }

    // sets wall jump
    void SetWallJumpingToFalse()
    {
        wallJumping = false;
    }

    // changes physics values to stressed values
    public void Stress()
    {
        if (isStressed)
        {
            return;
        }
        ground_acceleration = STRESSED_GROUND_ACCELERATION;
        ground_deceleration = STRESSED_GROUND_DECELERATION;
        air_acceleration = STRESSED_AIR_ACCELERATION;
        air_deceleration = STRESSED_AIR_DECELERATION;
        maxSpeed = STRESSED_MAX_SPEED;
        jumpSpeed = STRESSED_JUMP_SPEED;
        wallSlidingSpeed = STRESSED_WALL_SLIDING_SPEED;
        xWallForce = STRESSED_X_WALL_FORCE;
        yWallForce = STRESSED_Y_WALL_FORCE;
        isStressed = true;
    }

    // changes physics values to unstressed values
    public void Destress()
    {
        if (!isStressed)
        {
            return;
        }
        ground_acceleration = UNSTRESSED_GROUND_ACCELERATION;
        ground_deceleration = UNSTRESSED_GROUND_DECELERATION;
        air_acceleration = UNSTRESSED_AIR_ACCELERATION;
        air_deceleration = UNSTRESSED_AIR_DECELERATION;
        maxSpeed = UNSTRESSED_MAX_SPEED;
        jumpSpeed = UNSTRESSED_JUMP_SPEED;
        wallSlidingSpeed = UNSTRESSED_WALL_SLIDING_SPEED;
        xWallForce = UNSTRESSED_X_WALL_FORCE;
        yWallForce = UNSTRESSED_Y_WALL_FORCE;
        isStressed = false;
    }

}

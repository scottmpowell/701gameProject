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
    public float acceleration;
    public float deceleration;
    public float maxSpeed;
    public float jumpSpeed;
    private float dt;
    private bool isGrounded;
    //do jump enumeration shit

    // Start is called before the first frame update
    void Start()
    {
        // get rigidbody component and freeze rotation on it
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        dt = Time.fixedDeltaTime;

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

        animator.SetBool("Grounded", isGrounded);
        animator.SetFloat("Speed", Mathf.Abs(Input.GetAxis("Horizontal")));

        // applies new velocity to player Rigidbody2D
        rigidBody.velocity = movement;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (CollisionIsWithGround(collision))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (!CollisionIsWithGround(collision))
        {
            isGrounded = false;
        }
    }

    private bool CollisionIsWithGround(Collision2D collision)
    {
        bool is_with_ground = false;
        foreach (ContactPoint2D c in collision.contacts)
        {
            Vector2 collision_direction_vector = c.point - rigidBody.position;
            if (collision_direction_vector.y < 0)
            {
                is_with_ground = true;
            }
        }

        return is_with_ground;
    }


}

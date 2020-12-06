using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Constants for heart rate
    public float UNSTRESSED_GROUND_ACCELERATION = 0.6F;
    public float UNSTRESSED_GROUND_DECELERATION = 2F;
    public float UNSTRESSED_AIR_ACCELERATION = 0.5F;
    public float UNSTRESSED_AIR_DECELERATION = 0.2F;
    public float UNSTRESSED_MAX_SPEED = 6F;
    public float UNSTRESSED_JUMP_SPEED = 5.4F;
    public float STRESSED_GROUND_ACCELERATION = 0.4F;
    public float STRESSED_GROUND_DECELERATION = 1F;
    public float STRESSED_AIR_ACCELERATION = 0.5F;
    public float STRESSED_AIR_DECELERATION = 0.2F;
    public float STRESSED_MAX_SPEED = 1F;
    public float STRESSED_JUMP_SPEED = 7.4F;

    // Stress Boolean
    public bool isStressed;
    
    // animation stuff
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    //phsyics stuff
    private Rigidbody2D rigidBody;
    public float ground_acceleration;
    public float ground_deceleration;
    public float air_acceleration;
    public float air_deceleration;
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

    public void Stress()
    {
	if (isStressed) {
		return;
	}
	ground_acceleration = STRESSED_GROUND_ACCELERATION;
	ground_deceleration = STRESSED_GROUND_DECELERATION;
	air_acceleration = STRESSED_AIR_ACCELERATION;
	air_deceleration = STRESSED_AIR_DECELERATION;
	maxSpeed = STRESSED_MAX_SPEED;
	jumpSpeed = STRESSED_JUMP_SPEED;
	isStressed = true;
    }

    public void Destress()
    {
	if (!isStressed) {
		return;
	}
	ground_acceleration = UNSTRESSED_GROUND_ACCELERATION;
	ground_deceleration = UNSTRESSED_GROUND_DECELERATION;
	air_acceleration = UNSTRESSED_AIR_ACCELERATION;
	air_deceleration = UNSTRESSED_AIR_DECELERATION;
	maxSpeed = UNSTRESSED_MAX_SPEED;
	jumpSpeed = UNSTRESSED_JUMP_SPEED;
	isStressed = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class PlayerController : MonoBehaviour {

    //animation stuff
    public SkeletonAnimation skeletonAnimation;
    public AnimationReferenceAsset idle, walk, jump, run;
    public string currentState;
    public string currentAnimation;


    // physics stuff
    private Rigidbody2D rigidBody;
    public float acceleration;
    public float deceleration;
    public float maxSpeed;
    public float jumpSpeed;
    private bool isJumping;
    private bool isGrounded;


    // Start is called before the first frame update
    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation; // prevents sprite from rotating
        currentState = "Idle";
        isJumping = false;
        SetCharacterState(currentState);
    }

    // Update is called once per frame
    void Update() {
        Move();
    }

    public void Move()
    {
        // keeps track of new velocity to set as rigidBody velocity
        Vector2 movement = Vector2.zero;

        // Handles walking physics
        float x_input = Input.GetAxis("Horizontal");
        if (x_input > 0) // handles acceleration
        {
            movement.x = rigidBody.velocity.x + maxSpeed * Time.deltaTime * acceleration;
            transform.localScale = new Vector2(1f, 1f); // flips sprite
        }
        else if (x_input < 0)
        {
            movement.x = rigidBody.velocity.x + -maxSpeed * Time.deltaTime * acceleration;
            transform.localScale = new Vector2(-1f, 1f); // flips sprite
        }
        else // handles deceleration
        {
            if (rigidBody.velocity.x > 0)
            {
                movement.x = rigidBody.velocity.x - maxSpeed * Time.deltaTime * deceleration;
                if (movement.x < 0)
                {
                    movement.x = 0;
                }
            }
            else if (rigidBody.velocity.x < 0)
            {
                movement.x = rigidBody.velocity.x + maxSpeed * Time.deltaTime * deceleration;
                if(movement.x > 0)
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
        } else if (movement.x < -maxSpeed)
        {
            movement.x = -maxSpeed;
        }

        // handles jumping
        if (Input.GetButton("Jump") && isGrounded)
        {
            isJumping = true;
            movement.y = jumpSpeed;
        } 
        else
        {
            movement.y = rigidBody.velocity.y;
        }

        // handles some animation
        if ((movement.x > 8 || movement.x < -8) && !isJumping && isGrounded)
        {
            SetCharacterState("Run");
        }
        else if (movement.x != 0 && !isJumping && isGrounded)
        {
            SetCharacterState("Walk");
        } 
        else if (movement.x == 0 && !isJumping && isGrounded)
        {
            SetCharacterState("Idle");
        } else if(isJumping && isGrounded)
        {
            SetCharacterState("Jump");
        }
         
        // applies new velocity to player Rigidbody2D
        rigidBody.velocity = movement;

    }

    public void SetAnimation(AnimationReferenceAsset animation, bool loop, float timeScale)
    {

        if (animation.name.Equals(currentAnimation))
        {
            return;
        }
        skeletonAnimation.state.SetAnimation(0, animation, loop).TimeScale = timeScale;
        currentAnimation = animation.name;
    }

    public void SetCharacterState(string state)
    {
        if (state.Equals("Idle"))
        {
            SetAnimation(idle, true, 1f);
        }
        else if (state.Equals("Walk"))
        {
            SetAnimation(walk, true, 1f);
        }
        else if (state.Equals("Run"))
        {
            SetAnimation(run, true, 1f);
        }
        else if (state.Equals("Jump"))
        {
            SetAnimation(jump, false, .6f);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if(CollisionIsWithGround(collision))
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(!CollisionIsWithGround(collision))
        {
            isJumping = false;
            isGrounded = false;
        }
    }

    private bool CollisionIsWithGround(Collision2D collision)
    {
        bool is_with_ground = false;
        foreach(ContactPoint2D c in collision.contacts)
        {
            Vector2 collision_direction_vector = c.point - rigidBody.position;
            if(collision_direction_vector.y < 0)
            {
                is_with_ground = true;
            }
        }

        return is_with_ground;
    }


}

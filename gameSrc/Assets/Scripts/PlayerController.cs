using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using TMPro;

public class PlayerController : MonoBehaviour {

    public SkeletonAnimation skeletonAnimation;
    public AnimationReferenceAsset idle, walk, jump;
    public string currentState;
    public string currentAnimation;
    public float max_speed;
    public float movement_scalar;
    public float jump_scalar;
    private bool is_on_ground;
    private Rigidbody2D rigidBody;

    // Start is called before the first frame update
    void Start() {
        rigidBody = GetComponent<Rigidbody2D>();
        currentState = "Idle";
        SetCharacterState(currentState);
    }

    // Update is called once per frame
    void Update() {
        Move();
    }

    public void SetAnimation(AnimationReferenceAsset animation, bool loop, float timeScale) {

        if(animation.name.Equals(currentAnimation)) {
            return;
        }
        skeletonAnimation.state.SetAnimation(0, animation, loop).TimeScale = timeScale;
        currentAnimation = animation.name;
    }

    public void SetCharacterState(string state) {
        if(state.Equals("Idle")) {
            SetAnimation(idle, true, 1f);
        } else if (state.Equals("Walk")) {
            SetAnimation(walk, true, 2f);
        } else if (state.Equals("Jump")) {
            SetAnimation(jump, true, 1f);
        }
    }

    public void Move() {
        Vector2 movement = Vector2.zero;

        if(rigidBody.velocity.magnitude < max_speed) { 
            movement.x = Input.GetAxis("Horizontal");
        }
        if(Input.GetButtonDown("Jump") && is_on_ground) {
            movement.y = jump_scalar;
        }
        rigidBody.AddForce(movement_scalar * movement);

        if (movement.x != 0) {
            if (movement.x > 0) transform.localScale = new Vector2(1f, 1f);
            if (movement.x < 0) transform.localScale = new Vector2(-1f, 1f);
            SetCharacterState("Walk");
        } else if (movement.y != 0) {
            SetCharacterState("Jump");
        } else {
            SetCharacterState("Idle");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision);
        if(CollisionIsWithGround(collision))
        {
            is_on_ground = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(!CollisionIsWithGround(collision))
        {
            is_on_ground = false;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigidBody;

    public Transform front;
    public Transform back;
    public Transform forward;
    public Transform backward;
    public float checkRadius;
    public LayerMask whatIsGround;
    public bool direction;
    public float speed;


    void Start()
    {
        // get rigidbody component and freeze rotation on it
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.flipX = direction;
    }

    void Update()
    {

        bool keepGoing = false;
        Vector2 movement = Vector2.zero;

        if (direction)
        {
            keepGoing = Physics2D.OverlapCircle(front.position, checkRadius, whatIsGround) &&
                        !Physics2D.OverlapCircle(forward.position, checkRadius, whatIsGround);
        }
        else
        {
            keepGoing = Physics2D.OverlapCircle(back.position, checkRadius, whatIsGround) &&
                        !Physics2D.OverlapCircle(backward.position, checkRadius, whatIsGround);
        }

        if (keepGoing)
        {
            movement.x = direction ? speed : -speed;
        }
        else
        {
            spriteRenderer.flipX = !spriteRenderer.flipX;
            direction = !direction;
            movement.x = direction ? speed : -speed;
        }

        rigidBody.velocity = movement;

    }
}

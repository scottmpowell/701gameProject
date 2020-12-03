using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patrol : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    //private Animator animator;
    private Rigidbody2D rigidBody;

    public Transform front;
    public Transform back;
    public float checkRadius;
    public LayerMask whatIsGround;

    public float speed;

    private bool direction;

    void Start()
    {
        // get rigidbody component and freeze rotation on it
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;

        spriteRenderer = GetComponent<SpriteRenderer>();
        //animator = GetComponent<Animator>();

        spriteRenderer.flipX = true;
        direction = true;
    }

    void Update()
    {

        bool keepGoing = false;
        Vector2 movement = Vector2.zero;

        if (direction)
        {
            keepGoing = Physics2D.OverlapCircle(front.position, checkRadius, whatIsGround);
        }
        else
        {
            keepGoing = Physics2D.OverlapCircle(back.position, checkRadius, whatIsGround);
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

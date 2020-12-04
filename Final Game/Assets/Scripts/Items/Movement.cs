using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float speed;
    public float magnitude;

    private bool direction;
    private Vector2 OGPos;

    // Start is called before the first frame update
    void Start()
    {
        OGPos = transform.position;
        direction = true;

    }

    // Update is called once per frame
    void Update()
    {
        Vector2 movement = transform.position;
        if(direction)
        {
            movement.y += speed;
        }
        else
        {
            movement.y -= speed;
        }

        if(movement.y > OGPos.y + magnitude || movement.y < OGPos.y - magnitude)
        {
            direction = !direction;
        }

        transform.position = movement;
    }
}

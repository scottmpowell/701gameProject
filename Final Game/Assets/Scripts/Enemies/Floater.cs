using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floater : MonoBehaviour
{
    public bool vertical;
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
    void FixedUpdate()
    {
        if(vertical)
        {
            Vert();
        }
        else
        {
            Hor();
        }
    }

    void Vert()
    {
        Vector2 position = transform.position;
        if (direction)
        {
            position.y += speed;
        }
        else
        {
            position.y -= speed;
        }

        if (position.y > OGPos.y + magnitude || position.y < OGPos.y - magnitude)
        {
            direction = !direction;
        }

        transform.position = position;
    }

    void Hor()
    {
        Vector2 position = transform.position;
        if (direction)
        {
            position.x += speed;
        }
        else
        {
            position.x -= speed;
        }

        if (position.x > OGPos.x + magnitude || position.x < OGPos.x - magnitude)
        {
            direction = !direction;
        }

        transform.position = position;
    }

}

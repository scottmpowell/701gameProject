using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrophyMovement : MonoBehaviour
{
    public float speed;
    public float magnitude;
    public LayerMask whatIsPlayer;
    public Transform topLeft;
    public Transform bottomRight;

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
        Move();
        CheckWin();
    }


    void Move()
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

    void CheckWin()
    {
        bool colliding = Physics2D.OverlapArea(topLeft.position, bottomRight.position, whatIsPlayer);
        if(colliding)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }
    }

}

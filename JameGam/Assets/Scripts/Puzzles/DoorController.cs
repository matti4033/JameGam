using UnityEngine;

public class DoorController : MonoBehaviour
{
    public float moveDistance = 3f;
    public float moveSpeed = 2f;

    public GameObject player;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool opening = false;
    private bool closing = false;

    void Start()
    {
        startPos = transform.position;
        targetPos = startPos + Vector3.up * moveDistance;
    }

    public void OpenDoor()
    {
        opening = true;
        closing = false;
        Debug.Log("Door opening!");
    }

    public void CloseDoor()
    {
        closing = true;
        opening = false;
        Debug.Log("Door closing!");
    }

    void Update()
    {
        if (opening)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPos) < 0.01f)
            {
                transform.position = targetPos;
                opening = false;
            }
        }

        if (closing)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                startPos,
                moveSpeed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, startPos) < 0.01f)
            {
                transform.position = startPos;
                closing = false;
            }
        }

        if (!closing && !opening && player.transform.position.x > transform.position.x)
        {
            CloseDoor();
        }
    }
}

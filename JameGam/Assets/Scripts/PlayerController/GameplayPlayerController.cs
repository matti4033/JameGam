using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius;
    [SerializeField] private LayerMask groundLayer;

    private bool isGrounded;
    private float moveX, moveY;

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    private void FixedUpdate()
    {
        Vector2 movement = new Vector2(moveX, moveY);
        rb.linearVelocity = movement * moveSpeed;
    }

    public void OnGameMove(InputValue value)
    {
        Debug.Log("MOVE");
        Vector2 movement = value.Get<Vector2>();

        moveX = movement.x;
        moveY = movement.y;
    }

    public void OnJump(InputValue value)
    {
        Debug.Log("JUMP");

        if (isGrounded & value.isPressed)
        {
            rb.AddForceAtPosition(new Vector2(0, jumpVelocity), Vector2.up, ForceMode2D.Impulse);
            //rb.AddForce(Vector2.up * jumpVelocity * 50, ForceMode2D.Impulse); 
        }
    }
}

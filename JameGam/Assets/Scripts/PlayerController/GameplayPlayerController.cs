using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GameplayPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpVelocity;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private float checkRadius;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask stringLayer;

    [SerializeField] private float dropDownDuration = 0.3f;
    private Collider2D playerCollider;

    private bool isGrounded;
    private float moveX, moveY;

    private void Awake()
    {
        playerCollider = GetComponent<Collider2D>();

    }

    private void Update()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer | stringLayer);

        if (Input.GetKeyDown(KeyCode.E))
        {
            rb.AddForce(Vector2.up * jumpVelocity, ForceMode2D.Impulse);
        }
    }

    private void FixedUpdate()
    {
        //Vector2 movement = new Vector2(moveX, moveY);
        //rb.linearVelocity = movement * moveSpeed;

        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);
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
        if (value.isPressed)
        {
            if (isGrounded)
            {
                if (moveY < -0.5f)
                {
                    DropThroughPlatform();
                }
                else
                {
                    rb.AddForce(Vector2.up * jumpVelocity, ForceMode2D.Impulse);
                }
            }
        }
    }

    private void DropThroughPlatform()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1f, stringLayer);
        if (hit.collider != null)
        {
            StartCoroutine(DisableCollision(hit.collider));
        }
    }

    private IEnumerator DisableCollision(Collider2D platform)
    {
        Physics2D.IgnoreCollision(playerCollider, platform, true);
        yield return new WaitForSeconds(dropDownDuration);
        if (platform != null)
            Physics2D.IgnoreCollision(playerCollider, platform, false);
    }
}

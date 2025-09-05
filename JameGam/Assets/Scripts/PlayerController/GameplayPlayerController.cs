using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class GameplayPlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private Rigidbody2D rb;

    [SerializeField] private float jumpVelocity;
    [SerializeField] private float jumpBoostMultiplier;
    [SerializeField] private float fallMultiplier;

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

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
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
                    float boostedVelocity = jumpVelocity * jumpBoostMultiplier;
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, boostedVelocity);
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

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

    [SerializeField] private GameObject throwPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private float throwForce = 10f;

    [SerializeField] private float dropDownDuration = 0.3f;
    private Collider2D playerCollider;

    public GameObject bossManager;

    private bool isGrounded;
    private float moveX, moveY;
    private GameObject currentBoss;

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
        currentBoss = bossManager.GetComponent<BossManager>().activeBoss.gameObject;
        if (currentBoss == null) return; 
        
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

    public void OnAction(InputValue value)
    {
        if (value.isPressed)
        {
            BaseBoss boss = FindObjectOfType<BaseBoss>();
            if(boss != null && boss.IsTired)
                ThrowThing();
        }
    }

    private void ThrowThing()
    {
        GameObject obj = Instantiate(throwPrefab, throwPoint.position, Quaternion.identity);
        Rigidbody2D rb2d = obj.GetComponent<Rigidbody2D>();
        if (rb2d != null && currentBoss != null)
        {
            Vector2 start = throwPoint.position;
            Vector2 target = currentBoss.transform.position;

            float distance = Vector2.Distance(start, target);

            float timeToTarget = Mathf.Clamp(distance / 10f, 0.5f, 2f);

            Vector2 velocity = CalculateArcVelocity(start, target, timeToTarget);
            rb2d.linearVelocity = velocity;
        }
    }

    private Vector2 CalculateArcVelocity(Vector2 start, Vector2 target, float timeToTarget)
    {
        Vector2 distance = target - start;

        float vx = distance.x / timeToTarget;

        float vy = (distance.y - 0.5f * Physics2D.gravity.y * timeToTarget * timeToTarget) / timeToTarget;

        return new Vector2(vx, vy);
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

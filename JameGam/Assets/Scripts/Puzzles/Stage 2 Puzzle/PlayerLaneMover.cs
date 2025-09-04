using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerLaneMover : MonoBehaviour
{
    [Header("Lane Layout (match StringWaveDirector)")]
    public int lanes = 6;

    public float topY = 3.5f;
    public float bottomY = -3.5f;

    [Header("Input (either/or)")] [Tooltip("Optional Input System action for Lane Up (e.g., W/UpArrow).")]
    public InputActionReference laneUpAction;

    [Tooltip("Optional Input System action for Lane Down (e.g., S/DownArrow).")]
    public InputActionReference laneDownAction;

    [Tooltip("Keyboard fallback if you don't use Input Actions.")]
    public KeyCode upKeyA = KeyCode.W, upKeyB = KeyCode.UpArrow;

    public KeyCode downKeyA = KeyCode.S, downKeyB = KeyCode.DownArrow;

    [Header("Horizontal X (fallback)")] public bool driveXHere = false;
    public float horizontalSpeedX = 6f;
    public KeyCode leftKeyA = KeyCode.A, leftKeyB = KeyCode.LeftArrow;
    public KeyCode rightKeyA = KeyCode.D, rightKeyB = KeyCode.RightArrow;

    [Header("Snap Behavior")] [Tooltip("If ON, teleport to lane center on press. If OFF, smoothly converge.")]
    public bool instantSnap = false;

    [Tooltip("Higher = snappier convergence when instantSnap is OFF.")]
    public float snapSpeed = 18f;

    [Tooltip("Zero vertical velocity when a hop happens (prevents bounce).")]
    public bool zeroYVelocityOnHop = true;

    [Header("Change Constraints (optional)")]
    [Tooltip("Require being grounded to change lanes (assign groundCheck & mask).")]
    public bool requireGroundedToChange = false;

    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundMask;

    Rigidbody2D rb;
    int targetLane;
    bool laneJustChanged;

    float LaneY(int laneIndex)
    {
        if (lanes <= 1) return (topY + bottomY) * 0.5f;
        float t = (laneIndex + 0.5f) / lanes; // centers
        return Mathf.Lerp(topY, bottomY, t);
    }

    int NearestLaneIndex(float y)
    {
        if (lanes <= 1) return 0;
        float t = Mathf.InverseLerp(topY, bottomY, y);
        int idx = Mathf.RoundToInt(t * lanes - 0.5f);
        return Mathf.Clamp(idx, 0, lanes - 1);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        if (laneUpAction) laneUpAction.action.performed += OnLaneUp;
        if (laneDownAction) laneDownAction.action.performed += OnLaneDown;
        if (laneUpAction) laneUpAction.action.Enable();
        if (laneDownAction) laneDownAction.action.Enable();
    }

    void OnDisable()
    {
        if (laneUpAction) laneUpAction.action.performed -= OnLaneUp;
        if (laneDownAction) laneDownAction.action.performed -= OnLaneDown;
    }

    void Start()
    {
        // Start on closest lane and hard snap Y
        targetLane = NearestLaneIndex(rb.position.y);
        Vector2 startPos = new Vector2(rb.position.x, LaneY(targetLane));
        rb.position = startPos; // safe to set once at start
    }

    void Update()
    {
        // Keyboard fallback
        if (Input.GetKeyDown(upKeyA) || Input.GetKeyDown(upKeyB)) TryChangeLane(-1);
        if (Input.GetKeyDown(downKeyA) || Input.GetKeyDown(downKeyB)) TryChangeLane(+1);
    }

    void FixedUpdate()
    {
        float yTarget = LaneY(targetLane);

        if (instantSnap && laneJustChanged)
        {
            // Teleport Y to lane center in physics step
            rb.MovePosition(new Vector2(rb.position.x, yTarget));
            laneJustChanged = false;
            return;
        }

        // Smooth toward the lane center.
        float y = Mathf.Lerp(rb.position.y, yTarget, 1f - Mathf.Exp(-snapSpeed * Time.fixedDeltaTime));
        rb.MovePosition(new Vector2(rb.position.x, y));
        laneJustChanged = false;

        if (driveXHere)
        {
            float h = 0f;
            if (Input.GetKey(leftKeyA) || Input.GetKey(leftKeyB)) h -= 1f;
            if (Input.GetKey(rightKeyA) || Input.GetKey(rightKeyB)) h += 1f;
            rb.linearVelocity = new Vector2(h * horizontalSpeedX, rb.linearVelocity.y);
        }
    }

    void OnLaneUp(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) TryChangeLane(-1);
    }

    void OnLaneDown(InputAction.CallbackContext ctx)
    {
        if (ctx.performed) TryChangeLane(+1);
    }

    void TryChangeLane(int delta)
    {
        if (requireGroundedToChange && !IsGrounded()) return;

        int newLane = Mathf.Clamp(targetLane + delta, 0, Mathf.Max(0, lanes - 1));
        if (newLane == targetLane) return;

        targetLane = newLane;
        laneJustChanged = true;

        if (zeroYVelocityOnHop)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f); // Keep X from GameplayPlayerController
    }

    bool IsGrounded()
    {
        if (!groundCheck) return true; // treat as grounded if not wired
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
    }
}
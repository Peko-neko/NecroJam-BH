using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private float normalGravity;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float runSpeed = 12f;

    [Header("Acceleration")]
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float runAcceleration = 35f;
    [SerializeField] private float deceleration = 60f;
    [SerializeField] private float runDeceleration = 25f;

    [Header("Run")]
    [SerializeField] private float runDelay = 2f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 14f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Glide")]
    [SerializeField] private float glideFallSpeed = 2f;
    [SerializeField] private float glideGravity = 2f;
    [SerializeField] private float glideBoost = 5f;

    private bool glideBoostUsed;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference glideAction;

    [Header("Visuals")]
    [SerializeField] private Transform graphics;

    [Header("Particles")]
    [SerializeField] private ParticleSystem runDust;

    private Rigidbody2D rb;

    private Vector2 moveInput;

    private bool glideHeld;
    private bool isGrounded;
    private bool isGliding;
    private bool isRunning;

    private float moveHeldTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        normalGravity = rb.gravityScale;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();
        glideAction.action.Enable();

        jumpAction.action.performed += OnJump;
    }

    private void OnDisable()
    {
        jumpAction.action.performed -= OnJump;

        moveAction.action.Disable();
        jumpAction.action.Disable();
        glideAction.action.Disable();
    }

    private void Update()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();
        glideHeld = glideAction.action.IsPressed();

        CheckGrounded();
        UpdateRun();
        UpdateGlide();
        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        Move();
        HandleGlide();
        UpdateRunDust();

    }

    // =========================
    // MOVEMENT
    // =========================

    private void Move()
    {
        bool moving = Mathf.Abs(moveInput.x) > 0.01f;

        float targetSpeed;

        if (isRunning)
            targetSpeed = moveInput.x * runSpeed;
        else
            targetSpeed = moveInput.x * walkSpeed;

        float accelerationRate;

        if (moving)
        {
            accelerationRate =
                isRunning
                    ? runAcceleration
                    : acceleration;
        }
        else
        {
            accelerationRate =
                isRunning
                    ? runDeceleration
                    : deceleration;
        }

        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            accelerationRate * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            newSpeed,
            rb.linearVelocity.y
        );
    }

    private void UpdateRun()
    {
        if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            moveHeldTime += Time.deltaTime;

            if (moveHeldTime >= runDelay)
                isRunning = true;
        }
        else
        {
            moveHeldTime = 0f;
            isRunning = false;
        }
    }

    // =========================
    // JUMP
    // =========================

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!isGrounded)
            return;

        isGliding = false;
        glideBoostUsed = false;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }

    // =========================
    // GLIDE
    // =========================

    private void UpdateGlide()
    {
        if (isGrounded || !glideHeld)
        {
            isGliding = false;
            glideBoostUsed = false;
            return;
        }

        if (rb.linearVelocity.y <= 0f)
        {
            isGliding = true;
        }
    }

    private void HandleGlide()
    {
        if (!isGliding)
        {
            rb.gravityScale = normalGravity;
            return;
        }

        rb.gravityScale = glideGravity;

        // Boost only once when glide begins
        if (!glideBoostUsed)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                glideBoost
            );

            glideBoostUsed = true;
        }

        // Limit falling speed
        if (rb.linearVelocity.y < -glideFallSpeed)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                -glideFallSpeed
            );
        }
    }

    // =========================
    // GROUND
    // =========================

    private void CheckGrounded()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    // =========================
    // VISUALS
    // =========================

    private void UpdateVisuals()
    {
        if (graphics == null)
            return;

        if (moveInput.x > 0.01f)
        {
            graphics.localScale =
                new Vector3(1f, 1f, 1f);
        }
        else if (moveInput.x < -0.01f)
        {
            graphics.localScale =
                new Vector3(-1f, 1f, 1f);
        }
    }

    // =========================
    // PUBLIC STATE
    // =========================

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool IsGliding()
    {
        return isGliding;
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public bool IsMoving()
    {
        return Mathf.Abs(moveInput.x) > 0.01f;
    }

    public float GetHorizontalSpeed()
    {
        return Mathf.Abs(rb.linearVelocity.x);
    }

    // =========================
    // DEBUG
    // =========================

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }

    private void UpdateRunDust()
    {
        if (runDust == null)
            return;

        if (isRunning && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            if (!runDust.isPlaying)
                runDust.Play();
        }
        else
        {
            if (runDust.isPlaying)
                runDust.Stop();
        }
    }
}
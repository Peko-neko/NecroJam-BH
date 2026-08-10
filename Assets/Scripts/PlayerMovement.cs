using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{

    private float normalGravity;
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float deceleration = 60f;

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

    private Rigidbody2D rb;

    private Vector2 moveInput;
    private bool glideHeld;

    private bool isGrounded;
    private bool isGliding;

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
        UpdateGlide();
        UpdateVisuals();

    }

    private void FixedUpdate()
    {
        Move();
        HandleGlide();
    }

    private void Move()
    {
        float targetSpeed = moveInput.x * moveSpeed;

        float accelerationRate;

        if (Mathf.Abs(targetSpeed) > 0.01f)
            accelerationRate = acceleration;
        else
            accelerationRate = deceleration;

        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            accelerationRate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(
            newSpeed,
            rb.linearVelocity.y);
    }

    private void OnJump(InputAction.CallbackContext context)
    {
        if (!isGrounded)
            return;

        isGliding = false;

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce);
    }

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
                glideBoost);

            glideBoostUsed = true;
        }

        // Limit falling speed
        if (rb.linearVelocity.y < -glideFallSpeed)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                -glideFallSpeed);
        }
    }

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
            groundLayer);
    }

    private void UpdateVisuals()
    {
        if (graphics == null)
            return;

        if (moveInput.x > 0.01f)
            graphics.localScale = new Vector3(1f, 1f, 1f);
        else if (moveInput.x < -0.01f)
            graphics.localScale = new Vector3(-1f, 1f, 1f);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius);
    }
}
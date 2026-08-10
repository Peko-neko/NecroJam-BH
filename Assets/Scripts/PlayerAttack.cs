using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference attackAction;

    [Header("Attack")]
    [SerializeField] private Collider2D attackHitbox;
    [SerializeField] private float attackDuration = 0.2f;

    [Header("Hook")]
    [SerializeField] private Collider2D hookHitbox;
    [SerializeField] private float hookHoldTime = 0.2f;

    private Rigidbody2D rb;

    private bool isAttacking;
    private bool isHooking;

    private float attackTimer;
    private float holdTimer;

    private Hookable currentHook;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (attackHitbox != null)
            attackHitbox.enabled = false;

        if (hookHitbox != null)
            hookHitbox.enabled = false;
    }

    private void OnEnable()
    {
        attackAction.action.Enable();
    }

    private void OnDisable()
    {
        attackAction.action.Disable();
    }

    private void Update()
    {
        HandleInput();
        UpdateAttack();
    }

    private void HandleInput()
    {
        if (attackAction.action.IsPressed())
        {
            holdTimer += Time.deltaTime;

            // Start hook detection after holding
            if (holdTimer >= hookHoldTime && !isAttacking && !isHooking)
            {
                StartHookDetection();
            }
        }

        if (attackAction.action.WasPressedThisFrame())
        {
            StartAttack();
        }

        if (attackAction.action.WasReleasedThisFrame())
        {
            holdTimer = 0f;

            if (isHooking)
                ReleaseHook();

            StopHookDetection();
        }
    }

    private void StartAttack()
    {
        if (isHooking)
            return;

        if (isAttacking)
            return;

        isAttacking = true;
        attackTimer = attackDuration;

        if (attackHitbox != null)
            attackHitbox.enabled = true;
    }

    private void UpdateAttack()
    {
        if (!isAttacking)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer <= 0f)
        {
            isAttacking = false;

            if (attackHitbox != null)
                attackHitbox.enabled = false;
        }
    }

    private void StartHookDetection()
    {
        if (hookHitbox == null)
            return;

        hookHitbox.enabled = true;
    }

    private void StopHookDetection()
    {
        if (hookHitbox == null)
            return;

        hookHitbox.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isHooking && hookHitbox != null && hookHitbox.enabled)
        {
            Hookable hookable = other.GetComponentInParent<Hookable>();

            if (hookable != null)
            {
                AttachToHook(hookable);
            }
        }
    }

    private void AttachToHook(Hookable hookable)
    {
        currentHook = hookable;
        isHooking = true;

        Debug.Log("Attached to: " + hookable.name);
    }

    private void ReleaseHook()
    {
        isHooking = false;
        currentHook = null;

        Debug.Log("Released hook");
    }

    public bool IsHooking()
    {
        return isHooking;
    }

    public Hookable GetCurrentHook()
    {
        return currentHook;
    }
}
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHookMovement : MonoBehaviour
{
    [Header("Hook Movement")]
    [SerializeField] private float pullStrength = 35f;
    [SerializeField] private float maxPullSpeed = 18f;

    [Header("Swing")]
    [SerializeField] private float swingStrength = 20f;

    private Rigidbody2D rb;
    private PlayerAttack playerAttack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAttack = GetComponent<PlayerAttack>();
    }

    private void FixedUpdate()
    {
        if (playerAttack == null)
            return;

        if (!playerAttack.IsHooking())
            return;

        Hookable hook = playerAttack.GetCurrentHook();

        if (hook == null)
            return;

        Transform hookPoint = hook.GetHookPoint();

        if (hookPoint == null)
            return;

        PullTowardsHook(hookPoint);
    }

    private void PullTowardsHook(Transform hookPoint)
    {
        Vector2 direction =
            ((Vector2)hookPoint.position - rb.position).normalized;

        float distance =
            Vector2.Distance(rb.position, hookPoint.position);

        if (distance < 0.5f)
            return;

        float force = distance * pullStrength;

        rb.AddForce(
            direction * force,
            ForceMode2D.Force
        );

        LimitPullSpeed(direction);
    }

    private void LimitPullSpeed(Vector2 pullDirection)
    {
        float velocityTowardsHook =
            Vector2.Dot(rb.linearVelocity, pullDirection);

        if (velocityTowardsHook <= maxPullSpeed)
            return;

        Vector2 excess =
            pullDirection *
            (velocityTowardsHook - maxPullSpeed);

        rb.linearVelocity -= excess;
    }
}
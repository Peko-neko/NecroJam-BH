using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float smoothTime = 0.08f;

    [Header("Input")]
    [SerializeField] private InputActionReference moveAction;

    [Header("Visuals")]
    [SerializeField] private Transform graphics;
    [SerializeField] private float maxLeanAngle = 10f;
    [SerializeField] private float leanSpeed = 10f;

    private Rigidbody2D rb;

    private Vector2 moveInput;
    private Vector2 currentVelocity;
    private Vector2 velocitySmoothRef;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
    }

    private void Update()
    {
        moveInput = moveAction.action.ReadValue<Vector2>();

        UpdateLean();
    }

    private void FixedUpdate()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;

        currentVelocity = Vector2.SmoothDamp(
            currentVelocity,
            targetVelocity,
            ref velocitySmoothRef,
            smoothTime);

        rb.linearVelocity = currentVelocity;
    }

    private void UpdateLean()
    {
        if (graphics == null)
            return;

        float targetAngle = -currentVelocity.x / moveSpeed * maxLeanAngle;

        graphics.localRotation = Quaternion.Lerp(
            graphics.localRotation,
            Quaternion.Euler(0, 0, targetAngle),
            leanSpeed * Time.deltaTime);
    }
}
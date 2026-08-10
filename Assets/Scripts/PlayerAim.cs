using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    [Header("Aim")]
    [SerializeField] private float aimDistance = 2f;

    [Header("Hitboxes")]
    [SerializeField] private Transform attackHitbox;
    [SerializeField] private Transform hookHitbox;

    [Header("Aim Visual")]
    [SerializeField] private Transform aimCursor;

    public Vector2 AimDirection { get; private set; }

    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        UpdateAim();
        UpdateHitboxes();
        UpdateCursor();
    }

    private void UpdateAim()
    {
        Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 direction =
            mouse - transform.position;

        if (direction.sqrMagnitude < 0.001f)
            return;

        AimDirection = direction.normalized;
    }

    private void UpdateHitboxes()
    {
        float angle =
            Mathf.Atan2(
                AimDirection.y,
                AimDirection.x
            ) * Mathf.Rad2Deg;

        Quaternion rotation =
            Quaternion.Euler(0f, 0f, angle);

        if (attackHitbox != null)
            attackHitbox.rotation = rotation;

        if (hookHitbox != null)
            hookHitbox.rotation = rotation;
    }

    private void UpdateCursor()
    {
        if (aimCursor == null)
            return;

        aimCursor.position =
            (Vector2)transform.position +
            AimDirection * aimDistance;
    }
}
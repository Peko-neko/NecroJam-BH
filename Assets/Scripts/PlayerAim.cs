using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    Camera cam;

    public Vector2 AimDirection { get; private set; }

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        Vector3 mouse = cam.ScreenToWorldPoint(Input.mousePosition);
        mouse.z = transform.position.z;

        AimDirection = (mouse - transform.position).normalized;
    }
}
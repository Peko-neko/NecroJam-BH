using UnityEngine;
using UnityEngine.InputSystem;

namespace AlignedGames

{

    public class PlayerCameraManager : MonoBehaviour

    {

        [Header("Camera Settings")]
        public Transform target;          // The player to follow
        public float followSpeed = 5f;    // Speed of camera following the target
        public Vector2 followOffset;      // Offset from player position

        [Header("Look Ahead Settings")]
        public float maxLookAheadDistance = 3f;  // Maximum distance the camera can move toward the mouse
        public float mouseRange = 2f;            // Range where camera focuses primarily on the player
        public float smoothingFactor = 5f;       // Smoothing factor for the mouse influence

        private Camera cam;
        private float targetZoom; // The target zoom level

        private void Awake()

        {

            cam = GetComponent<Camera>();
            targetZoom = cam.orthographicSize;

        }

        private void LateUpdate()

        {

            if (target == null)

            {

                target = GameObject.FindWithTag("Player").transform;

            }

            // Get mouse position in world space
            Vector3 mouseWorldPos = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            mouseWorldPos.z = 0f;

            // Calculate direction from player to mouse
            Vector3 playerToMouse = mouseWorldPos - target.position;
            Vector3 targetPosition = (Vector2)target.position + followOffset;

            // Calculate influence of the mouse based on distance
            float distance = playerToMouse.magnitude;
            float mouseInfluence = Mathf.SmoothStep(0f, 1f, (distance - mouseRange) / maxLookAheadDistance);

            // Clamp influence between 0 and 1
            mouseInfluence = Mathf.Clamp01(mouseInfluence);

            // Apply smoothed influence to camera position
            Vector3 mouseOffset = Vector3.ClampMagnitude(playerToMouse, maxLookAheadDistance);
            targetPosition += mouseOffset * mouseInfluence;

            // Smooth follow camera position
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            transform.position = new Vector3(smoothedPosition.x, smoothedPosition.y, -10f); // Keep Z axis at -10 for 2D

        }

    }

}
using UnityEngine;

namespace AlignedGames
{

    public class ScreenShakeManager : MonoBehaviour
    {
        [Header("Shake Parameters")]
        // Maximum allowed shake magnitude
        public float maxShakeMagnitude = 0.5f;
        // Speed at which the shake effect damps out
        public float dampingSpeed = 1.0f;

        // Remaining duration of the current shake
        private float currentShakeDuration;
        // Current magnitude of the shake effect
        private float shakeMagnitude;
        // Original local position before shake started
        private Vector3 originalPosition;
        // Flag to track if shake is active
        private bool isShaking;

        void Start()
        {
            // Store the original position at start
            originalPosition = transform.localPosition;
        }

        void Update()
        {
            if (currentShakeDuration > 0)
            {
                if (!isShaking)
                {
                    // Capture position at the start of shaking
                    originalPosition = transform.localPosition;
                    isShaking = true;
                }

                // Apply random shake offset based on magnitude
                transform.localPosition = originalPosition + (Random.insideUnitSphere * shakeMagnitude);
                // Reduce shake duration over time, factoring in damping
                currentShakeDuration -= Time.deltaTime * dampingSpeed;
            }
            else if (isShaking)
            {
                // Reset position once shaking ends
                transform.localPosition = originalPosition;
                isShaking = false;
            }
        }

        // Method to start or update shake effect
        public void TriggerShake(float duration, float magnitude)
        {
            // Clamp magnitude to max allowed
            magnitude = Mathf.Clamp(magnitude, 0f, maxShakeMagnitude);

            // Override current shake only if new one is stronger or lasts longer
            if (magnitude > shakeMagnitude || duration > currentShakeDuration)
            {
                shakeMagnitude = magnitude;
                currentShakeDuration = duration;
            }
        }

        // Additional method to trigger shake using trauma parameters
        public void OnReceiveTrauma(float traumaDuration, float traumaMagnitude)
        {
            TriggerShake(traumaDuration, traumaMagnitude);
        }
    }

}

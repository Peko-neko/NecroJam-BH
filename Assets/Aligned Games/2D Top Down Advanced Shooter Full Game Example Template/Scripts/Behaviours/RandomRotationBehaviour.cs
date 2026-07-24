using UnityEngine;

namespace AlignedGames
{
    public class RandomRotationBehaviour : MonoBehaviour
    {
        // Rotation speed in degrees per second
        public float rotationSpeed = 90f;
        private float currentRotationSpeed;

        private void Start()
        {
            // Randomly decide rotation direction (clockwise or counterclockwise)
            float direction = Random.value > 0.5f ? 1f : -1f;
            // Randomize speed magnitude between 50% and 100% of rotationSpeed
            float magnitude = Random.Range(0.5f, 1f);
            currentRotationSpeed = rotationSpeed * magnitude * direction;
        }

        private void Update()
        {
            // Rotate the object around Z axis based on currentRotationSpeed
            transform.Rotate(0f, 0f, currentRotationSpeed * Time.deltaTime);
        }
    }
}

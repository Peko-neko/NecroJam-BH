using UnityEngine;

namespace AlignedGames
{
    public class AvoidEnemiesBehaviour : MonoBehaviour
    {
        // How far the object checks for nearby enemies
        public float avoidanceRadius = 1.2f;

        // How strongly the object gets pushed away from others
        public float pushStrength = 0.05f;

        // Tag used to identify enemies to avoid
        public string enemyTag = "Enemy";

        // This runs every frame after all Update functions have finished
        void LateUpdate()
        {
            // Get all colliders in a circle around this object
            Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius);

            Vector2 totalPush = Vector2.zero; // Accumulated push direction
            int pushCount = 0; // Number of nearby enemies

            // Go through each nearby collider
            foreach (var other in nearby)
            {
                // Skip self
                if (other.gameObject == gameObject) continue;

                // Skip if the other object isn't tagged as an enemy
                if (!other.CompareTag(enemyTag)) continue;

                // Calculate direction from the other object to this one
                Vector2 diff = (Vector2)(transform.position - other.transform.position);
                float dist = diff.magnitude;

                if (dist > 0)
                {
                    // Normalize and weight by inverse of distance (closer = stronger push)
                    Vector2 pushDir = diff.normalized / dist;
                    totalPush += pushDir;
                    pushCount++;
                }
            }

            // If any enemies were found, apply average push
            if (pushCount > 0)
            {
                Vector2 averagePush = totalPush / pushCount;
                transform.position += (Vector3)(averagePush * pushStrength);
            }
        }

        // Draw a circle in the Scene view to visualize the avoidance radius
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
        }
    }
}

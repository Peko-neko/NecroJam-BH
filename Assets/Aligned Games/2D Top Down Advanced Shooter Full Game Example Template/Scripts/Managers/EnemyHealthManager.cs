using UnityEngine;

namespace AlignedGames
{

    public class EnemyHealthManager : MonoBehaviour
    {
        [Header("Health Settings")]
        public int maxHealth = 50;          // Maximum health value for the enemy
        private int currentHealth;          // Current health value, private because it is managed internally

        [Header("Blood Decal Settings")]
        public GameObject bloodDecalPrefab; // Prefab for the blood decal that appears when the enemy takes damage
        public Vector2 bloodDecalScaleRange = new Vector2(0.8f, 1.2f); // Random scale range for the blood decal to vary size
        public Vector2 bloodDecalRotationRange = new Vector2(0f, 360f); // Random rotation range for blood decal to look natural

        public int sortingOrder;             // Sorting order for decal rendering (to control layering in 2D)
        public float decalLifetime = 5f;    // How long the blood decal stays visible before it fades away
        public float fadeDuration = 1f;     // Duration of the decal's fade out effect

        private void Awake()
        {
            // Initialize current health to max health when the enemy is spawned or enabled
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage)
        {
            // Reduce current health by the damage amount
            currentHealth -= damage;

            // Spawn a blood decal to show damage visually
            SpawnBloodDecal();

            // If health drops to zero or below, trigger death logic
            if (currentHealth <= 0)
                Die();
        }

        private void SpawnBloodDecal()
        {
            // If no blood decal prefab is assigned, exit early to avoid errors
            if (bloodDecalPrefab == null) return;

            // Create a new blood decal object at the enemy's position, no rotation initially
            GameObject decalGO = Instantiate(bloodDecalPrefab, transform.position, Quaternion.identity);

            // Apply a random rotation around Z-axis to make decals look varied
            float rotation = Random.Range(bloodDecalRotationRange.x, bloodDecalRotationRange.y);
            decalGO.transform.rotation = Quaternion.Euler(0f, 0f, rotation);

            // Apply a random scale within the defined range to the decal
            float scale = Random.Range(bloodDecalScaleRange.x, bloodDecalScaleRange.y);

            // Try to get the SpriteFadeBehaviour component to initialize fade settings
            var bloodDecal = decalGO.GetComponent<SpriteFadeBehaviour>();
            if (bloodDecal != null)
            {
                // Initialize decal with sorting order, scale, lifetime, and fade duration
                bloodDecal.Initialize(sortingOrder, scale, decalLifetime, fadeDuration);
            }
        }

        private void Die()
        {
            // Log message when enemy dies (useful for debugging)
            Debug.Log("Enemy Died");

            // Destroy the enemy game object to remove it from the scene
            Destroy(gameObject);
        }
    }

}

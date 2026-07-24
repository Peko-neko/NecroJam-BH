using UnityEngine;

namespace AlignedGames
{
    public class ExplosionBehaviour : MonoBehaviour
    {
        [Header("Explosion Settings")]
        public float radius = 3f; // Explosion radius for AOE
        public int damage = 50;   // Damage dealt by the explosion
        public LayerMask affectedLayers; // Layers affected by the explosion (0 -> fallback to Everything)
        public float lifetime = 0.1f;    // Time before this object is destroyed

        [Header("Detection")]
        [Tooltip("Include trigger colliders when applying damage.")]
        public bool includeTriggerColliders = true;

        [Tooltip("If the masked pass finds 0 colliders, do an unmasked pass so layer setup never blocks damage.")]
        public bool fallbackToUnmaskedIfZero = true;

        [Header("Debug")]
        public bool logHits = false;        // Logs each hit and what took damage
        public bool probeWhenNoHits = true; // If no hits, list nearby colliders and their layers

        private void Start()
        {
            // Apply damage to all affected targets in radius
            ApplyExplosionDamage();

            // Schedule destruction of this component after lifetime
            Destroy(this, lifetime);
        }

        private void ApplyExplosionDamage()
        {
            // If user left mask empty (value == 0, which means "Nothing"), fallback to Everything (~0)
            int mask = (affectedLayers.value == 0) ? ~0 : affectedLayers.value;

            // Find all colliders within the radius on the specified layers
            Collider2D[] targets = Physics2D.OverlapCircleAll(transform.position, radius, mask);
            if (targets.Length == 0)
            {
                if (probeWhenNoHits)
                {
                    var probe = Physics2D.OverlapCircleAll(transform.position, radius, ~0);
                    Debug.Log($"[Explosion2D] Mask {mask} found 0. Unmasked sees {probe.Length}. Nearby colliders & layers:");
                    foreach (var c in probe)
                    {
                        int layer = c.gameObject.layer;
                        string lname = LayerMask.LayerToName(layer);
                        Debug.Log($"  - {c.name} on layer {layer} ('{lname}')", c);
                    }
                }

                // Safety: if mask blocked everything, optionally run once unmasked
                if (fallbackToUnmaskedIfZero)
                {
                    targets = Physics2D.OverlapCircleAll(transform.position, radius, ~0);
                    if (logHits) Debug.Log($"[Explosion2D] Fallback unmasked pass: {targets.Length} hits.", this);
                }
            }

            foreach (Collider2D col in targets)
            {
                if (!includeTriggerColliders && col.isTrigger) continue;

                // If the target is a player, apply damage
                if (col.CompareTag("Player"))
                {
                    PlayerHealthManager playerHealth =
                        col.GetComponentInParent<PlayerHealthManager>() ??
                        col.GetComponent<PlayerHealthManager>() ??
                        col.GetComponentInChildren<PlayerHealthManager>();

                    if (playerHealth != null)
                    {
                        if (logHits) Debug.Log($"[Explosion2D] Player hit: {col.name}", col);
                        playerHealth.TakeDamage(damage, transform.position);
                        continue; // avoid double-processing below
                    }
                }

                // If the target is a zombie enemy, apply damage and trigger aggression
                EnemyZombieAIManager zombieAI = col.GetComponentInParent<EnemyZombieAIManager>();
                if (zombieAI != null)
                {
                    EnemyHealthManager enemyHealth =
                        col.GetComponentInParent<EnemyHealthManager>() ??
                        col.GetComponent<EnemyHealthManager>() ??
                        col.GetComponentInChildren<EnemyHealthManager>();

                    if (enemyHealth != null)
                    {
                        if (logHits) Debug.Log($"[Explosion2D] Zombie enemy hit: {col.name}", col);
                        enemyHealth.TakeDamage(damage);
                    }

                    zombieAI.TriggerAggression();
                    continue; // avoid double-processing below
                }

                // If the target is a human enemy, apply damage and trigger aggression
                HumanEnemyAIManager humanAI = col.GetComponentInParent<HumanEnemyAIManager>();
                if (humanAI != null)
                {
                    EnemyHealthManager enemyHealth =
                        col.GetComponentInParent<EnemyHealthManager>() ??
                        col.GetComponent<EnemyHealthManager>() ??
                        col.GetComponentInChildren<EnemyHealthManager>();

                    if (enemyHealth != null)
                    {
                        if (logHits) Debug.Log($"[Explosion2D] Human enemy hit: {col.name}", col);
                        enemyHealth.TakeDamage(damage);
                    }

                    humanAI.TriggerAggression();
                    continue; // avoid double-processing below
                }

                // Try to damage the object if they have a health component
                // (Use local/parent/children so it works whether the collider is on a child or root)
                var objectHealth =
                    col.GetComponent<ObjectHealthManager>() ??
                    col.GetComponentInParent<ObjectHealthManager>() ??
                    col.GetComponentInChildren<ObjectHealthManager>();

                if (objectHealth != null)
                {
                    if (logHits) Debug.Log($"[Explosion2D] Object hit (ObjectHealthManager): {col.name}", col);
                    objectHealth.TakeDamage(damage);
                    continue;
                }

                if (logHits) Debug.Log($"[Explosion2D] No known health component on: {col.name}", col);
            }
        }

        // Draw explosion radius in editor when selected
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}

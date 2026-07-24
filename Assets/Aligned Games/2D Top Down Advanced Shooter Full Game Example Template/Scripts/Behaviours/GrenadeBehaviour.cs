using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections; // Added to allow using coroutines like WaitForSeconds

namespace AlignedGames
{
    public class GrenadeBehaviour : MonoBehaviour
    {
        // Time before the grenade explodes
        public float explodeDelay = 2f;

        // Radius within which enemies take damage
        public float explosionRadius = 3f;

        // Prevents grenade from exploding more than once
        private bool hasExploded = false;

        // Damage amount dealt by the grenade
        public int explodeDamage;

        // Optional visual effect to show when grenade explodes
        public GameObject explosionEffectPrefab;

        // Called when the grenade is first created
        private void Start()
        {
            // Start a coroutine that waits, then triggers explosion
            StartCoroutine(DelayedExplosion());
        }

        // Coroutine that waits before exploding
        private IEnumerator DelayedExplosion()
        {
            // Wait for the set delay time
            yield return new WaitForSeconds(explodeDelay);

            // Call the explode method
            Explode();
        }

        // Method to handle explosion logic
        void Explode()
        {
            // Exit if the grenade has already exploded
            if (hasExploded) return;

            // Mark as exploded to avoid multiple triggers
            hasExploded = true;

            // Create explosion effect (optional)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

            // Find all enemies within explosion radius
            Collider2D[] enemiesHit = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

            // Apply damage to each enemy in the list
            foreach (Collider2D enemy in enemiesHit)
            {
                DamageEnemy(enemy.gameObject);
            }

            // Destroy the grenade object after explosion
            Destroy(gameObject);
        }

        // This helps visualize explosion range in the editor
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }

        // Applies damage to any object with an EnemyHealthManager
        public void DamageEnemy(GameObject enemy)
        {
            EnemyHealthManager enemyScript = enemy.GetComponent<EnemyHealthManager>();

            // Only apply damage if the component exists
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(explodeDamage);
            }
        }
    }
}

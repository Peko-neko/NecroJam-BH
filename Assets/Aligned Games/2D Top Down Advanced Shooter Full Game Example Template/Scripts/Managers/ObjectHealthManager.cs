using UnityEngine;

namespace AlignedGames
{
    public class ObjectHealthManager : MonoBehaviour
    {
        public int maxHealth = 100;
        public GameObject explosionPrefab;
        public Vector3 explosionOffset = Vector3.zero;

        public int currentHealth;
        bool isDead;

        void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (isDead || amount <= 0) return;

            currentHealth -= amount;
            if (currentHealth <= 0)
            {
                isDead = true;
                if (explosionPrefab != null)
                {
                    Instantiate(explosionPrefab, transform.position + explosionOffset, Quaternion.identity);
                }
                Destroy(gameObject);
            }
        }
    }
}

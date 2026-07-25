using UnityEngine;

namespace AlignedGames
{
    public class CelestialBullet : MonoBehaviour
    {
        public int damage = 1;
        public float lifeTime = 8f;

        public GameObject owner;

        void Start()
        {
            Destroy(gameObject, lifeTime);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == owner)
                return;

            // Ignore every enemy
            if (other.GetComponent<CelestialEnemyAI>() != null)
                return;

            if (other.CompareTag("Player"))
            {
                PlayerHealthManager player =
                    other.GetComponent<PlayerHealthManager>();

                if (player != null)
                    player.TakeDamage(damage, transform.position);

                Destroy(gameObject);
                return;
            }

            if (other.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
        }
        }
    }
using UnityEngine;

namespace AlignedGames
{
    public class CelestialShooter : MonoBehaviour
    {
        [Header("References")]
        public GameObject bulletPrefab;
        public Transform firePoint;

        [Header("Pattern")]
        public BulletPattern bulletPattern = BulletPattern.Single;

        [Header("Stats")]
        public float fireRate = 2f;
        public float bulletSpeed = 8f;
        public int bulletCount = 8;
        public float spreadAngle = 60f;

        private float fireTimer;
        private float spiralAngle;
        private Transform player;

        void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");

            if (p != null)
                player = p.transform;
        }

        void Update()
        {
            if (player == null)
                return;

            fireTimer += Time.deltaTime;

            if (fireTimer >= fireRate)
            {
                fireTimer = 0f;
                Fire();
            }
        }

        public void Fire()
        {
            switch (bulletPattern)
            {
                case BulletPattern.Single:
                    FireSingle();
                    break;

                case BulletPattern.Burst:
                    FireBurst();
                    break;

                case BulletPattern.Circle:
                    FireCircle();
                    break;

                case BulletPattern.Spiral:
                    FireSpiral();
                    break;
            }
        }

        void SpawnBullet(Vector2 direction)
        {
            GameObject bullet = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.identity);

            // Give the bullet its owner
            CelestialBullet cb = bullet.GetComponent<CelestialBullet>();

            if (cb != null)
            {
                cb.owner = gameObject;
            }

            // Launch the bullet
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = direction.normalized * bulletSpeed;
            }
        }

        void FireSingle()
        {
            Vector2 dir =
                (player.position - firePoint.position).normalized;

            SpawnBullet(dir);
        }

        void FireBurst()
        {
            Vector2 dir =
                (player.position - firePoint.position).normalized;

            float startAngle = -spreadAngle / 2f;
            float step = spreadAngle / (bulletCount - 1);

            for (int i = 0; i < bulletCount; i++)
            {
                Vector2 d =
                    Quaternion.Euler(0, 0, startAngle + step * i) * dir;

                SpawnBullet(d);
            }
        }

        void FireCircle()
        {
            float step = 360f / bulletCount;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = step * i;

                Vector2 dir = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad));

                SpawnBullet(dir);
            }
        }

        void FireSpiral()
        {
            float step = 360f / bulletCount;

            spiralAngle += 20f;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = spiralAngle + i * step;

                Vector2 dir = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad));

                SpawnBullet(dir);
            }
        }
    }
}
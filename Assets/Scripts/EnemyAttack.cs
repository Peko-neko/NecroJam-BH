using UnityEngine;

    public enum FireMode
    {
        FixedDirection,
        AimAtPlayer,
        Circle
    }

    public class EnemyAttack : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Bullet bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private Transform fireDirection;

        [Header("Attack")]
        [SerializeField] private FireMode fireMode = FireMode.FixedDirection;

        [Header("Circle")]
        [SerializeField] private int circleBulletCount = 12;

        private Transform player;

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
                player = playerObj.transform;
        }

        public void Fire()
        {
            switch (fireMode)
            {
                case FireMode.FixedDirection:
                    FireSingle(fireDirection.right);
                    break;

                case FireMode.AimAtPlayer:
                    if (player != null)
                    {
                        Vector2 dir = (player.position - firePoint.position).normalized;
                        FireSingle(dir);
                    }
                    break;

                case FireMode.Circle:
                    FireCircle();
                    break;
            }
        }

        private void FireSingle(Vector2 direction)
        {
            Bullet bullet = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.identity);

            bullet.Fire(direction);
        }

        private void FireCircle()
        {
            float step = 360f / circleBulletCount;

            for (int i = 0; i < circleBulletCount; i++)
            {
                float angle = step * i;

                Vector2 dir = new Vector2(
                    Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad));

                FireSingle(dir);
            }
        }
    }
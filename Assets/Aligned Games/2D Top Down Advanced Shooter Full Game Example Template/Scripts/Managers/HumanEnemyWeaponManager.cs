using UnityEngine;

namespace AlignedGames
{
    public class HumanEnemyWeaponManager : MonoBehaviour
    {
        [Header("Weapon Settings")]
        public GameObject bulletPrefab;
        public float bulletSpeed = 10f;
        public int damage = 10;
        public float shootRate = 0.2f;

        private float lastShootTime;


        public void TryShoot(Transform firePoint, Vector3 targetPosition)
        {
            if (Time.time < lastShootTime + shootRate)
                return;

            lastShootTime = Time.time;

            Shoot(firePoint, targetPosition);
        }


        private void Shoot(Transform firePoint, Vector3 targetPosition)
        {
            Vector3 direction =
                (targetPosition - firePoint.position).normalized;


            GameObject bullet = Instantiate(
                bulletPrefab,
                firePoint.position,
                Quaternion.identity
            );


            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = direction * bulletSpeed;
            }


            float angle = Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;


            bullet.transform.rotation =
                Quaternion.Euler(0, 0, angle);


            EnemyProjectileBehaviour projectile =
                bullet.GetComponent<EnemyProjectileBehaviour>();

            if (projectile != null)
            {
                projectile.SetDamage(damage);
            }
        }
    }
}
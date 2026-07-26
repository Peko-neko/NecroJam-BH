using UnityEngine;

namespace AlignedGames
{
    public class EnemyAI : MonoBehaviour
    {
        [Header("AI Settings")]
        public float speed = 2f;
        public float shootRange = 7f;
        public float stopRange = 1.5f;
        public float rotationSpeed = 2f;

        [Header("Weapon Settings")]
        public Transform firePoint;

        [Header("Sight Settings")]
        public LayerMask obstacleMask;

        private Transform player;

        private bool lostSightLastFrame = false;
        private bool isStrafing = false;
        private int strafeDirection = 1;

        private HumanEnemyWeaponManager weaponManager;

        private void Start()
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            weaponManager = GetComponent<HumanEnemyWeaponManager>();
        }

        private void Update()
        {
            if (player == null)
                return;

            float distance = Vector2.Distance(transform.position, player.position);

            if (!CanSeePlayer())
            {
                if (!lostSightLastFrame)
                {
                    PickStrafeDirection();
                    lostSightLastFrame = true;
                    isStrafing = true;
                }

                StrafeToFindSight();

                if (distance <= shootRange)
                    ShootAtPlayer();
            }
            else
            {
                lostSightLastFrame = false;
                isStrafing = false;

                if (distance > shootRange)
                {
                    MoveTowardsPlayer();
                }
                else
                {
                    if (distance > stopRange)
                        MoveTowardsPlayer();
                    else
                        StrafeNearPlayer();

                    ShootAtPlayer();
                }
            }

            RotateFirePoint();
        }

        private void MoveTowardsPlayer()
        {
            if (Vector2.Distance(transform.position, player.position) <= stopRange)
                return;

            transform.position = Vector2.MoveTowards(
                transform.position,
                player.position,
                speed * Time.deltaTime);
        }

        private void ShootAtPlayer()
        {
            weaponManager.TryShoot(firePoint, player.position);
        }

        private void RotateFirePoint()
        {
            Vector2 direction = player.position - firePoint.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            firePoint.rotation = Quaternion.Euler(0, 0, angle);
        }

        private bool CanSeePlayer()
        {
            Vector2 direction = (player.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, player.position);

            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                direction,
                distance,
                obstacleMask);

            return hit.collider == null;
        }

        private void PickStrafeDirection()
        {
            strafeDirection = Random.value > 0.5f ? 1 : -1;
        }

        private void StrafeToFindSight()
        {
            Vector2 perpendicular =
                Vector2.Perpendicular((player.position - transform.position).normalized);

            transform.position += (Vector3)(perpendicular * strafeDirection * speed * Time.deltaTime);
        }

        private void StrafeNearPlayer()
        {
            Vector2 perpendicular =
                Vector2.Perpendicular((player.position - transform.position).normalized);

            transform.position += (Vector3)(perpendicular * strafeDirection * speed * 0.5f * Time.deltaTime);
        }
    }
}
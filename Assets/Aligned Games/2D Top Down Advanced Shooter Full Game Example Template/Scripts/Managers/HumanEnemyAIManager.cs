using UnityEngine;

namespace AlignedGames
{

    public class HumanEnemyAIManager : MonoBehaviour
    {
        [Header("AI Settings")]
        public float speed = 2f;  // Movement speed of the enemy
        public float chaseRange = 10f;  // Distance at which enemy starts chasing player
        public float shootRange = 7f;  // Distance at which enemy will shoot player
        public float stopRange = 1.5f; // Distance to stop moving to avoid collision with player
        public float rotationSpeed = 2f; // How fast the enemy rotates to face the player

        [Header("Weapon Settings")]
        public HumanEnemyWeaponManager weaponManager; // Reference to the weapon manager controlling shooting
        public Transform firePoint; // Point from which bullets/projectiles are fired

        [Header("Sight Settings")]
        public LayerMask obstacleMask; // Layers that block enemy's line of sight

        [Header("Aggro Communication")]
        public float aggroBroadcastInterval = 2f; // How often enemy broadcasts aggression to others
        public float aggroBroadcastRadius = 5f; // Radius around enemy to alert other enemies

        private Transform player; // Reference to the player's transform
        public GameObject cachedWeaponPrefab; // Cached weapon prefab for dropping on death
        private bool isAggressive = false; // Whether enemy is currently aggressive

        private Quaternion targetRotation; // Target rotation for random rotating when idle

        private bool lostSightLastFrame = false; // Tracks if enemy lost sight of player in last frame
        private bool isStrafing = false; // Whether enemy is strafing (moving side to side)
        private float originalChaseRange; // Stores original chase range value
        private int strafeDirection = 0; // Direction for strafing: -1 = left, 1 = right
        private float aggroBroadcastTimer = 0f; // Timer for broadcasting aggression

        [Header("Animation Settings")]
        public Animator animator; // Reference to enemy Animator component
        public bool usesRifle = true; // Flag for using rifle animations vs pistol

        private void Start()
        {
            // Find player by tag and cache weapon prefab from weapon manager
            player = GameObject.FindGameObjectWithTag("Player").transform;
            cachedWeaponPrefab = weaponManager.GetSelectedWeaponPrefab();
            SetRandomRotation(); // Initialize a random rotation for idle state
            originalChaseRange = chaseRange; // Save chase range value
        }

        private void Update()
        {
            // If no player found, exit early
            if (player == null) return;

            // Update weapon type flag based on weapon manager pistol state
            usesRifle = !weaponManager.Pistol;

            // Calculate distance from enemy to player
            float distance = Vector2.Distance(transform.position, player.position);
            // Extend visible range if strafing for better detection
            float visibleRange = isStrafing ? chaseRange * 2f : chaseRange;

            // Draw debug line: green if player in chase range and visible, else red
            Color lineColor = (distance <= chaseRange && CanSeePlayer(visibleRange)) ? Color.green : Color.red;
            Debug.DrawLine(transform.position, player.position, lineColor);

            if (isAggressive || distance <= chaseRange)
            {
                // Increment aggression broadcast timer
                aggroBroadcastTimer += Time.deltaTime;
                if (aggroBroadcastTimer >= aggroBroadcastInterval)
                {
                    BroadcastAggression(); // Alert nearby enemies
                    aggroBroadcastTimer = 0f;
                }

                if (!CanSeePlayer(visibleRange))
                {
                    // If lost sight this frame, start strafing
                    if (!lostSightLastFrame)
                    {
                        PickStrafeDirection();
                        lostSightLastFrame = true;
                        isStrafing = true;
                    }

                    StrafeToFindSight(); // Move sideways to regain sight

                    if (distance <= shootRange)
                    {
                        ShootAtPlayer(); // Shoot if in range even when strafing
                    }
                }
                else
                {
                    // Regained sight of player
                    lostSightLastFrame = false;
                    isStrafing = false;

                    if (distance > shootRange)
                    {
                        MoveTowardsPlayer(); // Move closer if out of shooting range
                    }
                    else
                    {
                        // Stop moving if too close, else strafe side to side
                        if (distance > stopRange)
                            MoveTowardsPlayer();
                        else
                            StrafeNearPlayer();

                        ShootAtPlayer(); // Shoot player when in range
                    }
                }

                // Handle animations and rotate to face player
                HandleAnimations(isStrafing || Vector2.Distance(transform.position, player.position) > stopRange);
                RotateTowardsPlayer();

            }
            else
            {
                // Not aggressive and player out of range: idle animations and random rotation
                HandleAnimations(false);
                RotateRandomly();
            }
        }

        private void MoveTowardsPlayer()
        {
            // Calculate direction to player
            Vector2 direction = (player.position - transform.position).normalized;
            float distance = Vector2.Distance(transform.position, player.position);

            // Move towards player if outside stop range
            if (distance > stopRange)
            {
                transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            }
        }

        private void ShootAtPlayer()
        {
            TriggerAggression(); // Set aggressive state
            weaponManager.TryShoot(firePoint, player.position); // Attempt to shoot player
        }

        private void RotateTowardsPlayer()
        {
            // Calculate angle to player and smoothly rotate towards them
            Vector2 direction = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            Quaternion desiredRotation = Quaternion.Euler(0, 0, angle);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
        }

        private void RotateRandomly()
        {
            // Smoothly rotate towards a random target rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // When close to target rotation, pick a new random rotation
            if (Quaternion.Angle(transform.rotation, targetRotation) < 5f)
            {
                SetRandomRotation();
            }
        }

        private void SetRandomRotation()
        {
            // Pick a random angle for idle rotation
            float randomAngle = Random.Range(0f, 360f);
            targetRotation = Quaternion.Euler(0, 0, randomAngle);
        }

        public void TriggerAggression()
        {
            // Set aggressive state to true and log for debugging
            isAggressive = true;
            Debug.Log("Human Enemy is now aggressive!");
        }

        private void OnDestroy()
        {
            // Called when enemy is destroyed, e.g. on death
            Debug.Log("OnDestroy triggered for " + gameObject.name);

            // Drop the cached weapon prefab at a random nearby position
            if (cachedWeaponPrefab != null)
            {
                Vector2 randomOffset = Random.insideUnitCircle * 1.5f;
                Vector3 dropPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0);
                Quaternion randomRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                Instantiate(cachedWeaponPrefab, dropPosition, randomRotation);
            }
        }

        private bool CanSeePlayer(float range)
        {
            // Check line of sight to player with raycast, returns true if no obstacle blocks view
            Vector2 directionToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            float checkDistance = Mathf.Min(range, distanceToPlayer);

            RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, checkDistance, obstacleMask);
            return hit.collider == null;
        }

        private void PickStrafeDirection()
        {
            // Randomly pick strafe direction: left (-1) or right (1)
            strafeDirection = Random.value > 0.5f ? 1 : -1;
        }

        private void StrafeToFindSight()
        {
            // Move sideways perpendicular to player to try to regain line of sight
            Vector2 perpendicular = Vector2.Perpendicular((player.position - transform.position).normalized) * strafeDirection;
            Vector2 targetPosition = (Vector2)transform.position + perpendicular.normalized * speed * Time.deltaTime;
            transform.position = targetPosition;
        }

        // Strafe left or right when too close to player to maintain distance
        private void StrafeNearPlayer()
        {
            Vector2 perpendicular = Vector2.Perpendicular((player.position - transform.position).normalized);
            Vector2 strafeVector = perpendicular * (strafeDirection == 0 ? 1 : strafeDirection) * speed * 0.5f * Time.deltaTime;
            transform.position += (Vector3)strafeVector;
        }

        private void BroadcastAggression()
        {
            // Alert other nearby human enemies to become aggressive
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aggroBroadcastRadius);
            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue; // Skip self

                HumanEnemyAIManager otherAI = hit.GetComponent<HumanEnemyAIManager>();
                if (otherAI != null && !otherAI.isAggressive)
                {
                    otherAI.TriggerAggression();
                }
            }
        }

        private void HandleAnimations(bool isMoving)
        {
            // Control animation states based on movement and weapon type
            if (animator == null) return;

            if (isMoving)
            {
                if (usesRifle)
                {
                    animator.SetBool("isMovingRifle", true);
                    animator.SetBool("isMovingPistol", false);
                    animator.SetBool("isPistolIdle", false);
                    animator.SetBool("isRifleIdle", false);
                }
                else
                {
                    animator.SetBool("isMovingPistol", true);
                    animator.SetBool("isMovingRifle", false);
                    animator.SetBool("isPistolIdle", false);
                    animator.SetBool("isRifleIdle", false);
                }
            }
            else
            {
                if (usesRifle)
                {
                    animator.SetBool("isRifleIdle", true);
                    animator.SetBool("isPistolIdle", false);
                    animator.SetBool("isMovingPistol", false);
                    animator.SetBool("isMovingRifle", false);
                }
                else
                {
                    animator.SetBool("isPistolIdle", true);
                    animator.SetBool("isRifleIdle", false);
                    animator.SetBool("isMovingPistol", false);
                    animator.SetBool("isMovingRifle", false);
                }
            }
        }

    }

}

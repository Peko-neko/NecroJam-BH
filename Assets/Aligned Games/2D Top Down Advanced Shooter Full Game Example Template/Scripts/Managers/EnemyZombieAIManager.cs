using UnityEngine;
using System.Collections;

namespace AlignedGames
{
    public class EnemyZombieAIManager : MonoBehaviour
    {
        [Header("AI Settings")]
        public float speed = 2f; // Movement speed of the zombie
        public float chaseRange = 5f; // Distance at which zombie starts chasing the player
        public float attackRange = 1f; // Distance at which zombie can attack the player
        public int damage = 10; // Damage dealt to player per attack
        public float attackCooldown = 1f; // Time delay between attacks

        [Header("Roaming Settings")]
        public float minRoamDuration = 2f; // Minimum time to roam before idle
        public float maxRoamDuration = 5f; // Maximum time to roam before idle
        public float minIdleDuration = 1f; // Minimum idle time before roaming again
        public float maxIdleDuration = 3f; // Maximum idle time before roaming again

        [Header("Animation Settings")]
        public Animator animator; // Reference to Animator component for zombie animations
        private static readonly int IsMovingHash = Animator.StringToHash("isMoving"); // Hash for moving animation bool
        private static readonly int IsAttackingHash = Animator.StringToHash("isAttacking"); // Hash for attacking animation bool
        private static readonly int IsIdleHash = Animator.StringToHash("isIdle"); // Hash for idle animation bool

        [Header("Audio Settings")]
        public AudioSource audioSource; // Audio source to play zombie sounds
        public AudioClip growlSound; // Default growl sound clip
        public AudioClip attackSound; // Default attack sound clip
        public AudioClip hurtSound; // Default hurt sound clip
        public AudioClip idleSound; // Default idle sound clip

        public AudioClip[] growlClips; // Array of growl clips for randomization
        public AudioClip[] attackClips; // Array of attack clips for randomization
        public AudioClip[] hurtClips; // Array of hurt clips for randomization
        public AudioClip[] idleClips; // Array of idle clips for randomization

        public float randomIdleInterval = 10f; // Seconds between random idle sounds

        [Header("Herd Settings")]
        public float herdAggroRadius = 3f; // Radius to alert other zombies to become aggressive

        [Header("Sight Settings")]
        public LayerMask obstacleMask; // Layer mask for obstacles blocking line of sight

        private Transform player; // Reference to player transform
        private float lastAttackTime; // Timestamp of last attack to enforce cooldown
        private bool isAggressive = false; // Is zombie currently aggressive (chasing player)
        private Vector2 roamDirection; // Current direction zombie is roaming
        private bool isIdle; // Is zombie currently idling

        // Roaming Control
        private float roamTimer; // Timer counting down roaming duration
        private float idleTimer; // Timer counting down idle duration
        private bool isRoaming; // Is zombie currently roaming

        // Strafe Logic
        private bool lostSightLastFrame = false; // Was player lost from sight last frame
        private bool isStrafing = false; // Is zombie strafing to regain sight
        private int strafeDirection = 0; // Direction of strafe (-1 or 1)

        [Header("Sound Play Chances")]
        [Range(0f, 1f)] public float idleSoundPlayChance = 0.2f; // Chance to play idle sound each interval
        [Range(0f, 1f)] public float growlSoundPlayChance = 1f; // Chance to play growl sound when triggered
        [Range(0f, 1f)] public float attackSoundPlayChance = 1f; // Chance to play attack sound on attack
        [Range(0f, 1f)] public float hurtSoundPlayChance = 1f; // Chance to play hurt sound on damage

        [Header("Sound Delay Ranges")]
        public Vector2 idleSoundDelayRange = new Vector2(0f, 0.2f); // Delay range before playing idle sound
        public Vector2 growlSoundDelayRange = new Vector2(0f, 0.2f); // Delay range before playing growl sound
        public Vector2 attackSoundDelayRange = new Vector2(0f, 0.2f); // Delay range before playing attack sound
        public Vector2 hurtSoundDelayRange = new Vector2(0f, 0.2f); // Delay range before playing hurt sound

        private void Start()
        {
            // Find player by tag at start
            player = GameObject.FindGameObjectWithTag("Player").transform;

            SetIdleState(true); // Start in idle state
            StartRoaming(); // Begin roaming behavior
            PlayGrowlSound(); // Play initial growl sound

            if (idleSoundPlayChance > 0f)
                StartCoroutine(PlayIdleLoop()); // Start loop to play idle sounds randomly
        }

        private void Update()
        {
            if (player == null) return; // Exit if no player found

            float distance = Vector2.Distance(transform.position, player.position); // Distance to player

            if (isAggressive || distance <= chaseRange)
            {
                // If cannot see player
                if (!CanSeePlayer())
                {
                    // If lost sight just this frame, pick strafe direction
                    if (!lostSightLastFrame)
                    {
                        PickStrafeDirection();
                        lostSightLastFrame = true;
                        isStrafing = true;
                    }

                    StrafeToFindSight(); // Strafe to try to regain sight
                    SetIdleState(false); // Not idle while strafing

                    if (distance <= attackRange)
                    {
                        AttackPlayer(); // Attack if in range
                    }
                }
                else
                {
                    lostSightLastFrame = false;
                    isStrafing = false;

                    if (distance > attackRange)
                    {
                        MoveTowardsPlayer(); // Chase player
                        SetIdleState(false);
                    }
                    else
                    {
                        AttackPlayer(); // Attack player
                        SetIdleState(false);
                    }
                }

                LookAtPlayer(); // Face the player
            }
            else
            {
                HandleRoaming(); // Otherwise roam around
            }
        }

        // Coroutine to play idle sounds at intervals randomly
        private IEnumerator PlayIdleLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(randomIdleInterval);

                if (idleSoundPlayChance > 0f && Random.value < idleSoundPlayChance)
                {
                    if (audioSource != null)
                    {
                        AudioClip clip = idleClips.Length > 0 ? idleClips[Random.Range(0, idleClips.Length)] : idleSound;
                        if (clip != null) audioSource.PlayOneShot(clip);
                    }
                }
            }
        }

        // Handle roaming and idling logic
        private void HandleRoaming()
        {
            if (isRoaming)
            {
                roamTimer -= Time.deltaTime; // Countdown roaming time
                MoveInRoamDirection(); // Move in roam direction

                if (roamTimer <= 0f)
                {
                    StartIdling(); // Switch to idle after roam duration
                }
            }
            else
            {
                idleTimer -= Time.deltaTime; // Countdown idle time
                SetIdleState(true);

                if (idleTimer <= 0f)
                {
                    StartRoaming(); // Switch to roaming after idle duration
                }
            }
        }

        private void StartRoaming()
        {
            isRoaming = true;
            roamTimer = Random.Range(minRoamDuration, maxRoamDuration); // Random roam time
            SetIdleState(false);
            PickRandomRoamDirection(); // Pick new roam direction
        }

        private void StartIdling()
        {
            isRoaming = false;
            idleTimer = Random.Range(minIdleDuration, maxIdleDuration); // Random idle time
            SetIdleState(true);
        }

        // Pick a random direction to roam in 2D space
        private void PickRandomRoamDirection()
        {
            float randomAngle = Random.Range(0f, 360f);
            roamDirection = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle)).normalized;
        }

        // Move zombie in the roam direction
        private void MoveInRoamDirection()
        {
            transform.position += (Vector3)(roamDirection * speed * Time.deltaTime);
            SetAnimationState(true, false, false); // Moving animation
            LookInRoamDirection(); // Face roam direction
        }

        // Move zombie towards player position
        private void MoveTowardsPlayer()
        {
            Vector2 direction = (player.position - transform.position).normalized;
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            SetAnimationState(true, false, false); // Moving animation
        }

        // Attack player if cooldown allows
        private void AttackPlayer()
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                PlayerHealthManager playerHealth = player.GetComponent<PlayerHealthManager>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage, transform.position);
                    Debug.Log("Zombie Attacked Player");

                    if (attackSoundPlayChance > 0f && Random.value < attackSoundPlayChance)
                    {
                        AudioClip clip = attackClips.Length > 0 ? attackClips[Random.Range(0, attackClips.Length)] : attackSound;
                        PlaySoundWithDelay(clip, attackSoundDelayRange.x, attackSoundDelayRange.y);
                    }
                }

                SetAnimationState(false, true, false); // Attacking animation
            }
        }

        // Rotate zombie to face the player
        private void LookAtPlayer()
        {
            Vector2 direction = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Rotate zombie to face the roam direction
        private void LookInRoamDirection()
        {
            float angle = Mathf.Atan2(roamDirection.y, roamDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Trigger aggressive state and alert nearby zombies
        public void TriggerAggression()
        {
            if (isAggressive) return;

            isAggressive = true;
            Debug.Log("Zombie is now aggressive!");
            PlayGrowlSound();
            SetIdleState(false);
            AlertNearbyZombies();
        }

        // Alert nearby zombies within herdAggroRadius to become aggressive
        private void AlertNearbyZombies()
        {
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, herdAggroRadius);
            foreach (Collider2D col in nearbyColliders)
            {
                if (col != null && col.gameObject != this.gameObject)
                {
                    EnemyZombieAIManager otherZombie = col.GetComponent<EnemyZombieAIManager>();
                    if (otherZombie != null)
                    {
                        otherZombie.TriggerAggression();
                    }
                }
            }
        }

        // Called when zombie takes damage; plays hurt sound
        public void TakeDamage(int damage)
        {
            if (hurtSoundPlayChance > 0f && Random.value < hurtSoundPlayChance)
            {
                AudioClip clip = hurtClips.Length > 0 ? hurtClips[Random.Range(0, hurtClips.Length)] : hurtSound;
                PlaySoundWithDelay(clip, hurtSoundDelayRange.x, hurtSoundDelayRange.y);
            }
        }

        // Play a growl sound with chance and delay
        private void PlayGrowlSound()
        {
            if (growlSoundPlayChance > 0f && Random.value < growlSoundPlayChance)
            {
                AudioClip clip = growlClips.Length > 0 ? growlClips[Random.Range(0, growlClips.Length)] : growlSound;
                PlaySoundWithDelay(clip, growlSoundDelayRange.x, growlSoundDelayRange.y);
            }
        }

        // Set idle animation state and optionally play idle sound
        private void SetIdleState(bool state)
        {
            if (isIdle == state) return;

            isIdle = state;
            if (state)
            {
                SetAnimationState(false, false, true); // Idle animation

                if (idleSoundPlayChance > 0f && Random.value < idleSoundPlayChance)
                {
                    AudioClip clip = idleClips.Length > 0 ? idleClips[Random.Range(0, idleClips.Length)] : idleSound;
                    PlaySoundWithDelay(clip, idleSoundDelayRange.x, idleSoundDelayRange.y);
                }
            }
            else
            {
                SetAnimationState(false, false, false); // Clear animation states
            }
        }

        // Update animator parameters for movement, attack, and idle
        private void SetAnimationState(bool isMoving, bool isAttacking, bool isIdle)
        {
            if (animator == null) return;

            animator.SetBool(IsMovingHash, isMoving);
            animator.SetBool(IsAttackingHash, isAttacking);
            animator.SetBool(IsIdleHash, isIdle);
        }

        // Randomly choose strafe direction left (-1) or right (1)
        private void PickStrafeDirection()
        {
            strafeDirection = Random.value > 0.5f ? 1 : -1;
        }

        // Move sideways to try to regain sight of the player
        private void StrafeToFindSight()
        {
            Vector2 toPlayer = (player.position - transform.position).normalized;
            Vector2 perpendicular = Vector2.Perpendicular(toPlayer) * strafeDirection;
            transform.position += (Vector3)(perpendicular.normalized * speed * Time.deltaTime);
            SetAnimationState(true, false, false); // Moving animation while strafing
        }

        // Check if zombie can see player (no obstacles in line of sight)
        private bool CanSeePlayer()
        {
            Vector2 toPlayer = (player.position - transform.position).normalized;
            float dist = Vector2.Distance(transform.position, player.position);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer, dist, obstacleMask);
            return hit.collider == null;
        }

        // Play a sound clip with a random delay between min and max
        private void PlaySoundWithDelay(AudioClip clip, float minDelay, float maxDelay)
        {
            if (clip != null && audioSource != null)
            {
                float delay = Random.Range(minDelay, maxDelay);
                StartCoroutine(PlayAfterDelay(clip, delay));
            }
        }

        // Coroutine to wait before playing the audio clip
        private IEnumerator PlayAfterDelay(AudioClip clip, float delay)
        {
            yield return new WaitForSeconds(delay);
            audioSource.PlayOneShot(clip);
        }

        // Draw debug visuals in the editor for chase range and herd radius
        private void OnDrawGizmos()
        {
            if (player == null)
                player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (player == null)
                return;

            // Draw line from zombie to player; color depends on state
            Gizmos.color = isAggressive ? Color.magenta : (Vector2.Distance(transform.position, player.position) <= chaseRange ? Color.red : Color.green);
            Gizmos.DrawLine(transform.position, player.position);

            // Draw semi-transparent sphere for herd aggro radius
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
            Gizmos.DrawWireSphere(transform.position, herdAggroRadius);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlignedGames
{
    public class PlayerMeleeAttackBehaviour : MonoBehaviour
    {
        [Header("Melee Settings")]
        [SerializeField] private GameObject meleeHitbox; // The hitbox used to detect melee hits
        [SerializeField] private float meleeDuration = 0.3f; // How long the melee hitbox stays active
        [SerializeField] private int meleeDamage = 50; // Damage dealt by melee attack
        [SerializeField] private float knockbackForce = 5f; // Force applied to enemies when hit
        [SerializeField] private float meleeCooldown = 1f; // Time between allowed melee attacks

        [Header("Attack Animation")]
        [SerializeField] private Animator playerAnimator; // Animator to play melee animation

        [Header("Sound Settings")]
        [SerializeField] private AudioSource audioSource; // Audio source to play attack sounds
        [SerializeField] private AudioClip[] attackSounds; // Array of possible attack sounds

        private HashSet<Collider2D> hitTargets = new HashSet<Collider2D>(); // Keeps track of already hit targets to avoid double hits

        [Header("Input Actions")]
        public InputAction meleeAction; // Input action for melee attack

        private bool isCooldown = false; // Whether melee is on cooldown

        public GameObject hitEffectPrefab; // Visual effect prefab for hitting obstacles
        public GameObject bloodEffectPrefab; // Visual effect prefab for hitting enemies (blood)

        [Header("Random Visuals")]
        public Sprite[] hitSprites; // Possible sprites for hit effects on obstacles
        public Sprite[] bloodSprites; // Possible sprites for blood effects on enemies

        [Header("Hit Sounds")]
        public AudioClip[] obstaclehitSounds; // Sounds when hitting obstacles
        public AudioClip[] enemyhitSounds; // Sounds when hitting enemies
        public float hitSoundVolume = 0.7f; // Volume for hit sounds

        public GameObject GunToHide; // Gun object to hide during melee attack

        private void OnEnable()
        {
            if (!meleeAction.enabled) meleeAction.Enable(); // Enable input action when script is enabled
        }

        private void OnDisable()
        {
            meleeAction.Disable(); // Disable input action when script is disabled
        }

        public void Start()
        {
            meleeHitbox.SetActive(false); // Make sure the melee hitbox is off at start
        }

        private void Update()
        {
            HandleMeleeAttack(); // Check for input each frame
        }

        private void HandleMeleeAttack()
        {
            // If melee button pressed and not on cooldown, perform attack
            if (meleeAction.WasPressedThisFrame() && !isCooldown)
            {
                PerformMeleeAttack();
            }
        }

        private void PerformMeleeAttack()
        {
            if (playerAnimator != null)
            {
                playerAnimator.Play("Melee"); // Play melee animation
            }

            PlayAttackSound(); // Play random attack sound

            GunToHide.SetActive(false); // Hide the gun during melee

            StartCoroutine(ActivateMeleeHitbox()); // Enable melee hitbox temporarily
            StartCoroutine(StartCooldown()); // Start melee cooldown timer
        }

        private IEnumerator ActivateMeleeHitbox()
        {
            hitTargets.Clear(); // Clear list of hit targets to avoid double hits in this attack
            meleeHitbox.SetActive(true); // Activate hitbox
            yield return new WaitForSeconds(meleeDuration); // Wait for melee duration
            meleeHitbox.SetActive(false); // Disable hitbox
        }

        private IEnumerator StartCooldown()
        {
            isCooldown = true; // Start cooldown

            yield return new WaitForSeconds(meleeCooldown / 2);
            GunToHide.SetActive(true); // Re-enable gun halfway through cooldown

            yield return new WaitForSeconds(meleeCooldown);
            isCooldown = false; // End cooldown
        }

        private void PlayAttackSound()
        {
            if (audioSource != null && attackSounds.Length > 0)
            {
                AudioClip randomSound = attackSounds[Random.Range(0, attackSounds.Length)];
                audioSource.PlayOneShot(randomSound); // Play a random attack sound
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Rotation to make hit effects face correctly
            Quaternion oppositeRotation = Quaternion.LookRotation(Vector3.forward, -transform.up);

            if (!meleeHitbox.activeSelf) return; // Ignore if hitbox inactive
            if (hitTargets.Contains(collision)) return; // Ignore if already hit this collider this attack

            hitTargets.Add(collision); // Add collider to hit list

            if (collision.CompareTag("Enemy"))
            {
                // Deal damage to enemy
                EnemyHealthManager enemyHealth = collision.GetComponent<EnemyHealthManager>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(meleeDamage);
                }

                // Apply knockback force if enemy has Rigidbody2D
                if (knockbackForce > 0 && collision.TryGetComponent<Rigidbody2D>(out Rigidbody2D rb))
                {
                    Vector2 knockbackDirection = (collision.transform.position - transform.position).normalized;
                    rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
                }

                // Trigger aggression on AI enemies if they have the scripts
                EnemyZombieAIManager zombieAI = collision.GetComponent<EnemyZombieAIManager>();
                if (zombieAI != null)
                {
                    zombieAI.TriggerAggression();
                }

                HumanEnemyAIManager humanAI = collision.GetComponent<HumanEnemyAIManager>();
                if (humanAI != null)
                {
                    humanAI.TriggerAggression();
                }

                // Spawn blood effect
                if (bloodEffectPrefab != null)
                {
                    GameObject blood = Instantiate(bloodEffectPrefab, meleeHitbox.transform.position, oppositeRotation);
                    TryAssignRandomSprite(blood, bloodSprites);
                }

                PlayRandomEnemyHitSound(); // Play enemy hit sound
            }
            else if (collision.CompareTag("Obstacle") || collision.CompareTag("Wall"))
            {
                // Spawn hit effect on obstacles or walls
                if (hitEffectPrefab != null)
                {
                    GameObject hit = Instantiate(hitEffectPrefab, meleeHitbox.transform.position, oppositeRotation);
                    TryAssignRandomSprite(hit, hitSprites);
                }

                PlayRandomObstacleHitSound(); // Play obstacle hit sound
            }
        }

        private void TryAssignRandomSprite(GameObject obj, Sprite[] spriteArray)
        {
            if (spriteArray != null && spriteArray.Length > 0)
            {
                SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                    sr.sprite = spriteArray[Random.Range(0, spriteArray.Length)]; // Assign a random sprite from array
            }
        }

        private void PlayRandomEnemyHitSound()
        {
            if (enemyhitSounds != null && enemyhitSounds.Length > 0)
            {
                AudioClip clip = enemyhitSounds[Random.Range(0, enemyhitSounds.Length)];
                if (clip == null) return;

                // Create temporary audio source for 2D sound
                GameObject audioObj = new GameObject("TempEnemyHitAudio");
                audioObj.transform.position = transform.position;

                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.clip = clip;
                source.volume = hitSoundVolume;
                source.spatialBlend = 0f; // 0 = 2D sound
                source.Play();

                Destroy(audioObj, clip.length); // Destroy after clip finishes
            }
        }

        private void PlayRandomObstacleHitSound()
        {
            if (obstaclehitSounds != null && obstaclehitSounds.Length > 0)
            {
                AudioClip clip = obstaclehitSounds[Random.Range(0, obstaclehitSounds.Length)];
                if (clip == null) return;

                // Create temporary audio source for 2D sound
                GameObject audioObj = new GameObject("TempObstacleHitAudio");
                audioObj.transform.position = transform.position;

                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.clip = clip;
                source.volume = hitSoundVolume;
                source.spatialBlend = 0f; // 0 = 2D sound
                source.Play();

                Destroy(audioObj, clip.length); // Destroy after clip finishes
            }
        }
    }
}

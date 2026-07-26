using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace AlignedGames
{
    public class PlayerMeleeAttackBehaviour : MonoBehaviour
    {

        [SerializeField] private SpriteRenderer weaponRenderer;

        [Header("Weapon Sprites")]
        [SerializeField] private Sprite idleWeaponSprite;
        [SerializeField] private Sprite blockWeaponSprite;

        [Header("Weapon Animator")]
        [SerializeField] private Animator weaponAnimator;

        [Header("Melee")]
        [SerializeField] private Transform attackPoint;
        [SerializeField] private Vector2 attackSize = new Vector2(2f, 1f);
        [SerializeField] private LayerMask meleeLayers;

        [SerializeField] private int meleeDamage = 50;
        [SerializeField] private float knockbackForce = 5f;
        [SerializeField] private float meleeCooldown = 1f;
        [SerializeField] private float attackDelay = 0.1f;

        [Header("Blocking")]
        [SerializeField] private InputAction blockAction;
        [SerializeField] private GameObject blockHitbox;

        [SerializeField] private SpriteRenderer playerSprite;
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite blockSprite;

        [SerializeField] private AudioClip blockSound;

        [SerializeField] private int powerPerBullet = 1;

        [Header("Guard")]
        [SerializeField] private int maxGuard = 100;
        [SerializeField] private int guardCostPerBullet = 20;
        [SerializeField] private float guardRecoverDelay = 2f;
        [SerializeField] private float guardRecoverRate = 40f;
        [SerializeField] private float guardBreakDuration = 1.5f;

        [SerializeField] private AudioClip guardBreakSound;
        private float currentGuard;
        private float lastGuardTime;
        private bool guardBroken;

        [Header("Animation")]
        [SerializeField] private Animator playerAnimator;

        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] attackSounds;

        [Header("Effects")]
        public GameObject hitEffectPrefab;
        public GameObject bloodEffectPrefab;

        public Sprite[] hitSprites;
        public Sprite[] bloodSprites;

        public AudioClip[] obstaclehitSounds;
        public AudioClip[] enemyhitSounds;
        public float hitSoundVolume = 0.7f;

        [Header("Input")]
        public InputAction meleeAction;

        private bool isCooldown;
        private bool isBlocking;

        private int storedPower;

        private readonly HashSet<Collider2D> hitTargets = new();

        void OnEnable()
        {
            if (!meleeAction.enabled)
                meleeAction.Enable();

            blockAction.Enable();
        }

        void OnDisable()
        {
            meleeAction.Disable();
            blockAction.Disable();
        }

        void Start()
        {
            currentGuard = maxGuard;
        }

        void Update()
        {
            HandleMeleeAttack();
            HandleBlock();
            RecoverGuard();
        }

        void HandleMeleeAttack()
        {
            if (guardBroken)
                return;

            if (meleeAction.WasPressedThisFrame() && !isCooldown)
                PerformMeleeAttack();
        }

        void PerformMeleeAttack()
        {
            if (weaponAnimator)
                weaponAnimator.Play("Melee", 0, 0f);

            PlayAttackSound();

            StartCoroutine(PerformAttack());
            StartCoroutine(StartCooldown());
        }

        IEnumerator PerformAttack()
        {
            yield return new WaitForSeconds(attackDelay);

            hitTargets.Clear();

            Collider2D[] hits =
                Physics2D.OverlapBoxAll(
                    attackPoint.position,
                    attackSize,
                    attackPoint.eulerAngles.z,
                    meleeLayers);

            Quaternion rotation =
                Quaternion.LookRotation(
                    Vector3.forward,
                    -transform.up);

            foreach (Collider2D hit in hits)
            {
                if (hitTargets.Contains(hit))
                    continue;

                hitTargets.Add(hit);

                HandleHit(hit, rotation);
            }

            weaponAnimator.Play("Idle");
        }

        void HandleBlock()
        {
            if (guardBroken)
            {
                if (isBlocking)
                    EndBlock();

                return;
            }

            if (blockAction.IsPressed())
            {
                if (!isBlocking)
                    StartBlock();
            }
            else
            {
                if (isBlocking)
                    EndBlock();
            }
        }

        void StartBlock()
        {
            isBlocking = true;

            blockHitbox.SetActive(true);

            if (weaponRenderer != null)
            {
                weaponRenderer.enabled = true;
                weaponRenderer.sprite = blockWeaponSprite;
            }
        }

        void EndBlock()
        {
            isBlocking = false;

            blockHitbox.SetActive(false);

            if (weaponRenderer != null)
            {
                weaponRenderer.sprite = idleWeaponSprite;
            }
        }

        void RecoverGuard()
        {
            if (guardBroken)
                return;

            if (Time.time - lastGuardTime < guardRecoverDelay)
                return;

            currentGuard =
                Mathf.Min(
                    maxGuard,
                    currentGuard +
                    guardRecoverRate *
                    Time.deltaTime);
        }

        IEnumerator GuardBreak()
        {
            guardBroken = true;

            EndBlock();

            if (audioSource && guardBreakSound)
                audioSource.PlayOneShot(guardBreakSound);

            if (playerAnimator)
                playerAnimator.Play("Stunned");

            yield return new WaitForSeconds(
                guardBreakDuration);

            currentGuard = maxGuard;

            guardBroken = false;
        }

        IEnumerator StartCooldown()
        {
            isCooldown = true;

            yield return new WaitForSeconds(
                meleeCooldown * 0.5f);

            yield return new WaitForSeconds(
                meleeCooldown * 0.5f);

            isCooldown = false;
        }

        private void HandleHit(
    Collider2D collision,
    Quaternion oppositeRotation)
        {
            if (collision.CompareTag("Enemy"))
            {
                EnemyHealthManager enemyHealth =
                    collision.GetComponent<EnemyHealthManager>();

                if (enemyHealth != null)
                    enemyHealth.TakeDamage(meleeDamage);

                if (knockbackForce > 0 &&
                    collision.TryGetComponent(
                        out Rigidbody2D rb))
                {
                    Vector2 knockbackDirection =
                        (collision.transform.position -
                        transform.position).normalized;

                    rb.AddForce(
                        knockbackDirection * knockbackForce,
                        ForceMode2D.Impulse);
                }

                EnemyZombieAIManager zombieAI =
                    collision.GetComponent<EnemyZombieAIManager>();

                if (zombieAI != null)
                    zombieAI.TriggerAggression();

                HumanEnemyAIManager humanAI =
                    collision.GetComponent<HumanEnemyAIManager>();

                if (humanAI != null)
                    humanAI.TriggerAggression();

                if (bloodEffectPrefab != null)
                {
                    GameObject blood =
                        Instantiate(
                            bloodEffectPrefab,
                            collision.ClosestPoint(transform.position),
                            oppositeRotation);

                    TryAssignRandomSprite(
                        blood,
                        bloodSprites);
                }

                PlayRandomEnemyHitSound();
            }
            else if (
                collision.CompareTag("Obstacle") ||
                collision.CompareTag("Wall"))
            {
                if (hitEffectPrefab != null)
                {
                    GameObject hit =
                        Instantiate(
                            hitEffectPrefab,
                            collision.ClosestPoint(transform.position),
                            oppositeRotation);

                    TryAssignRandomSprite(
                        hit,
                        hitSprites);
                }

                PlayRandomObstacleHitSound();
            }
            else if (collision.CompareTag("Bullet"))
            {
                if (hitEffectPrefab != null)
                {
                    GameObject hit =
                        Instantiate(
                            hitEffectPrefab,
                            collision.transform.position,
                            oppositeRotation);

                    TryAssignRandomSprite(
                        hit,
                        hitSprites);
                }

                PlayRandomObstacleHitSound();

                Destroy(collision.gameObject);
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!isBlocking)
                return;

            if (!collision.CompareTag("Bullet"))
                return;

            storedPower += powerPerBullet;

            currentGuard -= guardCostPerBullet;
            lastGuardTime = Time.time;

            if (audioSource && blockSound)
                audioSource.PlayOneShot(blockSound);

            Destroy(collision.gameObject);

            if (currentGuard <= 0f)
                StartCoroutine(GuardBreak());
        }

        private void PlayAttackSound()
        {
            if (audioSource == null)
                return;

            if (attackSounds == null ||
                attackSounds.Length == 0)
                return;

            AudioClip clip =
                attackSounds[
                    Random.Range(
                        0,
                        attackSounds.Length)];

            audioSource.PlayOneShot(clip);
        }

        private void TryAssignRandomSprite(
            GameObject obj,
            Sprite[] spriteArray)
        {
            if (spriteArray == null)
                return;

            if (spriteArray.Length == 0)
                return;

            SpriteRenderer sr =
                obj.GetComponentInChildren<SpriteRenderer>();

            if (sr != null)
            {
                sr.sprite =
                    spriteArray[
                        Random.Range(
                            0,
                            spriteArray.Length)];
            }
        }

        private void PlayRandomEnemyHitSound()
        {
            if (enemyhitSounds == null)
                return;

            if (enemyhitSounds.Length == 0)
                return;

            AudioClip clip =
                enemyhitSounds[
                    Random.Range(
                        0,
                        enemyhitSounds.Length)];

            if (clip == null)
                return;

            GameObject audioObj =
                new GameObject("TempEnemyHitAudio");

            audioObj.transform.position =
                transform.position;

            AudioSource source =
                audioObj.AddComponent<AudioSource>();

            source.clip = clip;
            source.volume = hitSoundVolume;
            source.spatialBlend = 0f;

            source.Play();

            Destroy(audioObj, clip.length);
        }

        private void PlayRandomObstacleHitSound()
        {
            if (obstaclehitSounds == null)
                return;

            if (obstaclehitSounds.Length == 0)
                return;

            AudioClip clip =
                obstaclehitSounds[
                    Random.Range(
                        0,
                        obstaclehitSounds.Length)];

            if (clip == null)
                return;

            GameObject audioObj =
                new GameObject("TempObstacleHitAudio");

            audioObj.transform.position =
                transform.position;

            AudioSource source =
                audioObj.AddComponent<AudioSource>();

            source.clip = clip;
            source.volume = hitSoundVolume;
            source.spatialBlend = 0f;

            source.Play();

            Destroy(audioObj, clip.length);
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
                return;

            Gizmos.color = Color.red;

            Gizmos.matrix =
                Matrix4x4.TRS(
                    attackPoint.position,
                    attackPoint.rotation,
                    Vector3.one);

            Gizmos.DrawWireCube(
                Vector3.zero,
                attackSize);
        }
    }
}
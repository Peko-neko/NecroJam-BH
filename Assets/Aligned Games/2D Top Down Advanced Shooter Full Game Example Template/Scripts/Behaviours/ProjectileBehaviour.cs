using UnityEngine;

namespace AlignedGames
{

    public class ProjectileBehaviour : MonoBehaviour
    {
        [Header("Bullet Settings")]
        private int damage = 10; // Default damage dealt by the projectile
        public float lifetime = 3f; // Time before the projectile is destroyed automatically
        public bool destroyOnImpact = true; // Whether to destroy the projectile on hitting something
        public GameObject explosionEffectPrefab; // Optional explosion visual effect on impact
        public GameObject hitEffectPrefab; // Optional visual effect for hitting obstacles
        public GameObject bloodEffectPrefab; // Optional blood effect for hitting enemies

        [Header("Random Visuals")]
        public Sprite[] hitSprites; // Possible sprites for obstacle hit effects
        public Sprite[] bloodSprites; // Possible sprites for blood effects

        [Header("Hit Sounds")]
        public AudioClip[] obstaclehitSounds; // Sounds to play when hitting obstacles
        public AudioClip[] enemyhitSounds; // Sounds to play when hitting enemies
        public float hitSoundVolume = 0.7f; // Volume for hit sounds

        private void Start()
        {
            // Destroy the projectile after its lifetime expires
            Destroy(gameObject, lifetime);
        }

        // Allows other scripts to change the damage value
        public void SetDamage(int damageValue)
        {
            damage = damageValue;
        }

        // Called when the projectile collides with a trigger collider
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Create a rotation that faces opposite the projectile's direction
            Quaternion oppositeRotation = Quaternion.LookRotation(Vector3.forward, -transform.up);

            if (collision.CompareTag("Enemy"))
            {
                // Instantiate a blood effect if available
                if (bloodEffectPrefab != null)
                {
                    GameObject blood = Instantiate(bloodEffectPrefab, transform.position, oppositeRotation);
                    TryAssignRandomSprite(blood, bloodSprites); // Randomize blood sprite
                }

                // Deal damage if the enemy has a health manager
                var enemyHealth = collision.GetComponent<EnemyHealthManager>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(damage);

                // Trigger AI aggression if available
                var zombieAI = collision.GetComponent<EnemyZombieAIManager>();
                if (zombieAI != null)
                    zombieAI.TriggerAggression();

                var humanAI = collision.GetComponent<HumanEnemyAIManager>();
                if (humanAI != null)
                    humanAI.TriggerAggression();

                // Create explosion effect if assigned
                if (explosionEffectPrefab != null)
                    Instantiate(explosionEffectPrefab, transform.position, oppositeRotation);

                // Play a random enemy hit sound
                PlayRandomEnemyHitSound();

                // Optionally destroy the projectile
                if (destroyOnImpact)
                    Destroy(gameObject);
            }
            else if (collision.CompareTag("Obstacle") || collision.CompareTag("Wall"))
            {
                // Instantiate visual effect for obstacle hit
                if (hitEffectPrefab != null)
                {
                    GameObject hit = Instantiate(hitEffectPrefab, transform.position, oppositeRotation);
                    TryAssignRandomSprite(hit, hitSprites); // Randomize hit sprite
                }

                // Try to damage the object if they have a health component
                var objectHealth = collision.GetComponent<ObjectHealthManager>();
                if (objectHealth != null)
                    objectHealth.TakeDamage(damage);

                // Create explosion effect if assigned
                if (explosionEffectPrefab != null)
                    Instantiate(explosionEffectPrefab, transform.position, oppositeRotation);

                // Play a random obstacle hit sound
                PlayRandomObstacleHitSound();

                // Optionally destroy the projectile
                if (destroyOnImpact)
                    Destroy(gameObject);
            }
        }

        // Assigns a random sprite from a given array to a SpriteRenderer in the object
        private void TryAssignRandomSprite(GameObject obj, Sprite[] spriteArray)
        {
            if (spriteArray != null && spriteArray.Length > 0)
            {
                SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                    sr.sprite = spriteArray[Random.Range(0, spriteArray.Length)];
            }
        }

        // Plays a random sound effect for enemy hits
        private void PlayRandomEnemyHitSound()
        {
            if (enemyhitSounds != null && enemyhitSounds.Length > 0)
            {
                AudioClip clip = enemyhitSounds[Random.Range(0, enemyhitSounds.Length)];
                if (clip == null) return;

                // Create temporary GameObject for audio playback
                GameObject audioObj = new GameObject("TempEnemyHitAudio");
                audioObj.transform.position = transform.position;

                // Configure and play the sound
                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.clip = clip;
                source.volume = hitSoundVolume;
                source.spatialBlend = 0f; // 0 = 2D sound
                source.Play();

                // Destroy the audio object after the sound finishes
                Destroy(audioObj, clip.length);
            }
        }

        // Plays a random sound effect for obstacle hits
        private void PlayRandomObstacleHitSound()
        {
            if (obstaclehitSounds != null && obstaclehitSounds.Length > 0)
            {
                AudioClip clip = obstaclehitSounds[Random.Range(0, obstaclehitSounds.Length)];
                if (clip == null) return;

                // Create temporary GameObject for audio playback
                GameObject audioObj = new GameObject("TempObstacleHitAudio");
                audioObj.transform.position = transform.position;

                // Configure and play the sound
                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.clip = clip;
                source.volume = hitSoundVolume;
                source.spatialBlend = 0f; // 0 = 2D sound
                source.Play();

                // Destroy the audio object after the sound finishes
                Destroy(audioObj, clip.length);
            }
        }
    }
}

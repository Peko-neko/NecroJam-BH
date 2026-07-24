// Import Unity engine functionalities
using UnityEngine;

// Namespace to group related game classes under 'AlignedGames'
namespace AlignedGames
{
    // This class controls the behavior of a projectile (e.g., bullet) fired by enemies
    public class EnemyProjectileBehaviour : MonoBehaviour
    {
        private int damage = 10; // How much damage the bullet deals
        public float lifetime = 3f; // How long the bullet exists before auto-destroying
        public bool destroyOnImpact = true; // Should it disappear when hitting something?

        public GameObject explosionEffectPrefab; // Optional explosion effect on impact
        public GameObject hitEffectPrefab; // Optional generic hit effect
        public GameObject bloodEffectPrefab; // Optional blood effect if hitting player

        // ---------- Random Visuals ----------
        public Sprite[] hitSprites; // Optional sprites used in hit effects
        public Sprite[] bloodSprites; // Optional sprites used in blood effects

        // ---------- Hit Sounds ----------
        public AudioClip[] obstaclehitSounds; // Sounds for when bullet hits environment
        public AudioClip[] enemyhitSounds; // Sounds for when bullet hits player
        public float hitSoundVolume = 0.7f; // Volume of hit sounds

        // Called when the bullet is first created
        private void Start()
        {
            // Destroy the bullet after 'lifetime' seconds automatically
            Destroy(gameObject, lifetime);
        }

        // Allows setting bullet damage from other scripts
        public void SetDamage(int damageValue)
        {
            damage = damageValue;
        }

        // Called when the bullet collides with something
        private void OnTriggerEnter2D(Collider2D collision)
        {
            // Calculate visual rotation for effects
            Quaternion oppositeRotation = Quaternion.LookRotation(Vector3.forward, -transform.up);

            // If bullet hits the player
            if (collision.CompareTag("Player"))
            {
                // Spawn blood effect
                if (bloodEffectPrefab != null)
                {
                    GameObject blood = Instantiate(bloodEffectPrefab, transform.position, oppositeRotation);
                    TryAssignRandomSprite(blood, bloodSprites); // Assign a random blood sprite
                }

                // Try to damage the player if they have a health component
                var playerHealth = collision.GetComponent<PlayerHealthManager>();
                if (playerHealth != null)
                    playerHealth.TakeDamage(damage, transform.position);

                // Spawn explosion visual effect
                if (explosionEffectPrefab != null)
                    Instantiate(explosionEffectPrefab, transform.position, oppositeRotation);

                // Play a sound for hitting the player
                PlayRandomEnemyHitSound();

                // Destroy the bullet if set to do so
                if (destroyOnImpact)
                    Destroy(gameObject);

                return; // stop further processing for this collision
            }

            // If bullet hits an enemy (e.g., wake nearby AI)
            else if (collision.CompareTag("Enemy"))
            {
                // Trigger aggression in any nearby enemies or AI systems
                var zombieAI = collision.GetComponent<EnemyZombieAIManager>();
                if (zombieAI != null)
                    zombieAI.TriggerAggression();

                var humanAI = collision.GetComponent<HumanEnemyAIManager>();
                if (humanAI != null)
                    humanAI.TriggerAggression();

                // Deal damage if the enemy has a health manager
                var enemyHealth = collision.GetComponent<EnemyHealthManager>();
                if (enemyHealth != null)
                    enemyHealth.TakeDamage(damage);

                return; // no further effects for enemy hits in this script
            }

            // If bullet hits environment like a wall or obstacle
            else if (collision.CompareTag("Obstacle") || collision.CompareTag("Wall"))
            {
                // Spawn visual hit effect
                if (hitEffectPrefab != null)
                {
                    GameObject hit = Instantiate(hitEffectPrefab, transform.position, oppositeRotation);
                    TryAssignRandomSprite(hit, hitSprites); // Assign a random hit sprite
                }

                // Try to damage the object if they have a health component
                var objectHealth = collision.GetComponent<ObjectHealthManager>();
                if (objectHealth != null)
                    objectHealth.TakeDamage(damage);

                // Optional explosion effect
                if (explosionEffectPrefab != null)
                    Instantiate(explosionEffectPrefab, transform.position, oppositeRotation);

                // Play obstacle hit sound
                PlayRandomObstacleHitSound();

                // Destroy the bullet if set to do so
                if (destroyOnImpact)
                    Destroy(gameObject);

                return; // stop further processing for this collision
            }

            // (No tag matched) — do nothing
        }

        // Utility function to assign a random sprite from an array
        private void TryAssignRandomSprite(GameObject obj, Sprite[] spriteArray)
        {
            if (spriteArray != null && spriteArray.Length > 0)
            {
                SpriteRenderer sr = obj.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                    sr.sprite = spriteArray[Random.Range(0, spriteArray.Length)];
            }
        }

        // Plays a random sound from the enemy hit sounds
        private void PlayRandomEnemyHitSound()
        {
            if (enemyhitSounds != null && enemyhitSounds.Length > 0)
            {
                AudioClip clip = enemyhitSounds[Random.Range(0, enemyhitSounds.Length)];
                if (clip == null) return;

                // Create a temporary object to play the sound
                GameObject audioObj = new GameObject("TempEnemyHitAudio");
                audioObj.transform.position = transform.position;

                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.clip = clip;
                source.volume = hitSoundVolume;
                source.spatialBlend = 0f; // 2D sound
                source.Play();

                // Destroy audio object after clip finishes
                Destroy(audioObj, clip.length);
            }
        }

        // Plays a random sound from the obstacle hit sounds
        private void PlayRandomObstacleHitSound()
        {
            if (obstaclehitSounds != null && obstaclehitSounds.Length > 0)
            {
                AudioClip clip = obstaclehitSounds[Random.Range(0, obstaclehitSounds.Length)];
                if (clip == null) return;

                GameObject audioObj = new GameObject("TempObstacleHitAudio");
                audioObj.transform.position = transform.position;

                AudioSource source = audioObj.AddComponent<AudioSource>();
                source.clip = clip;
                source.volume = hitSoundVolume;
                source.spatialBlend = 0f; // 2D sound
                source.Play();

                Destroy(audioObj, clip.length);
            }
        }
    }

}

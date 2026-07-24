using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

namespace AlignedGames
{

    public class PlayerHealthManager : MonoBehaviour
    {
        [Header("Health Settings")]
        // Maximum health value
        public int maxHealth = 100;
        // Current health value
        public int currentHealth;

        [Header("Armor Settings")]
        // Maximum armor value
        public int maxArmor = 100;
        // Current armor value
        public int currentArmor;

        [Header("UI Elements - Health")]
        // UI slider showing health
        public Slider healthSlider;
        // UI text showing health numbers
        public TextMeshProUGUI healthText;

        [Header("UI Elements - Armor")]
        // UI slider showing armor
        public Slider armorSlider;
        // UI text showing armor numbers
        public TextMeshProUGUI armorText;

        [Header("Pickup Settings")]
        // Reference to the pickup item nearby
        public PickupIdentifier nearbyPickup;
        // Flag to check if player is currently picking up an item
        private bool isPickingUp = false;
        // Key assigned for pickup action (default is E)
        [SerializeField] private Key pickupKey = Key.E;
        // UI text showing pickup instructions
        [SerializeField] private TextMeshProUGUI pickupText;

        [Header("Pickup Sound Settings")]
        // Audio source component for playing sounds
        [SerializeField] private AudioSource audioSource;
        // Array of possible pickup sounds to play randomly
        [SerializeField] private AudioClip[] pickupSounds;

        [Header("Hit Feedback")]
        // Sound played when player is hit
        public AudioClip hitSound;
        // Prefab used to create screen shake effect on hit
        public GameObject ScreenShakePrefab;

        [Header("Blood Decal Settings")]
        // Blood decal prefab to spawn on hit
        public GameObject bloodDecalPrefab;
        // Range for random scale of blood decal
        public Vector2 bloodDecalScaleRange = new Vector2(0.8f, 1.2f);
        // Range for random rotation of blood decal
        public Vector2 bloodDecalRotationRange = new Vector2(0f, 360f);
        // Sorting order for blood decal rendering
        public int sortingOrder;
        // How long blood decal lasts before fading
        public float decalLifetime = 5f;
        // Duration of fade effect for blood decal
        public float fadeDuration = 1f;

        [Header("Hit Direction UI")]
        // UI images for hit direction indicators
        public Image hitLeft;
        public Image hitRight;
        public Image hitTop;
        public Image hitBottom;
        // Speed at which hit indicators fade out
        public float fadeSpeed = 1f;

        private void Awake()
        {
            // Initialize health and armor to max values at start
            currentHealth = maxHealth;
            currentArmor = maxArmor;
            UpdateHealthUI();
            UpdateArmorUI();
        }

        private void Start()
        {
            // Also ensure values are set at Start (redundant but safe)
            currentHealth = maxHealth;
            currentArmor = maxArmor;
            UpdateHealthUI();
            UpdateArmorUI();
        }

        private void Update()
        {
            // Check for pickup input and handle fading hit indicators each frame
            HandleHealthAndArmorPickup();
            FadeOutHitIndicators();
        }

        private void SpawnBloodDecal()
        {
            // Spawn a blood decal with random rotation and scale on player position
            if (bloodDecalPrefab == null) return;

            GameObject decalGO = Instantiate(bloodDecalPrefab, transform.position, Quaternion.identity);
            float rotation = Random.Range(bloodDecalRotationRange.x, bloodDecalRotationRange.y);
            decalGO.transform.rotation = Quaternion.Euler(0f, 0f, rotation);
            float scale = Random.Range(bloodDecalScaleRange.x, bloodDecalScaleRange.y);

            var bloodDecal = decalGO.GetComponent<SpriteFadeBehaviour>();
            if (bloodDecal != null)
            {
                // Initialize decal properties for fading and lifetime
                bloodDecal.Initialize(sortingOrder, scale, decalLifetime, fadeDuration);
            }
        }

        private void HandleHealthAndArmorPickup()
        {
            // If pickup key pressed and player is near a pickup
            if (Keyboard.current != null && Keyboard.current[pickupKey].wasPressedThisFrame && nearbyPickup != null)
            {
                isPickingUp = true;
                switch (nearbyPickup.pickupType)
                {
                    case PickupIdentifier.PickupItemType.Health:
                        PickupHealth(nearbyPickup);
                        break;
                    case PickupIdentifier.PickupItemType.Armor:
                        PickupArmor(nearbyPickup);
                        break;
                }

                PlayPickupSound();

                // Remove the pickup object after pickup
                if (nearbyPickup != null)
                    Destroy(nearbyPickup.gameObject);

                nearbyPickup = null;
                isPickingUp = false;
                if (pickupText != null) pickupText.gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Detect when player enters pickup collider
            if (other.CompareTag("Pickup"))
            {
                PickupIdentifier pickup = other.GetComponent<PickupIdentifier>();
                if (pickup != null && (pickup.pickupType == PickupIdentifier.PickupItemType.Health ||
                                       pickup.pickupType == PickupIdentifier.PickupItemType.Armor))
                {
                    nearbyPickup = pickup;
                    if (pickupText != null)
                    {
                        // Show pickup instructions
                        pickupText.gameObject.SetActive(true);
                        pickupText.text = $"Press [{pickupKey}] to pick up {pickup.pickupType}";
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Detect when player leaves pickup collider
            if (other.CompareTag("Pickup") && other.GetComponent<PickupIdentifier>() == nearbyPickup)
            {
                nearbyPickup = null;
                if (pickupText != null) pickupText.gameObject.SetActive(false);
            }
        }

        private void PlayPickupSound()
        {
            // Play a random pickup sound from the array
            if (pickupSounds.Length > 0 && audioSource != null)
            {
                AudioClip randomSound = pickupSounds[Random.Range(0, pickupSounds.Length)];
                audioSource.PlayOneShot(randomSound);
            }
        }

        public void PickupHealth(PickupIdentifier healthPickup)
        {
            // Pickup health and heal player
            if (healthPickup == null || healthPickup.pickupType != PickupIdentifier.PickupItemType.Health) return;
            Heal(healthPickup.healthToRestore);
        }

        public void PickupArmor(PickupIdentifier armorPickup)
        {
            // Pickup armor and add to current armor
            if (armorPickup == null || armorPickup.pickupType != PickupIdentifier.PickupItemType.Armor) return;
            AddArmor(armorPickup.armorToRestore);
        }

        public void TakeDamage(int damage, Vector2 sourcePosition)
        {
            // Called when player takes damage, applies armor first, then health

            SpawnBloodDecal();
            PlayHitFeedback(damage);

            // Calculate direction player was hit from
            Vector2 hitDirection = ((Vector2)transform.position - sourcePosition).normalized;
            string direction = "unknown";

            if (Mathf.Abs(hitDirection.x) > Mathf.Abs(hitDirection.y))
                direction = hitDirection.x > 0 ? "left" : "right";
            else
                direction = hitDirection.y > 0 ? "bottom" : "top";

            // Show hit indicator on UI based on direction and damage severity
            ShowHitIndicator(direction, damage);

            if (damage <= 0) return;

            // Apply damage to armor first, reducing damage if possible
            if (currentArmor > 0)
            {
                int damageToArmor = Mathf.Min(damage, currentArmor);
                currentArmor -= damageToArmor;
                damage -= damageToArmor;
                UpdateArmorUI();
            }

            // Apply remaining damage to health
            if (damage > 0)
            {
                currentHealth -= damage;
                if (currentHealth <= 0)
                {
                    currentHealth = 0;
                    Die();  // Player death
                }
                UpdateHealthUI();
            }
        }

        private void PlayHitFeedback(int damage)
        {
            // Play hit sound and spawn screen shake effect
            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);

            Instantiate(ScreenShakePrefab, transform.position, Quaternion.identity);

        }

        private void ShowHitIndicator(string direction, int damage)
        {
            // Set hit indicator UI color and visibility based on direction and damage severity
            Color severityColor = GetDamageColor(damage);

            switch (direction)
            {
                case "left":
                    SetAlpha(hitLeft, 1f);
                    hitLeft.color = severityColor;
                    break;
                case "right":
                    SetAlpha(hitRight, 1f);
                    hitRight.color = severityColor;
                    break;
                case "top":
                    SetAlpha(hitTop, 1f);
                    hitTop.color = severityColor;
                    break;
                case "bottom":
                    SetAlpha(hitBottom, 1f);
                    hitBottom.color = severityColor;
                    break;
            }
        }

        private Color GetDamageColor(int damage)
        {
            // Returns color intensity for hit indicators based on damage amount
            float intensity = 1f; // full red for low damage

            if (damage >= 50)
                intensity = 0.3f; // dark red for high damage
            else if (damage >= 20)
                intensity = 0.6f; // medium red for mid damage

            return new Color(intensity, 0f, 0f, 1f);
        }

        private void FadeOutHitIndicators()
        {
            // Gradually reduce alpha of hit indicator UI elements to fade them out
            FadeOut(hitLeft);
            FadeOut(hitRight);
            FadeOut(hitTop);
            FadeOut(hitBottom);
        }

        private void SetAlpha(Image img, float alpha)
        {
            // Helper to set the alpha transparency of an Image UI element
            if (img == null) return;
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }

        private void FadeOut(Image img)
        {
            // Decrease alpha over time for fade effect
            if (img == null || img.color.a <= 0f) return;
            Color c = img.color;
            c.a -= fadeSpeed * Time.deltaTime;
            img.color = c;
        }

        public void Heal(int amount)
        {
            // Heal the player by a certain amount, up to max health
            if (amount <= 0) return;
            currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
            UpdateHealthUI();
        }

        public void AddArmor(int amount)
        {
            // Add armor points, up to max armor
            if (amount <= 0) return;
            currentArmor = Mathf.Min(currentArmor + amount, maxArmor);
            UpdateArmorUI();
        }

        private void Die()
        {
            // Called when player health reaches zero
            Debug.Log("Player Died");
            if (GameManager.Instance != null)
                GameManager.Instance.OnPlayerDeath();
            Destroy(gameObject);
        }

        private void UpdateHealthUI()
        {
            // Update health slider and text UI to current values
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }

            if (healthText != null)
                healthText.text = $"{currentHealth} / {maxHealth}";
        }

        private void UpdateArmorUI()
        {
            // Update armor slider and text UI to current values
            if (armorSlider != null)
            {
                armorSlider.maxValue = maxArmor;
                armorSlider.value = currentArmor;
            }

            if (armorText != null)
                armorText.text = $"{currentArmor} / {maxArmor}";
        }
    }

}

using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem;

namespace AlignedGames
{
    public class PlayerGrenadeManager : MonoBehaviour
    {
        [Header("Grenade Settings")]
        public int grenadesLeft; // Current number of grenades the player has
        public int MaxgrenadesLeft; // Maximum grenades player can carry
        public GameObject grenadePrefab; // Prefab for the grenade to instantiate
        public Transform throwPoint; // Position from which grenades are thrown
        public float throwForce = 1f; // Multiplier for throw strength
        public float throwlifetime; // Time before grenade explodes
        public TextMeshProUGUI GrenadesText; // UI text to show grenade count

        [Header("Input Actions")]
        public InputAction throwAction; // Input action for throwing grenade
        public InputAction aimAction; // (Unused here but enabled)

        [Header("Pickup Settings")]
        public PickupIdentifier nearbyPickup; // Reference to nearby pickup grenade
        [SerializeField] private Key pickupKey = Key.E; // Key to pick up grenades
        [SerializeField] private TextMeshProUGUI pickupText; // UI text for pickup prompt
        [SerializeField] private AudioSource audioSource; // Audio source for pickup sounds
        [SerializeField] private AudioClip[] pickupSounds; // Array of pickup sounds

        public float maxThrowDistance = 12f; // Max distance grenade can be thrown

        private void OnEnable()
        {
            throwAction.Enable(); // Enable input actions when script active
            aimAction.Enable();
        }

        private void OnDisable()
        {
            throwAction.Disable(); // Disable input actions when script inactive
            aimAction.Disable();
        }

        private void Update()
        {
            HandleGrenadePickup(); // Check and handle grenade pickup input

            // Update grenade count UI
            GrenadesText.text = "Grenades : " + grenadesLeft.ToString("0") + " / " + MaxgrenadesLeft.ToString("0");

            // Clamp grenade count so it never exceeds max
            if (grenadesLeft > MaxgrenadesLeft)
            {
                grenadesLeft = MaxgrenadesLeft;
            }

            // If throw input pressed and grenades available, throw one
            if (throwAction.WasPressedThisFrame() && grenadesLeft > 0)
            {
                ThrowGrenade();
            }
        }

        private void HandleGrenadePickup()
        {
            // If pickup key pressed and player is near a grenade pickup
            if (Keyboard.current != null && Keyboard.current[pickupKey].wasPressedThisFrame && nearbyPickup != null)
            {
                // Check if pickup type is grenade
                if (nearbyPickup.pickupType == PickupIdentifier.PickupItemType.Grenade)
                {
                    PickupGrenades(nearbyPickup.grenadesToRestore); // Add grenades

                    // Play random pickup sound if available
                    if (pickupSounds.Length > 0 && audioSource != null)
                    {
                        AudioClip randomSound = pickupSounds[Random.Range(0, pickupSounds.Length)];
                        audioSource.PlayOneShot(randomSound);
                    }

                    Destroy(nearbyPickup.gameObject); // Remove pickup from scene
                    nearbyPickup = null; // Clear reference

                    if (pickupText != null) pickupText.gameObject.SetActive(false); // Hide pickup UI
                }
            }
        }

        private void ThrowGrenade()
        {
            // Instantiate grenade prefab at throw point
            GameObject grenade = Instantiate(grenadePrefab, throwPoint.position, Quaternion.identity);
            Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();

            // Get mouse position in world space
            Vector3 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f; // Ensure 2D plane

            // Calculate direction and distance to mouse from throw point
            Vector2 toMouse = mouseWorldPos - throwPoint.position;

            // Clamp max throw distance
            if (toMouse.magnitude > maxThrowDistance)
                toMouse = toMouse.normalized * maxThrowDistance;

            // Apply throw force scaled by distance and throwForce multiplier
            rb.AddForce(toMouse.normalized * toMouse.magnitude * throwForce, ForceMode2D.Impulse);

            // Set grenade explode delay slightly less than lifetime
            rb.GetComponent<GrenadeBehaviour>().explodeDelay = throwlifetime - 0.25f;

            // Start coroutine to reduce grenade velocity over time
            StartCoroutine(ReduceGrenadeForce(rb, throwlifetime));

            grenadesLeft--; // Reduce grenade count by one
        }

        private IEnumerator ReduceGrenadeForce(Rigidbody2D rb, float throwlifetime)
        {
            float halfTime = throwlifetime * 0.5f;
            float elapsed = 0f;

            while (elapsed < throwlifetime)
            {
                elapsed += Time.deltaTime;

                // During first half of lifetime, keep velocity as is
                if (elapsed < halfTime)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * rb.linearVelocity.magnitude;
                }
                // During second half, reduce velocity by half
                else if (elapsed < throwlifetime)
                {
                    rb.linearVelocity = rb.linearVelocity.normalized * (rb.linearVelocity.magnitude * 0.5f);
                }

                // Near end of lifetime, stop movement and destroy Rigidbody2D component
                if (elapsed >= throwlifetime - 0.25f)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    Destroy(rb);
                }

                yield return null;
            }
        }

        // Increase grenades left by given amount and clamp to max
        public void PickupGrenades(int amount)
        {
            if (amount <= 0) return;
            grenadesLeft += amount;
            if (grenadesLeft > MaxgrenadesLeft) grenadesLeft = MaxgrenadesLeft;
        }

        // Detect entering pickup trigger
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Pickup"))
            {
                PickupIdentifier pickup = other.GetComponent<PickupIdentifier>();
                if (pickup != null && pickup.pickupType == PickupIdentifier.PickupItemType.Grenade)
                {
                    nearbyPickup = pickup;
                    if (pickupText != null)
                    {
                        pickupText.gameObject.SetActive(true);
                        pickupText.text = $"Press [{pickupKey}] to pick up Grenades";
                    }
                }
            }
        }

        // Detect leaving pickup trigger
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Pickup") && other.GetComponent<PickupIdentifier>() == nearbyPickup)
            {
                nearbyPickup = null;
                if (pickupText != null) pickupText.gameObject.SetActive(false);
            }
        }
    }
}

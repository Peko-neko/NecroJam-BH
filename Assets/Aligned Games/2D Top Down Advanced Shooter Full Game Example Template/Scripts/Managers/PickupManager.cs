using UnityEngine;

namespace AlignedGames
{

    public class PickupManager : MonoBehaviour
    {
        // Reference to the Collider2D component on this pickup object
        private Collider2D pickupCollider;

        // Flag to check if the pickup can currently be collected by the player
        private bool canBePickedUp = false;

        private void Awake()
        {
            // Get the Collider2D component attached to the same GameObject
            pickupCollider = GetComponent<Collider2D>();

            // Check if the collider exists; if not, log an error for debugging
            if (pickupCollider == null)
            {
                Debug.LogError("No Collider2D found on the GunPickupIdentifier. Ensure it has a collider.");
            }
        }

        private void Start()
        {
            // Delay enabling pickup so the player can't pick it up immediately after spawn
            Invoke(nameof(EnablePickup), 0.5f);
        }

        private void EnablePickup()
        {
            // Allow the pickup to be collected and enable its collider so it can detect collisions
            canBePickedUp = true;

            if (pickupCollider != null)
            {
                pickupCollider.enabled = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Only respond if the pickup is enabled for collection
            if (!canBePickedUp) return;

            // Check if the colliding object has the "Player" tag
            if (other.CompareTag("Player"))
            {
                // Pickup logic was previously here but is now removed (Its in the weapon Manager)
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // Detect when player leaves the pickup trigger area

            if (other.CompareTag("Player"))
            {
                // Pickup logic was previously here but is now removed (Its in the weapon Manager)
            }
        }

        public void DisablePickupTemporarily(float duration)
        {
            // Temporarily disable the pickup so it cannot be collected
            canBePickedUp = false;

            // Disable the collider to prevent collision detection
            if (pickupCollider != null)
            {
                pickupCollider.enabled = false;
            }

            // Re-enable pickup after a delay specified by 'duration'
            Invoke(nameof(EnablePickup), duration);
        }
    }
}

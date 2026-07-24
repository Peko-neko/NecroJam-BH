using UnityEngine;
using UnityEngine.InputSystem;

namespace AlignedGames
{
    public class PlayerMovementManager : MonoBehaviour
    {
        [Header("Movement Settings")]
        // Speeds for walking, running, crawling
        public float walkSpeed = 3f;
        public float runSpeed = 6f;
        public float crawlSpeed = 1.5f;

        // Movement input value (from player)
        private Vector2 movementInput;

        // Rigidbody2D component for movement
        private Rigidbody2D rb;

        // Input system references
        public InputActionAsset inputActions;
        private InputAction moveAction;
        private InputAction runAction;
        private InputAction crawlAction;

        [Header("Animation Settings")]
        // Reference to player Animator
        public Animator animator;

        // Used to check which weapon is active
        private PlayerCombatManager combatManager;

        // Called when the object is initialized
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            // Get input actions from Player action map
            var playerActionMap = inputActions.FindActionMap("Player");
            moveAction = playerActionMap.FindAction("Move");
            runAction = playerActionMap.FindAction("Sprint");
            crawlAction = playerActionMap.FindAction("Crawl");

            // Enable input actions
            moveAction.Enable();
            runAction.Enable();
            crawlAction.Enable();

            // Get reference to combat manager
            combatManager = GetComponent<PlayerCombatManager>();
            if (combatManager == null)
                Debug.LogError("PlayerCombatManager not found on the same GameObject.");
        }

        // Re-enable inputs if object is enabled
        private void OnEnable()
        {
            moveAction.Enable();
            runAction.Enable();
            crawlAction.Enable();
        }

        // Disable inputs when object is disabled
        private void OnDisable()
        {
            moveAction.Disable();
            runAction.Disable();
            crawlAction.Disable();
        }

        // Handle input and animation every frame
        private void Update()
        {
            HandleMovementInput();
            HandleAnimations();
        }

        // Move player using physics
        private void FixedUpdate()
        {
            MovePlayer();
        }

        // Reads player movement input
        private void HandleMovementInput()
        {
            movementInput = moveAction.ReadValue<Vector2>();
        }

        // Moves the player using Rigidbody2D
        private void MovePlayer()
        {
            float speed = walkSpeed;

            // Check if run or crawl inputs are held
            if (runAction.IsPressed())
                speed = runSpeed;
            else if (crawlAction.IsPressed())
                speed = crawlSpeed;

            // Calculate new position
            Vector2 targetPosition = rb.position + movementInput.normalized * speed * Time.fixedDeltaTime;

            // Move player with collision support
            rb.MovePosition(targetPosition);
        }

        // Updates animation states based on movement and weapon
        private void HandleAnimations()
        {
            bool isMoving = movementInput.sqrMagnitude >= 0.1f;

            // Get current weapon type from combat manager
            PlayerCombatManager.WeaponType currentWeapon = combatManager.GetActiveWeaponType();

            // Reset all animator states
            animator.SetBool("isPistolIdle", false);
            animator.SetBool("isRifleIdle", false);
            animator.SetBool("isMovingPistol", false);
            animator.SetBool("isMovingRifle", false);

            // Set appropriate animation based on movement and weapon
            if (isMoving)
            {
                if (currentWeapon == PlayerCombatManager.WeaponType.Pistol)
                {
                    animator.SetBool("isMovingPistol", true);
                }
                else
                {
                    animator.SetBool("isMovingRifle", true);
                }
            }
            else
            {
                if (currentWeapon == PlayerCombatManager.WeaponType.Pistol)
                {
                    animator.SetBool("isPistolIdle", true);
                }
                else
                {
                    animator.SetBool("isRifleIdle", true);
                }
            }
        }

        // Uncomment and extend this section to add more weapon animations
        // else if (currentWeapon == PlayerCombatManager.WeaponType.Shotgun)
        // {
        //     animator.Play("MovingShotgun");
        // }
    }
}

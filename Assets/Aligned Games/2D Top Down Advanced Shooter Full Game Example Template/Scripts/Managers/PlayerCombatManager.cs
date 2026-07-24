using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Collections; // Added for potential future Coroutines if needed

namespace AlignedGames
{

    public class PlayerCombatManager : MonoBehaviour
    {
        [System.Serializable]
        public enum WeaponType
        {
            Pistol,
            SubMachineGun,
            MachineGun,
            BurstMachineGun,
            Shotgun,
            Rifle,
            SniperRifle,
            RocketLauncher
            // Make sure this order matches AmmoType below if casting
        }

        [System.Serializable]
        public enum AmmoType
        {
            PistolAmmo,
            SubMachineGunAmmo,
            MachineGunAmmo,
            BurstMachineGunAmmo,
            ShotgunAmmo,
            RifleAmmo,
            SniperRifleAmmo,
            RocketLauncherAmmo
            // Make sure this order matches WeaponType above if casting
        }


        [System.Serializable]
        public struct Weapon
        {
            public WeaponType Weapontype;
            public bool isActive;
            public float bulletSpread;
            public float fireRate;
            public float damage;
            public float bulletSpeed;
            public float reloadTime;
            public int bulletsPerShot;
            public GameObject bulletPrefab;
            public int magazineSize;
        }

        // Removed the unused Ammo struct

        [Header("Weapon Slots")]
        public GameObject defaultBulletPrefab; // Consider if still needed with Resource loading
        public TextMeshProUGUI weaponNameText;
        public TextMeshProUGUI pickupText; // Reference to the "Press 'E' to Pickup" text
        public GameObject currentMainWeapon; // Stores the *instance* of the main weapon prefab
        public GameObject currentPistol;     // Stores the *instance* of the pistol prefab
        public Weapon activeWeapon;          // Holds the stats of the currently equipped weapon
        private float nextFireTime;

        [Header("Projectile Sprites")]
        public Sprite[] projectileSprites; // Array of sprites for each weapon type

        [Header("Ammo Management")]
        public int currentMagazineSize; // Bullets currently in the mag
        public int currentMagsLeft;     // Number of full magazines available
                                        // Stores (current bullets in mag, remaining full mags) for each weapon type ever held
        private Dictionary<WeaponType, (int magazineSize, int magsLeft)> weaponAmmo = new Dictionary<WeaponType, (int magazineSize, int magsLeft)>();

        [Header("State Flags")]
        public bool isReloading = false;
        private bool canSwitchWeapon = true;
        private bool isPickingUp = false; // Combined pickup flag
        private bool isDroppingWeapon = false;
        private PickupIdentifier nearbyPickup; // Store the nearby pickup script

        [Header("Input Actions")]
        public InputAction fireAction;
        public InputAction reloadAction;
        public InputAction switchWeapon1Action;
        public InputAction switchWeapon2Action;
        public InputAction pickupAction;
        public InputAction scrollWeaponAction;
        public InputAction aimAction;

        [Header("Misc Settings")]
        public float DropRange = 1.5f; // Range for dropping weapon
        public string defaultPistolResourcePath = "Pickups/PistolPickup"; // Make path configurable

        [Header("Weapon System")]
        [SerializeField] private Vector3[] shootPoints;      // Array of shoot point offsets

        public GameObject muzzleFlash; // Active muzzle flash
        public Transform firePoint;          // Actual shoot point transform (instantiated)

        [SerializeField] private Sprite[] muzzleFlashTextures;        // Sprites for each weapon type

        public GameObject ScreenShake;

        [SerializeField] private Sprite[] selectedWeaponSprites; // Sprites for each weapon type (held in hand)
        [SerializeField] private SpriteRenderer heldWeaponRenderer; // The SpriteRenderer for the held weapon

        private void Awake()
        {
            if (pickupText != null)
            {
                pickupText.gameObject.SetActive(false); // Ensure the text is initially hidden
            }
            else
            {
                Debug.LogError("Pickup TextMeshProUGUI not assigned in PlayerCombatManager!");
            }

            // Enable all actions
            fireAction.Enable();
            reloadAction.Enable();
            switchWeapon1Action.Enable();
            switchWeapon2Action.Enable();
            pickupAction.Enable();
            scrollWeaponAction.Enable();
            aimAction.Enable();
        }

        private void OnEnable()
        {
            // Just ensure they are enabled if the object gets re-enabled
            if (!fireAction.enabled) fireAction.Enable();
            if (!reloadAction.enabled) reloadAction.Enable();
            if (!switchWeapon1Action.enabled) switchWeapon1Action.Enable();
            if (!switchWeapon2Action.enabled) switchWeapon2Action.Enable();
            if (!pickupAction.enabled) pickupAction.Enable();
            if (!scrollWeaponAction.enabled) scrollWeaponAction.Enable();
            if (!aimAction.enabled) aimAction.Enable();
        }

        private void OnDisable()
        {
            // Disable the input actions when component/object is disabled
            fireAction.Disable();
            reloadAction.Disable();
            switchWeapon1Action.Disable();
            switchWeapon2Action.Disable();
            pickupAction.Disable();
            scrollWeaponAction.Disable();
            aimAction.Disable();
        }

        private void Start()
        {
            InitializeWeapons();
            UpdateWeaponDisplay(); // Initial display update
        }

        private void InitializeWeapons()
        {
            // --- Initialize Pistol ---
            GameObject defaultPistolPrefab = Resources.Load<GameObject>(defaultPistolResourcePath);
            if (defaultPistolPrefab != null)
            {
                // Instantiate the default pistol PREFAB (not the pickup itself)
                // This instance will be stored but not visible in the scene until dropped.
                currentPistol = Instantiate(defaultPistolPrefab, transform.position, Quaternion.identity);
                currentPistol.SetActive(false); // Keep the stored instance inactive

                PickupIdentifier pistolPickupData = currentPistol.GetComponent<PickupIdentifier>();
                if (pistolPickupData != null && pistolPickupData.pickupType == PickupIdentifier.PickupItemType.Weapon)
                {
                    // Create the Weapon struct data from the prefab's identifier
                    Weapon initialPistolWeapon = new Weapon
                    {
                        Weapontype = pistolPickupData.Weapontype,
                        isActive = false, // Will be set true by EquipPistol
                        bulletSpread = pistolPickupData.bulletSpread,
                        fireRate = pistolPickupData.fireRate,
                        damage = pistolPickupData.damage,
                        bulletSpeed = pistolPickupData.bulletSpeed,
                        reloadTime = pistolPickupData.reloadTime,
                        bulletsPerShot = pistolPickupData.bulletsPerShot,
                        bulletPrefab = pistolPickupData.bulletPrefab ?? defaultBulletPrefab, // Fallback prefab
                        magazineSize = pistolPickupData.magazineSize
                    };

                    // Initialize ammo (full mag + 3 spares) and store it
                    int initialMagSize = initialPistolWeapon.magazineSize;
                    int initialMagsLeft = 3;
                    weaponAmmo[initialPistolWeapon.Weapontype] = (initialMagSize, initialMagsLeft);

                    // Equip the pistol immediately
                    EquipWeaponFromData(initialPistolWeapon, initialMagSize, initialMagsLeft);
                }
                else
                {
                    Debug.LogError($"Default Pistol Prefab at '{defaultPistolResourcePath}' is missing PickupIdentifier or is not type Weapon!");
                    // Optional: Destroy the invalid instance
                    if (currentPistol) Destroy(currentPistol);
                    currentPistol = null; // Prevent further errors
                                          // Consider fallback or stopping game execution here
                                          // Application.Quit(); // Uncomment if critical
                                          // return;
                }
            }
            else
            {
                Debug.LogError($"Default Pistol Prefab not found at '{defaultPistolResourcePath}'. Please ensure it exists in a Resources folder.");
                // Consider fallback or stopping game execution here
                // Application.Quit(); // Uncomment if critical
                // return;
            }

            // --- Initialize Main Weapon (if one is pre-assigned) ---
            // This part assumes you might assign a starting main weapon in the inspector
            if (currentMainWeapon != null)
            {
                // If pre-assigned, it should already be an instance. We need its stats.
                PickupIdentifier mainWeaponPickupData = currentMainWeapon.GetComponent<PickupIdentifier>();
                if (mainWeaponPickupData != null && mainWeaponPickupData.pickupType == PickupIdentifier.PickupItemType.Weapon)
                {
                    Weapon initialMainWeapon = new Weapon { /* ... fill from mainWeaponPickupData ... */ };
                    // Initialize ammo if not already in dictionary
                    if (!weaponAmmo.ContainsKey(initialMainWeapon.Weapontype))
                    {
                        int initialMagSize = initialMainWeapon.magazineSize;
                        int initialMagsLeft = 3; // Default starting ammo
                        weaponAmmo[initialMainWeapon.Weapontype] = (initialMagSize, initialMagsLeft);
                    }
                    // Don't auto-equip here, let the player switch or keep pistol active
                    currentMainWeapon.SetActive(false); // Ensure it's inactive initially
                }
                else
                {
                    Debug.LogWarning("Assigned currentMainWeapon is missing PickupIdentifier or is not type Weapon. It will be ignored until replaced.");
                    // Optional: Destroy the invalid instance
                    // if(currentMainWeapon) Destroy(currentMainWeapon);
                    currentMainWeapon = null;
                }
            }
        }


        private void Update()
        {
            // Don't aim/shoot/reload etc. if picking up/dropping
            if (isPickingUp || isDroppingWeapon) return;

            AimWeapon();
            HandleShooting();
            HandleWeaponSwitching();
            HandleReloading();
            HandlePickupInput(); // Check for pickup action
        }

        // --- Core Action Handling ---

        private void AimWeapon()
        {
            // Get mouse position in world space
            Vector2 screenPos = aimAction.ReadValue<Vector2>();
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane + 10f)); // Adjust Z as needed

            Vector2 aimDirection = ((Vector2)worldPos - (Vector2)transform.position).normalized;

            if (aimDirection.sqrMagnitude > 0.01f) // Avoid aiming at self
            {
                float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
                // Apply rotation - consider smoothing (Slerp) for visual appeal
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void HandleShooting()
        {
            // Use IsPressed for continuous fire, WasPressedThisFrame for semi-auto (adjust fireRate accordingly)
            if (fireAction.IsPressed() && activeWeapon.isActive && Time.time >= nextFireTime && !isReloading)
            {
                if (currentMagazineSize <= 0)
                {
                    StartReload(); // Auto-reload on empty click
                }
                else
                {
                    Shoot();
                }
            }
        }

        private void HandleReloading()
        {
            // Use WasPressedThisFrame to avoid starting reload multiple times if held
            if (reloadAction.WasPressedThisFrame() && !isReloading && currentMagazineSize < activeWeapon.magazineSize && currentMagsLeft > 0)
            {
                StartReload();
            }

        }

        private void HandleWeaponSwitching()
        {
            if (!canSwitchWeapon || isReloading) return; // Cannot switch while reloading

            // --- Direct Slot Switching ---
            if (switchWeapon1Action.WasPressedThisFrame())
            {
                // Save current weapon's ammo state before switching
                SaveCurrentWeaponAmmo();
                EquipPistol();
            }
            else if (switchWeapon2Action.WasPressedThisFrame())
            {
                // Save current weapon's ammo state before switching
                SaveCurrentWeaponAmmo();
                EquipMainWeapon();
            }

            // --- Scroll Wheel Switching ---
            float scroll = scrollWeaponAction.ReadValue<float>();
            if (Mathf.Abs(scroll) > 0.1f) // Check for significant scroll input
            {
                // Save current weapon's ammo state before switching
                SaveCurrentWeaponAmmo();
                if (scroll > 0f)
                {
                    EquipNextWeapon();
                }
                else // scroll < 0f
                {
                    EquipPreviousWeapon();
                }
            }
        }

        private void HandlePickupInput()
        {
            // Ensure player is near a pickup and presses the button, and isn't doing something else
            if (pickupAction.WasPressedThisFrame() && nearbyPickup != null && !isReloading && !isShooting())
            {

                GetComponent<AudioManager>().PlayPickupSound();

                isPickingUp = true; // Prevent other actions during pickup process

                PickupIdentifier pickupToProcess = nearbyPickup; // Copy reference
                nearbyPickup = null; // Clear the nearby reference immediately
                if (pickupText != null) pickupText.gameObject.SetActive(false); // Hide text

                // --- Determine Pickup Type ---
                if (pickupToProcess.pickupType == PickupIdentifier.PickupItemType.Weapon)
                {
                    PickupWeapon(pickupToProcess);
                }
                else // Must be Ammo
                {
                    PickupAmmo(pickupToProcess);
                }

                // Destroy the pickup object from the scene *after* processing it
                if (pickupToProcess != null) // Check if it wasn't already destroyed (e.g., inside PickupWeapon)
                {
                    Destroy(pickupToProcess.gameObject);
                }

                isPickingUp = false; // Allow actions again
            }
        }


        // --- Weapon & Ammo Logic ---

        private void Shoot()
        {
            if (currentMagazineSize <= 0 || !activeWeapon.isActive) return; // Extra safety checks

            // Set cooldown for next shot
            nextFireTime = Time.time + activeWeapon.fireRate;

            // Decrement ammo *before* firing (standard practice)
            currentMagazineSize -= activeWeapon.bulletsPerShot;
            if (currentMagazineSize < 0) currentMagazineSize = 0; // Clamp to prevent negative ammo

            // Instantiate and fire projectiles
            for (int i = 0; i < activeWeapon.bulletsPerShot; i++)
            {
                // Calculate spread
                float spreadAngle = Random.Range(-activeWeapon.bulletSpread, activeWeapon.bulletSpread);
                Quaternion spreadRotation = Quaternion.Euler(0, 0, spreadAngle);
                Quaternion finalRotation = firePoint.rotation * spreadRotation;

                // Instantiate bullet
                GameObject bulletPrefabToUse = activeWeapon.bulletPrefab ?? defaultBulletPrefab; // Use specific or default
                if (bulletPrefabToUse == null)
                {
                    Debug.LogError($"Cannot fire {activeWeapon.Weapontype}: Bullet Prefab is null!");
                    continue;
                }
                GameObject bullet = Instantiate(bulletPrefabToUse, firePoint.position, finalRotation);

                // Apply velocity
                Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = bullet.transform.right * activeWeapon.bulletSpeed;
                }
                else
                {
                    Debug.LogWarning($"Bullet prefab '{bullet.name}' is missing Rigidbody2D component.");
                }

                // Set damage (if projectile script exists)
                if (bullet.TryGetComponent(out ProjectileBehaviour projectileScript))
                {
                    projectileScript.SetDamage((int)activeWeapon.damage);
                }

                // Set sprite based on weapon type
                SpriteRenderer bulletSpriteRenderer = bullet.GetComponentInChildren<SpriteRenderer>();
                if (bulletSpriteRenderer != null && projectileSprites != null && projectileSprites.Length > (int)activeWeapon.Weapontype)
                {
                    bulletSpriteRenderer.sprite = projectileSprites[(int)activeWeapon.Weapontype];
                }
                else
                {
                    Debug.LogWarning("Projectile sprite could not be set. Check the projectileSprites array.");
                }
            }

            // Update UI
            UpdateWeaponDisplay();

            Instantiate(ScreenShake, firePoint.position, Quaternion.identity);
            Instantiate(muzzleFlash, new Vector3(firePoint.position.x, firePoint.position.y, firePoint.position.z), transform.rotation);
            GetComponent<AudioManager>().PlayShootSound(activeWeapon.Weapontype);

        }

        private void StartReload()
        {

            if (isReloading || currentMagazineSize == activeWeapon.magazineSize || currentMagsLeft < 1 || !activeWeapon.isActive)
                return; // Can't reload if already reloading, full, no mags left, or no active weapon

            GetComponent<AudioManager>().PlayReloadSound(activeWeapon.Weapontype);

            isReloading = true;
            canSwitchWeapon = false; // Prevent switching during reload animation/timer

            // Save current ammo state just before reload starts (important if reload is interrupted)
            SaveCurrentWeaponAmmo(true); // Pass true to indicate reload started

            // Optional: Play reload start sound/animation
            Debug.Log($"Starting reload for {activeWeapon.Weapontype}...");

            // Update UI immediately to show "Reloading..." or 0 ammo
            UpdateWeaponDisplay(true); // Pass flag to show reloading state

            // Use Invoke or preferably a Coroutine for the delay
            Invoke(nameof(FinishReload), activeWeapon.reloadTime);
        }

        private void FinishReload()
        {
            if (!isReloading) return; // Safety check

            if (currentMagsLeft > 0)
            {
                currentMagazineSize = activeWeapon.magazineSize; // Refill the magazine completely
                currentMagsLeft--;                            // Use up one spare magazine
            }
            // If currentMagsLeft was 0, magazine remains empty (or whatever value it had before StartReload if you don't clear it)

            isReloading = false;
            canSwitchWeapon = true; // Allow switching again
            nextFireTime = Time.time; // Allow firing immediately after reload

            // Update the stored ammo count for this weapon type
            SaveCurrentWeaponAmmo();

            // Update UI
            UpdateWeaponDisplay();
            Debug.Log($"Reload finished for {activeWeapon.Weapontype}. Mags left: {currentMagsLeft}");
            // Optional: Play reload end sound/animation
        }

        // --- Weapon Switching Implementation ---

        private void EquipPistol()
        {
            if (currentPistol == null)
            {
                Debug.Log("No pistol available to equip.");
                return;
            }
            // Check if already equipping pistol to prevent redundant calls
            if (activeWeapon.isActive && activeWeapon.Weapontype == WeaponType.Pistol) return;


            PickupIdentifier pickupData = currentPistol.GetComponent<PickupIdentifier>();
            if (pickupData && pickupData.pickupType == PickupIdentifier.PickupItemType.Weapon)
            {
                Weapon pistolWeaponData = new Weapon
                { /* ... fill from pickupData ... */
                    Weapontype = pickupData.Weapontype,
                    isActive = true,
                    bulletSpread = pickupData.bulletSpread,
                    fireRate = pickupData.fireRate,
                    damage = pickupData.damage,
                    bulletSpeed = pickupData.bulletSpeed,
                    reloadTime = pickupData.reloadTime,
                    bulletsPerShot = pickupData.bulletsPerShot,
                    bulletPrefab = pickupData.bulletPrefab ?? defaultBulletPrefab,
                    magazineSize = pickupData.magazineSize
                };

                // Retrieve stored ammo
                (int magSize, int magsLeft) ammo = (pistolWeaponData.magazineSize, 0); // Default if not found
                if (weaponAmmo.TryGetValue(pistolWeaponData.Weapontype, out var storedAmmo))
                {
                    ammo = storedAmmo;
                }
                else
                {
                    // Should have been initialized in Start, but safety check
                    Debug.LogWarning($"Ammo data for {pistolWeaponData.Weapontype} not found, initializing.");
                    weaponAmmo[pistolWeaponData.Weapontype] = ammo;
                }


                EquipWeaponFromData(pistolWeaponData, ammo.magSize, ammo.magsLeft);
            }
            else
            {
                Debug.LogError("Stored Pistol instance is invalid (missing PickupIdentifier or not Weapon type).");
            }
        }

        private void EquipMainWeapon()
        {
            if (currentMainWeapon == null)
            {
                Debug.Log("No main weapon available to equip.");
                // Optionally switch back to pistol if available
                // if (currentPistol != null && activeWeapon.Weapontype != WeaponType.Pistol) EquipPistol();
                return;
            }
            // Check if already equipping this weapon type to prevent redundant calls
            PickupIdentifier currentMainPickupData = currentMainWeapon.GetComponent<PickupIdentifier>();
            if (currentMainPickupData && activeWeapon.isActive && activeWeapon.Weapontype == currentMainPickupData.Weapontype) return;


            PickupIdentifier pickupData = currentMainWeapon.GetComponent<PickupIdentifier>();
            if (pickupData && pickupData.pickupType == PickupIdentifier.PickupItemType.Weapon)
            {
                Weapon mainWeaponData = new Weapon
                { /* ... fill from pickupData ... */
                    Weapontype = pickupData.Weapontype,
                    isActive = true,
                    bulletSpread = pickupData.bulletSpread,
                    fireRate = pickupData.fireRate,
                    damage = pickupData.damage,
                    bulletSpeed = pickupData.bulletSpeed,
                    reloadTime = pickupData.reloadTime,
                    bulletsPerShot = pickupData.bulletsPerShot,
                    bulletPrefab = pickupData.bulletPrefab ?? defaultBulletPrefab,
                    magazineSize = pickupData.magazineSize
                };

                // Retrieve stored ammo
                (int magSize, int magsLeft) ammo = (mainWeaponData.magazineSize, 0); // Default if not found
                if (weaponAmmo.TryGetValue(mainWeaponData.Weapontype, out var storedAmmo))
                {
                    ammo = storedAmmo;
                }
                else
                {
                    // This weapon must have been picked up, so ammo should exist. Log warning if not.
                    Debug.LogWarning($"Ammo data for {mainWeaponData.Weapontype} not found, initializing.");
                    weaponAmmo[mainWeaponData.Weapontype] = ammo;
                }

                EquipWeaponFromData(mainWeaponData, ammo.magSize, ammo.magsLeft);
            }
            else
            {
                Debug.LogError("Stored Main Weapon instance is invalid (missing PickupIdentifier or not Weapon type).");
            }
        }

        // Helper to consolidate equipping logic
        private void EquipWeaponFromData(Weapon weaponData, int magSize, int magsLeft)
        {
            activeWeapon = weaponData; // Set the active weapon stats
            activeWeapon.isActive = true;

            currentMagazineSize = magSize; // Load ammo counts
            currentMagsLeft = magsLeft;

            nextFireTime = Time.time; // Reset fire cooldown
            isReloading = false;      // Ensure not stuck in reload state
            CancelInvoke(nameof(FinishReload)); // Cancel any pending reload

            // Set the shoot point position for the equipped weapon
            int weaponIndex = (int)activeWeapon.Weapontype;

            if (weaponIndex < shootPoints.Length && weaponIndex < muzzleFlashTextures.Length)
            {
                // Set the shoot point position
                firePoint.localPosition = shootPoints[weaponIndex];

                // Change the muzzle flash sprite
                if (muzzleFlash != null)
                {
                    SpriteRenderer spriteRenderer = muzzleFlash.GetComponentInChildren<SpriteRenderer>();
                    if (spriteRenderer != null)
                        spriteRenderer.sprite = muzzleFlashTextures[weaponIndex];
                }
            }
            else
            {
                Debug.LogWarning($"Shoot point or muzzle flash texture not defined for {activeWeapon.Weapontype}");
            }

            // Set the held weapon sprite
            if (weaponIndex < selectedWeaponSprites.Length && heldWeaponRenderer != null)
            {
                heldWeaponRenderer.sprite = selectedWeaponSprites[weaponIndex];
            }
            else
            {
                Debug.LogWarning($"Held weapon sprite not defined for {activeWeapon.Weapontype}");
            }


            UpdateWeaponDisplay();    // Update UI
                                      // Optional: Play weapon switch sound/animation
            Debug.Log($"Equipped {activeWeapon.Weapontype}. Ammo: {currentMagazineSize}/{currentMagsLeft}");
        }


        private void EquipNextWeapon()
        {
            // Simple toggle between pistol and main (if main exists)
            if (activeWeapon.isActive && activeWeapon.Weapontype == WeaponType.Pistol && currentMainWeapon != null)
            {
                EquipMainWeapon();
            }
            else // If main is active, or no main exists, switch to pistol
            {
                EquipPistol();
            }
        }

        private void EquipPreviousWeapon()
        {
            // Same logic as EquipNext for a 2-weapon system
            EquipNextWeapon();
        }

        // --- Pickup Handling ---

        // Renamed for clarity - handles WEAPON pickups
        public void PickupWeapon(PickupIdentifier weaponPickup)
        {
            if (weaponPickup == null || weaponPickup.pickupType != PickupIdentifier.PickupItemType.Weapon)
            {
                Debug.LogError("PickupWeapon called with invalid PickupIdentifier.");
                return;
            }

            WeaponType incomingWeaponType = weaponPickup.Weapontype;

            // --- Handle Pistol Slot ---
            if (incomingWeaponType == WeaponType.Pistol)
            {
                // Check if player already has a pistol
                if (currentPistol != null)
                {
                    PickupIdentifier currentPistolData = currentPistol.GetComponent<PickupIdentifier>();
                    // If it's the *same type* of pistol (e.g. basic vs heavy), just add ammo
                    if (currentPistolData && currentPistolData.Weapontype == incomingWeaponType)
                    {
                        AddAmmoForWeapon(incomingWeaponType, weaponPickup.magazineSize, 3); // Add 3 mags for same weapon pickup
                        Debug.Log($"Picked up duplicate {incomingWeaponType}, added ammo.");
                        // Destroy(weaponPickup.gameObject); // Handled in HandlePickupInput
                    }
                    else
                    {
                        // Different pistol type or invalid current pistol, swap it
                        SaveCurrentWeaponAmmo(); // Save ammo if pistol was active
                        DropWeapon(ref currentPistol); // Drop the old one
                        ReplaceWeaponInstance(ref currentPistol, weaponPickup); // Replace with the new one
                    }
                }
                else
                {
                    // No pistol currently held, just take the new one
                    ReplaceWeaponInstance(ref currentPistol, weaponPickup);
                }
                // If the picked-up weapon was the one just equipped, update display immediately
                if (activeWeapon.isActive && activeWeapon.Weapontype == incomingWeaponType)
                {
                    UpdateWeaponDisplay();
                }
            }
            // --- Handle Main Weapon Slot ---
            else // Any weapon type other than Pistol goes to the main slot
            {
                // Check if player already has a main weapon
                if (currentMainWeapon != null)
                {
                    PickupIdentifier currentMainData = currentMainWeapon.GetComponent<PickupIdentifier>();
                    // If it's the *same type* of main weapon, just add ammo
                    if (currentMainData && currentMainData.Weapontype == incomingWeaponType)
                    {
                        AddAmmoForWeapon(incomingWeaponType, weaponPickup.magazineSize, 3); // Add 3 mags
                        Debug.Log($"Picked up duplicate {incomingWeaponType}, added ammo.");
                        // Destroy(weaponPickup.gameObject); // Handled in HandlePickupInput
                    }
                    else
                    {
                        // Different main weapon type, swap it
                        SaveCurrentWeaponAmmo(); // Save ammo if main weapon was active
                        DropWeapon(ref currentMainWeapon); // Drop the old one
                        ReplaceWeaponInstance(ref currentMainWeapon, weaponPickup); // Replace with the new one
                    }
                }
                else
                {
                    // No main weapon currently held, just take the new one
                    ReplaceWeaponInstance(ref currentMainWeapon, weaponPickup);
                }
                // If the picked-up weapon was the one just equipped, update display immediately
                if (activeWeapon.isActive && activeWeapon.Weapontype == incomingWeaponType)
                {
                    UpdateWeaponDisplay();
                }
            }
        }

        // NEW function to handle AMMO pickups
        private void PickupAmmo(PickupIdentifier ammoPickup)
        {
            if (ammoPickup == null || ammoPickup.pickupType != PickupIdentifier.PickupItemType.Ammo)
            {
                Debug.LogError("PickupAmmo called with invalid PickupIdentifier.");
                return;
            }

            // Find the corresponding WeaponType for the AmmoType
            WeaponType targetWeaponType;
            try
            {
                targetWeaponType = GetWeaponTypeFromAmmoType(ammoPickup.Ammotype);
            }
            catch (System.ArgumentOutOfRangeException e)
            {
                Debug.LogError($"Cannot pickup ammo: {e.Message}");
                return;
            }


            // Add the ammo to the dictionary (even if the player doesn't *currently* have the weapon equipped,
            // as long as they have held it before, the entry should exist).
            if (weaponAmmo.ContainsKey(targetWeaponType))
            {
                AddAmmoForWeapon(targetWeaponType, -1, ammoPickup.magsToAdd); // -1 mag size signifies don't change current mag
                Debug.Log($"Picked up {ammoPickup.magsToAdd} mags for {targetWeaponType}.");

                // If this ammo was for the currently active weapon, update the display
                if (activeWeapon.isActive && activeWeapon.Weapontype == targetWeaponType)
                {
                    // Update currentMagsLeft directly since AddAmmoForWeapon modifies the dictionary
                    currentMagsLeft = weaponAmmo[targetWeaponType].magsLeft;
                    UpdateWeaponDisplay();
                }
            }
            else
            {
                // Optional: Allow picking up ammo even if weapon was never held?
                // If so, you'd need default stats (like magazine size) to create the dictionary entry.
                // For now, we assume you must have held the weapon at least once.
                Debug.Log($"Cannot pick up {ammoPickup.Ammotype}: Player has not encountered a {targetWeaponType} yet.");
            }
            // Destroy(ammoPickup.gameObject); // Handled in HandlePickupInput
        }

        public WeaponType GetActiveWeaponType()
        {
            return activeWeapon.Weapontype;
        }

        // Helper function to add ammo to the dictionary and optionally update current weapon state
        private void AddAmmoForWeapon(WeaponType weaponType, int specificMagSize, int magsToAdd)
        {
            if (weaponAmmo.TryGetValue(weaponType, out var currentAmmo))
            {
                int newMagsLeft = currentAmmo.magsLeft + magsToAdd;
                int magSizeToStore = specificMagSize >= 0 ? specificMagSize : currentAmmo.magazineSize; // Use specific size or existing stored size

                weaponAmmo[weaponType] = (magSizeToStore, newMagsLeft); // Update dictionary

                // If this is the currently active weapon, also update its immediate ammo counts
                if (activeWeapon.isActive && activeWeapon.Weapontype == weaponType)
                {
                    // Only update currentMagazineSize if a specific size was provided (e.g., from same-weapon pickup)
                    if (specificMagSize >= 0)
                    {
                        currentMagazineSize = specificMagSize;
                    }
                    currentMagsLeft = newMagsLeft;
                }
            }
            else if (specificMagSize >= 0) // Only add if we have enough info (mag size) - case for initial pickup
            {
                weaponAmmo[weaponType] = (specificMagSize, magsToAdd);
                // If this is the currently active weapon, update its counts
                if (activeWeapon.isActive && activeWeapon.Weapontype == weaponType)
                {
                    currentMagazineSize = specificMagSize;
                    currentMagsLeft = magsToAdd;
                }
            }
            else
            {
                Debug.LogWarning($"Tried to add ammo for uninitialized weapon type {weaponType} without providing mag size.");
            }
        }


        // Replaces the weapon INSTANCE in the slot (e.g., currentPistol)
        private void ReplaceWeaponInstance(ref GameObject weaponSlot, PickupIdentifier newWeaponPickup)
        {
            if (newWeaponPickup == null || newWeaponPickup.pickupType != PickupIdentifier.PickupItemType.Weapon) return;

            // Destroy the old weapon *instance* if it exists
            if (weaponSlot != null)
            {
                Destroy(weaponSlot);
            }

            // Create a new instance of the weapon prefab associated with the pickup
            // Assumes the pickup object itself has the correct prefab/stats.
            // We instantiate the pickup's GameObject to store it.
            weaponSlot = Instantiate(newWeaponPickup.gameObject, transform.position, Quaternion.identity);
            weaponSlot.SetActive(false); // Keep the stored instance inactive

            WeaponType newWeaponType = newWeaponPickup.Weapontype;

            // Create the Weapon struct data
            Weapon newWeaponData = new Weapon
            { /* ... fill from newWeaponPickup ... */
                Weapontype = newWeaponPickup.Weapontype,
                isActive = true, // Assume we equip it immediately
                bulletSpread = newWeaponPickup.bulletSpread,
                fireRate = newWeaponPickup.fireRate,
                damage = newWeaponPickup.damage,
                bulletSpeed = newWeaponPickup.bulletSpeed,
                reloadTime = newWeaponPickup.reloadTime,
                bulletsPerShot = newWeaponPickup.bulletsPerShot,
                bulletPrefab = newWeaponPickup.bulletPrefab ?? defaultBulletPrefab,
                magazineSize = newWeaponPickup.magazineSize
            };


            // Get stored ammo, or initialize if first time picking up this type
            (int magSize, int magsLeft) ammo;
            if (weaponAmmo.TryGetValue(newWeaponType, out var storedAmmo))
            {
                // IMPORTANT: Use the *new* weapon's magazine size for the current mag count, but keep stored spare mags.
                ammo = (newWeaponData.magazineSize, storedAmmo.magsLeft);
                // Update the stored mag size in case it changed (e.g., different variant pickup)
                weaponAmmo[newWeaponType] = ammo;
            }
            else
            {
                // First time picking up this type - give default ammo
                ammo = (newWeaponData.magazineSize, 3); // Full mag + 3 spares
                weaponAmmo[newWeaponType] = ammo; // Add to dictionary
            }

            // Equip the new weapon
            EquipWeaponFromData(newWeaponData, ammo.magSize, ammo.magsLeft);

            Debug.Log($"Replaced weapon slot with {newWeaponType}.");
        }

        // Drops the specified weapon instance
        private void DropWeapon(ref GameObject weaponInstanceRef)
        {
            if (weaponInstanceRef == null || isDroppingWeapon) return;

            isDroppingWeapon = true; // Prevent dropping multiple times quickly

            PickupIdentifier pickupData = weaponInstanceRef.GetComponent<PickupIdentifier>();
            if (pickupData == null)
            {
                Debug.LogError("Cannot drop weapon: Stored instance is invalid.");
                Destroy(weaponInstanceRef); // Destroy the invalid instance
                weaponInstanceRef = null;
                isDroppingWeapon = false;
                return;
            }

            // --- Save Ammo Before Dropping ---
            // Important: Check if the weapon being dropped *is* the active one
            if (activeWeapon.isActive && activeWeapon.Weapontype == pickupData.Weapontype)
            {
                SaveCurrentWeaponAmmo();
            }
            // If not active, its ammo should already be saved correctly in the dictionary from the last switch.


            // --- Instantiate the Dropped Item ---
            // We instantiate the *stored instance* itself to create the pickup in the world
            Vector2 randomOffset = Random.insideUnitCircle * DropRange;
            Vector3 dropPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y, 0f);
            GameObject droppedItem = Instantiate(weaponInstanceRef, dropPosition, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
            droppedItem.SetActive(true); // Make the dropped item visible and interactable

            // Optional: Add physics (Rigidbody2D, Collider2D) to the dropped item prefab if not already present
            // Optional: Add a small outward force to the dropped item

            Debug.Log($"Dropped {pickupData.Weapontype}.");

            // --- Clear Player's Slot ---
            // Destroy the instance the player was holding internally
            Destroy(weaponInstanceRef);
            weaponInstanceRef = null;

            // If the dropped weapon was the active one, equip the other weapon (if available) or nothing
            if (activeWeapon.isActive && activeWeapon.Weapontype == pickupData.Weapontype)
            {
                activeWeapon.isActive = false; // Deactivate
                                               // Try equipping the other weapon slot
                if (pickupData.Weapontype == WeaponType.Pistol && currentMainWeapon != null)
                {
                    EquipMainWeapon();
                }
                else if (pickupData.Weapontype != WeaponType.Pistol && currentPistol != null)
                {
                    EquipPistol();
                }
                else
                {
                    // No other weapon to switch to
                    activeWeapon = default; // Clear active weapon struct
                    UpdateWeaponDisplay(); // Update UI to show no weapon
                }
            }


            isDroppingWeapon = false;
        }


        // --- Utility and Helpers ---

        // Saves the current magazine size and mags left to the dictionary for the active weapon
        private void SaveCurrentWeaponAmmo(bool isReloadingSave = false)
        {
            if (activeWeapon.isActive)
            {
                // If saving because a reload started, store 0 in the current mag
                int magSizeToSave = isReloadingSave ? 0 : currentMagazineSize;
                weaponAmmo[activeWeapon.Weapontype] = (magSizeToSave, currentMagsLeft);
                // Debug.Log($"Saved ammo for {activeWeapon.Weapontype}: {magSizeToSave}/{currentMagsLeft}");
            }
        }


        private void UpdateWeaponDisplay(bool showReloading = false)
        {
            if (weaponNameText != null)
            {
                if (activeWeapon.isActive)
                {
                    string ammoText;
                    if (showReloading)
                    {
                        ammoText = "Reloading...";
                    }
                    else
                    {
                        ammoText = $"{currentMagazineSize} / {currentMagsLeft}";
                    }

                    weaponNameText.text = $"{activeWeapon.Weapontype}\nAmmo: {ammoText}";
                }
                else
                {
                    weaponNameText.text = "No Weapon";
                }
            }
        }


        // Helper to get WeaponType from AmmoType (assumes they match order/naming convention)
        private WeaponType GetWeaponTypeFromAmmoType(AmmoType ammoType)
        {
            // Direct casting works if enums have the same underlying integer values and order
            int ammoIndex = (int)ammoType;
            if (System.Enum.IsDefined(typeof(WeaponType), ammoIndex))
            {
                return (WeaponType)ammoIndex;
            }
            else
            {
                throw new System.ArgumentOutOfRangeException(nameof(ammoType), $"No corresponding WeaponType found for {ammoType}");
            }

            // // Alternative: String manipulation (less efficient, more error-prone if names change)
            // string ammoName = ammoType.ToString(); // e.g., "PistolAmmo"
            // string weaponName = ammoName.Replace("Ammo", ""); // e.g., "Pistol"
            // if (System.Enum.TryParse<WeaponType>(weaponName, out WeaponType result)) {
            //     return result;
            // } else {
            //      throw new System.ArgumentOutOfRangeException(nameof(ammoType), $"Could not parse WeaponType from {ammoType}");
            // }
        }


        // Check if currently firing (useful for input checks)
        private bool isShooting()
        {
            // Considers if fire button is pressed AND within the fire rate cooldown
            return fireAction.IsPressed() && activeWeapon.isActive && Time.time < nextFireTime;
            // Or simply check if the button is pressed? Depends on desired behavior.
            // return fireAction.IsPressed() && activeWeapon.isActive;
        }

        // --- Collision Handling for Pickups ---

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Pickup")) // Make sure your pickup prefabs have the tag "Pickup"
            {
                PickupIdentifier pickup = other.GetComponent<PickupIdentifier>();
                if (pickup != null)
                {
                    // Only process if it's a weapon or ammo pickup
                    if (pickup.pickupType == PickupIdentifier.PickupItemType.Weapon ||
                        pickup.pickupType == PickupIdentifier.PickupItemType.Ammo)
                    {
                        nearbyPickup = pickup; // Store reference to the script
                        if (pickupText != null)
                        {
                            pickupText.gameObject.SetActive(true);
                            string actionKey = pickupAction.GetBindingDisplayString(); // Get readable key name

                            // Display different text based on pickup type
                            if (pickup.pickupType == PickupIdentifier.PickupItemType.Weapon)
                            {
                                pickupText.text = $"Press [{actionKey}] to pickup {pickup.Weapontype}";
                            }
                            else // Ammo
                            {
                                // Show the ammo type (e.g., PistolAmmo)
                                pickupText.text = $"Press [{actionKey}] to pickup {pickup.Ammotype}";
                            }
                        }
                    }
                }
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.CompareTag("Pickup")) // Make sure your pickup prefabs have the tag "Pickup"
            {
                PickupIdentifier pickup = other.GetComponent<PickupIdentifier>();
                if (pickup != null)
                {
                    // Only process if it's a weapon or ammo pickup
                    if (pickup.pickupType == PickupIdentifier.PickupItemType.Weapon ||
                        pickup.pickupType == PickupIdentifier.PickupItemType.Ammo)
                    {
                        nearbyPickup = pickup; // Store reference to the script
                        if (pickupText != null)
                        {
                            pickupText.gameObject.SetActive(true);
                            string actionKey = pickupAction.GetBindingDisplayString(); // Get readable key name

                            // Display different text based on pickup type
                            if (pickup.pickupType == PickupIdentifier.PickupItemType.Weapon)
                            {
                                pickupText.text = $"Press [{actionKey}] to pickup {pickup.Weapontype}";
                            }
                            else // Ammo
                            {
                                // Show the ammo type (e.g., PistolAmmo)
                                pickupText.text = $"Press [{actionKey}] to pickup {pickup.Ammotype}";
                            }
                        }
                    }
                }
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            // If the object leaving the trigger is the one we stored as nearby, clear it
            if (other.CompareTag("Pickup") && other.GetComponent<PickupIdentifier>() == nearbyPickup)
            {
                if (pickupText != null)
                {
                    pickupText.gameObject.SetActive(false); // Hide text
                }
                nearbyPickup = null; // Clear the reference
            }
        }

    }

}
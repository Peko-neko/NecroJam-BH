using UnityEngine;

namespace AlignedGames
{

    public class HumanEnemyWeaponManager : MonoBehaviour
    {
        [Header("Weapon Settings")]
        public GameObject bulletPrefab; // Prefab for the bullet projectile
        public GameObject rocketPrefab; // Prefab for the rocket projectile (rocket launcher)
        public float bulletSpeed = 10f; // Speed of the bullet projectile
        public int damage = 10; // Damage per bullet
        public float spreadAngle = 0f; // Bullet spread angle for accuracy variation
        public int bulletsPerShot = 1; // Number of bullets fired per shot
        public float shootRate = 0.2f; // Time between shots in seconds

        public WeaponPreset[] presets; // Array of weapon presets for configuring stats

        public GameObject[] weaponPrefabs; // Prefabs for different weapon models

        [Header("Weapon Types")]
        public bool Pistol; // Flag for pistol weapon type
        public bool SubMachineGun; // Flag for submachine gun type
        public bool MachineGun; // Flag for machine gun type
        public bool BurstMachineGun; // Flag for burst fire machine gun
        public bool Shotgun; // Flag for shotgun type
        public bool Rifle; // Flag for rifle type
        public bool SniperRifle; // Flag for sniper rifle type
        public bool RocketLauncher; // Flag for rocket launcher type

        [Header("Visual/Audio Additions")]
        public Sprite[] weaponVisuals; // Array of sprites for weapon visuals
        public SpriteRenderer SelectedWeapon; // SpriteRenderer to show selected weapon
        public Transform[] shootPoints; // Points from where bullets are fired
        public Sprite[] muzzleFlashSprites; // Sprites for muzzle flash effects
        public Sprite[] bulletSprites; // Sprites to represent bullets visually

        [Header("Audio Settings")]
        public AudioSource audioSource; // Audio source component for sounds
        public AudioClip shootSound; // Default shoot sound clip
        public AudioClip reloadSound; // Reload sound clip
        public WeaponAudioSet[] weaponAudioSets; // Audio sets for each weapon type

        [Header("Reload Settings")]
        public int magazineSize = 10; // Number of bullets per magazine
        public int currentAmmo; // Current ammo in magazine
        public float reloadTime = 1.5f; // Time it takes to reload
        public bool isReloading; // Flag to check if currently reloading

        private float lastShootTime; // Timestamp of last shot fired

        [System.Serializable]
        public struct WeaponPreset
        {
            public string name; // Name of the preset/weapon type
            public int damage; // Damage value for this preset
            public int bulletsPerShot; // Number of bullets fired per shot
            public float spreadAngle; // Spread angle for bullet direction randomness
            public float shootRate; // Fire rate for this preset
            public float bulletSpeed; // Speed of bullets fired
            public int magazineSize; // Magazine size for this preset
            public float reloadTime; // Reload duration for this preset

            public WeaponPreset(string name, int damage, int bulletsPerShot, float spreadAngle, float shootRate, float bulletSpeed, int magazineSize, float reloadTime)
            {
                this.name = name;
                this.damage = damage;
                this.bulletsPerShot = bulletsPerShot;
                this.spreadAngle = spreadAngle;
                this.shootRate = shootRate;
                this.bulletSpeed = bulletSpeed;
                this.magazineSize = magazineSize;
                this.reloadTime = reloadTime;
            }
        }

        [System.Serializable]
        public struct WeaponAudioSet
        {
            public AudioClip[] shootClips; // Array of shoot sound clips for variation
            public AudioClip reloadClip; // Reload sound clip
        }

        private void Awake()
        {
            currentAmmo = magazineSize; // Initialize current ammo to full magazine on awake
        }

        private void OnValidate()
        {
            AutoSetWeaponStats(); // Automatically set weapon stats in editor when values change
        }

        private void AutoSetWeaponStats()
        {
            for (int i = 0; i < presets.Length; i++)
            {
                var preset = presets[i]; // Get current preset

                if (GetWeaponTypeStatus(preset.name)) // Check if preset matches active weapon type
                {
                    damage = preset.damage; // Set damage
                    bulletsPerShot = preset.bulletsPerShot; // Set bullets per shot
                    spreadAngle = preset.spreadAngle; // Set bullet spread
                    shootRate = preset.shootRate; // Set fire rate
                    bulletSpeed = preset.bulletSpeed; // Set bullet speed
                    magazineSize = preset.magazineSize; // Set magazine size
                    reloadTime = preset.reloadTime; // Set reload time
                    currentAmmo = magazineSize; // Reset current ammo to magazine size

                    // Set weapon visual sprite if available
                    if (weaponVisuals != null && i < weaponVisuals.Length && SelectedWeapon != null)
                    {
                        SelectedWeapon.sprite = weaponVisuals[i];
                    }

                    // Set audio clips from audio sets if available
                    if (weaponAudioSets != null && i < weaponAudioSets.Length)
                    {
                        var audioSet = weaponAudioSets[i];
                        if (audioSet.shootClips != null && audioSet.shootClips.Length > 0)
                        {
                            shootSound = audioSet.shootClips[Random.Range(0, audioSet.shootClips.Length)];
                        }

                        reloadSound = audioSet.reloadClip;
                    }

                    break; // Exit loop after setting stats for active weapon
                }
            }
        }

        public GameObject GetSelectedWeaponPrefab()
        {
            int index = GetActiveWeaponIndex(); // Get active weapon index
            if (index >= 0 && weaponPrefabs != null && index < weaponPrefabs.Length)
            {
                return weaponPrefabs[index]; // Return prefab corresponding to active weapon
            }

            Debug.LogWarning("No valid weapon prefab found for enemy. " + this.gameObject.name);
            return null; // Return null if no valid prefab found
        }

        private bool GetWeaponTypeStatus(string weaponName)
        {
            // Return true if the specified weapon type is active
            switch (weaponName)
            {
                case "Pistol": return Pistol;
                case "SubMachineGun": return SubMachineGun;
                case "MachineGun": return MachineGun;
                case "BurstMachineGun": return BurstMachineGun;
                case "Shotgun": return Shotgun;
                case "Rifle": return Rifle;
                case "SniperRifle": return SniperRifle;
                case "RocketLauncher": return RocketLauncher;
                default: return false;
            }
        }

        public void TryShoot(Transform firePoint, Vector3 targetPosition)
        {
            if (isReloading) return; // Prevent shooting while reloading

            if (currentAmmo <= 0)
            {
                StartCoroutine(Reload()); // Start reload if out of ammo
                return;
            }

            if (Time.time >= lastShootTime + shootRate) // Check if enough time passed since last shot
            {
                lastShootTime = Time.time; // Update last shoot time
                Shoot(firePoint, targetPosition); // Fire the weapon
            }
        }

        private void Shoot(Transform firePoint, Vector3 targetPosition)
        {
            int weaponIndex = GetActiveWeaponIndex(); // Get current weapon index

            for (int i = 0; i < bulletsPerShot; i++) // Fire bullets per shot count
            {
                // Determine shoot position based on weapon index or fallback to firePoint
                Vector3 shootPos = shootPoints != null && weaponIndex < shootPoints.Length
                    ? shootPoints[weaponIndex].position
                    : firePoint.position;

                Vector3 direction = (targetPosition - shootPos).normalized; // Calculate direction to target
                direction = ApplySpread(direction); // Apply spread to direction

                GameObject projectile = null;

                if (RocketLauncher && rocketPrefab != null) // Use rocket prefab if rocket launcher
                {
                    projectile = Instantiate(rocketPrefab, shootPos, Quaternion.identity);
                }
                else // Use bullet prefab otherwise
                {
                    projectile = Instantiate(bulletPrefab, shootPos, Quaternion.identity);
                }

                Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
                if (rb != null)
                    rb.linearVelocity = direction * bulletSpeed; // Set projectile velocity

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg; // Calculate rotation angle
                projectile.transform.rotation = Quaternion.Euler(0, 0, angle); // Apply rotation

                EnemyProjectileBehaviour bulletScript = projectile.GetComponent<EnemyProjectileBehaviour>();
                if (bulletScript != null)
                {
                    bulletScript.SetDamage(damage); // Set damage on projectile script
                }

                if (!RocketLauncher) // If not rocket launcher, apply bullet visuals and muzzle flash
                {
                    if (bulletSprites != null && weaponIndex < bulletSprites.Length)
                    {
                        SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
                        if (sr != null)
                            sr.sprite = bulletSprites[weaponIndex]; // Set bullet sprite
                    }

                    if (muzzleFlashSprites != null && weaponIndex < muzzleFlashSprites.Length && muzzleFlashSprites[weaponIndex] != null)
                    {
                        // Create muzzle flash game object
                        GameObject flash = new GameObject("MuzzleFlash");
                        flash.transform.SetParent(shootPoints[weaponIndex]);
                        flash.transform.localPosition = Vector3.zero;
                        flash.transform.localRotation = Quaternion.identity;
                        flash.transform.localScale = Vector3.one * 1.5f;

                        SpriteRenderer sr = flash.AddComponent<SpriteRenderer>();
                        sr.sprite = muzzleFlashSprites[weaponIndex];
                        sr.sortingOrder = 10;
                        sr.sortingLayerName = SelectedWeapon.sortingLayerName;

                        StartCoroutine(FadeAndDestroy(flash, sr, 0.3f)); // Fade and destroy muzzle flash
                    }
                }
            }

            currentAmmo--; // Decrease ammo count after shooting

            // Play shooting audio if available
            if (audioSource != null && weaponAudioSets != null && weaponIndex >= 0 && weaponIndex < weaponAudioSets.Length)
            {
                AudioClip[] clips = weaponAudioSets[weaponIndex].shootClips;
                if (clips != null && clips.Length > 0)
                {
                    AudioClip selected = clips[Random.Range(0, clips.Length)];
                    if (selected != null)
                        audioSource.PlayOneShot(selected);
                }
            }
        }

        private System.Collections.IEnumerator FadeAndDestroy(GameObject obj, SpriteRenderer sr, float duration)
        {
            float halfTime = duration / 2f; // Half duration for fade in and out
            float timer = 0f;

            Color color = sr.color; // Starting color

            // Fade in
            while (timer < halfTime)
            {
                float alpha = Mathf.Lerp(0f, 1f, timer / halfTime);
                sr.color = new Color(color.r, color.g, color.b, alpha);
                timer += Time.deltaTime;
                yield return null;
            }

            timer = 0f;

            // Fade out
            while (timer < halfTime)
            {
                float alpha = Mathf.Lerp(1f, 0f, timer / halfTime);
                sr.color = new Color(color.r, color.g, color.b, alpha);
                timer += Time.deltaTime;
                yield return null;
            }

            Destroy(obj); // Destroy muzzle flash object
        }

        private Vector3 ApplySpread(Vector3 direction)
        {
            float angle = Random.Range(-spreadAngle, spreadAngle); // Random spread angle
            return Quaternion.Euler(0, 0, angle) * direction; // Rotate direction by spread angle
        }

        private System.Collections.IEnumerator Reload()
        {
            isReloading = true; // Set reload flag

            int index = GetActiveWeaponIndex();
            // Play reload sound if available
            if (audioSource != null && weaponAudioSets != null && index >= 0 && index < weaponAudioSets.Length)
            {
                AudioClip reload = weaponAudioSets[index].reloadClip;
                if (reload != null)
                    audioSource.PlayOneShot(reload);
            }

            yield return new WaitForSeconds(reloadTime); // Wait for reload time

            currentAmmo = magazineSize; // Refill ammo
            isReloading = false; // Clear reload flag
        }

        private int GetActiveWeaponIndex()
        {
            // Return index of active weapon based on boolean flags
            if (Pistol) return 0;
            if (SubMachineGun) return 1;
            if (MachineGun) return 2;
            if (BurstMachineGun) return 3;
            if (Shotgun) return 4;
            if (Rifle) return 5;
            if (SniperRifle) return 6;
            if (RocketLauncher) return 7;
            return -1; // No weapon active
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red; // Set gizmo color to red

            int index = GetActiveWeaponIndex();
            if (shootPoints != null && index >= 0 && index < shootPoints.Length && shootPoints[index] != null)
            {
                Gizmos.DrawSphere(shootPoints[index].position, 0.05f); // Draw a sphere at the shoot point
            }
        }
    }

}

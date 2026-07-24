using UnityEngine;

namespace AlignedGames
{
    public class AudioManager : MonoBehaviour
    {
        // Reference to an AudioSource component that will play the sounds
        [SerializeField] private AudioSource audioSource;

        // Pickup sound clips - array allows multiple variations for randomness
        [Header("Generic Pickup")]
        [SerializeField] private AudioClip[] pickupClips;

        // Reload sound clips specific to each weapon type
        [Header("Weapon-Specific Reload Clips")]
        [SerializeField] private AudioClip[] pistolReloadClips;
        [SerializeField] private AudioClip[] subMachineGunReloadClips;
        [SerializeField] private AudioClip[] machineGunReloadClips;
        [SerializeField] private AudioClip[] burstMachineGunReloadClips;
        [SerializeField] private AudioClip[] shotgunReloadClips;
        [SerializeField] private AudioClip[] rifleReloadClips;
        [SerializeField] private AudioClip[] sniperRifleReloadClips;
        [SerializeField] private AudioClip[] rocketLauncherReloadClips;

        // Shoot sound clips specific to each weapon type
        [Header("Weapon-Specific Shoot Clips")]
        [SerializeField] private AudioClip[] pistolShootClips;
        [SerializeField] private AudioClip[] subMachineGunShootClips;
        [SerializeField] private AudioClip[] machineGunShootClips;
        [SerializeField] private AudioClip[] burstMachineGunShootClips;
        [SerializeField] private AudioClip[] shotgunShootClips;
        [SerializeField] private AudioClip[] rifleShootClips;
        [SerializeField] private AudioClip[] sniperRifleShootClips;
        [SerializeField] private AudioClip[] rocketLauncherShootClips;

        // Cooldown time to prevent playing shoot sounds too frequently
        public float shootSoundCooldown = 0.05f;
        private float lastShootSoundTime = -1f;

        // Play a random pickup sound from the array when an item is picked up
        public void PlayPickupSound()
        {
            if (pickupClips.Length == 0)
            {
                Debug.LogWarning("No audio clips assigned for pickup sound.");
                return;
            }

            AudioClip randomClip = pickupClips[Random.Range(0, pickupClips.Length)];
            audioSource.PlayOneShot(randomClip);
        }

        // Play a reload sound for a specific weapon type
        public void PlayReloadSound(PlayerCombatManager.WeaponType weaponType)
        {
            // Choose the correct array of reload clips based on weapon type
            AudioClip[] clips = weaponType switch
            {
                PlayerCombatManager.WeaponType.Pistol => pistolReloadClips,
                PlayerCombatManager.WeaponType.SubMachineGun => subMachineGunReloadClips,
                PlayerCombatManager.WeaponType.MachineGun => machineGunReloadClips,
                PlayerCombatManager.WeaponType.BurstMachineGun => burstMachineGunReloadClips,
                PlayerCombatManager.WeaponType.Shotgun => shotgunReloadClips,
                PlayerCombatManager.WeaponType.Rifle => rifleReloadClips,
                PlayerCombatManager.WeaponType.SniperRifle => sniperRifleReloadClips,
                PlayerCombatManager.WeaponType.RocketLauncher => rocketLauncherReloadClips,
                _ => null
            };

            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning($"No reload clips for {weaponType}");
                return;
            }

            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            audioSource.PlayOneShot(randomClip);
        }

        // Play a shooting sound for a specific weapon type, respecting cooldown between sounds
        public void PlayShootSound(PlayerCombatManager.WeaponType weaponType)
        {
            // Check if enough time passed since the last shoot sound to play a new one
            if (Time.time - lastShootSoundTime < shootSoundCooldown)
                return;

            // Choose the correct array of shoot clips based on weapon type
            AudioClip[] clips = weaponType switch
            {
                PlayerCombatManager.WeaponType.Pistol => pistolShootClips,
                PlayerCombatManager.WeaponType.SubMachineGun => subMachineGunShootClips,
                PlayerCombatManager.WeaponType.MachineGun => machineGunShootClips,
                PlayerCombatManager.WeaponType.BurstMachineGun => burstMachineGunShootClips,
                PlayerCombatManager.WeaponType.Shotgun => shotgunShootClips,
                PlayerCombatManager.WeaponType.Rifle => rifleShootClips,
                PlayerCombatManager.WeaponType.SniperRifle => sniperRifleShootClips,
                PlayerCombatManager.WeaponType.RocketLauncher => rocketLauncherShootClips,
                _ => null
            };

            if (clips == null || clips.Length == 0)
            {
                Debug.LogWarning($"No shoot clips for {weaponType}");
                return;
            }

            lastShootSoundTime = Time.time;

            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            audioSource.PlayOneShot(randomClip);
        }
    }
}

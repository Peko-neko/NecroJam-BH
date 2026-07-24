using UnityEngine;

namespace AlignedGames
{

    public class PickupIdentifier : MonoBehaviour
    {
        // Enum to define if this pickup is a weapon, ammo, health, or armor
        public enum PickupItemType
        {
            Weapon,
            Ammo,
            Health,
            Armor,
            Grenade
        }

        [Header("Grenade Stats (If Grenade)")]
        public int grenadesToRestore = 3; // Default grenade restore count

        [Header("Pickup Type")]
        public PickupItemType pickupType = PickupItemType.Weapon; // Default to Weapon

        [Header("Weapon Stats (If Weapon)")]
        public PlayerCombatManager.WeaponType Weapontype;
        public float bulletSpread;
        public float fireRate;
        public float damage;
        public float bulletSpeed;
        public float reloadTime;
        public int bulletsPerShot;
        public GameObject bulletPrefab;
        public int magazineSize; // Magazine size for this weapon type

        [Header("Ammo Stats (If Ammo)")]
        public PlayerCombatManager.AmmoType Ammotype; // Which ammo type this pack contains
        public int magsToAdd = 3; // How many magazines this pickup gives

        [Header("Health Stats (If Health)")] // New section for Health pickups
        public int healthToRestore = 25;   // Amount of health this pickup gives

        [Header("Armor Stats (If Armor)")]   // New section for Armor pickups
        public int armorToRestore = 50;    // Amount of armor this pickup gives

    }

}
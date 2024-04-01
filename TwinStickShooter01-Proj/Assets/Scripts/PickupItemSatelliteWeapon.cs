using UnityEngine;

public class PickupItemSatelliteWeapon : PickupItemBase
{
    [SerializeField] protected SatelliteWeaponBase _satelliteWeaponPrefab;

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
    }

    protected override void OnPlayerPickedUp(PlayerController enteredPlayer)
    {
        // Attach the associated Satellite Weapon to the player that picked this up
        enteredPlayer.AttachSatelliteWeapon(_satelliteWeaponPrefab);
        
        base.OnPlayerPickedUp(enteredPlayer);
    }
}

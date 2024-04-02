using UnityEngine;

// This enemy target's the player's satellite weapon
public class EnemyDemonChort : EnemyBase
{
    protected override void Start()
    {
        base.Start();

        // Set the player's satellite weapon as the target
        if(PlayerController.CurrentPlayerController.CurrentSatelliteWeapon != null)
        {
            SetTarget(PlayerController.CurrentPlayerController.CurrentSatelliteWeapon.gameObject);
        }
        else
        {
            SetTarget(PlayerController.CurrentPlayerController.gameObject);
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // If the target is null, then it is likely the Satellite weapon was destroyed. Set the player as the target.
        if(_target == null)
        {
            SetTarget(PlayerController.CurrentPlayerController.gameObject);
        }
        else if(_target.TryGetComponent<PlayerController>(out var playerController))
        {
            // This enemy has the player as a target. Check if the player picked up a satellite.
            if(PlayerController.CurrentPlayerController.CurrentSatelliteWeapon != null)
            {
                SetTarget(PlayerController.CurrentPlayerController.CurrentSatelliteWeapon.gameObject);
            }
        }
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.TryGetComponent<SatelliteWeaponBase>(out var satelliteWeapon))
        {
            // This enemy has touched the player's satellite weapon. Deal damage to the satellite weapon.
            satelliteWeapon.DealDamage(_damage);

            // Play a chomping/eating sound
            AudioManager.Instance.PlaySound(AudioManager.SFX.SatelliteWeaponEaten);
        }

        base.OnTriggerEnter2D(other);
    }
}

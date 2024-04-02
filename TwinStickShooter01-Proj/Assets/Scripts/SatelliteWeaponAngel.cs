using UnityEngine;

public class SatelliteWeaponAngel : SatelliteWeaponBase
{
    protected override void KillSatelliteWeapon()
    {
        base.KillSatelliteWeapon();

        // Play Death Particle, if this enemy has one
        if(_deathParticlePrefab != null)
        {
            ParticleSystem deathParticle = GameObject.Instantiate(_deathParticlePrefab, transform.position, Quaternion.identity);
            deathParticle.Play();
        }

        // Play death cry
        AudioManager.Instance.PlaySound(AudioManager.SFX.SatelliteWeaponDeathCry);
    }
}

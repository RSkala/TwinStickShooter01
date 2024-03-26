using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("WeaponBase Fields")]
    [SerializeField] ProjectileBase _projectilePrefab;
    [SerializeField] AudioClip[] _profileFireClips; // RKS: MARKED FOR DEATH
    [SerializeField] AudioManager.SFX _projectileFireSfx = AudioManager.SFX.None;
    [SerializeField] float _projectileShotsPerSecond;
    [SerializeField] Transform _firePoint;
    [SerializeField] float _projectileSpeed;

    public SpriteRenderer SpriteRenderer { get; private set; }

    //protected float _fireRate;
    public float FireRate { get; protected set; }

    protected virtual void Start()
    {
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        FireRate = 1.0f / _projectileShotsPerSecond;
    }

    void Update()
    {
        
    }

    public void UpdateWeaponRotation(Quaternion weaponRotation)
    {
        // probably not necessary
    }

    public void FireProjectile(Quaternion projectileRotation)
    {
        // RKS TODO: Allocate references on Start for pooling
        //ProjectilePistolBullet newPistolBullet = GameObject.Instantiate(_pistolBulletPrefab, projectilePosition, projectileRotation);
        //ProjectileArrow newPistolBullet = GameObject.Instantiate(_projectileArrowPrefab, projectilePosition, projectileRotation);

        // Fire the projectile from the weapon
        ProjectileBase newProjectile = GameObject.Instantiate(_projectilePrefab, _firePoint.position, projectileRotation); // WIP

        // Play the fire sound
        AudioManager.Instance.PlaySound(_projectileFireSfx);
    }
}

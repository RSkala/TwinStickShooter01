using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    [Header("WeaponBase Fields")]
    [SerializeField] ProjectileBase _projectilePrefab;
    [SerializeField] AudioManager.SFX _projectileFireSfx = AudioManager.SFX.None;
    [SerializeField] float _projectileShotsPerSecond;
    [SerializeField] Transform _firePoint;
    [SerializeField] float _projectileSpeed;
    [SerializeField] float _projectileLifetime;

    public SpriteRenderer SpriteRenderer { get; private set; }

    public float FireRate { get; protected set; }

    protected virtual void Start()
    {
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        FireRate = 1.0f / _projectileShotsPerSecond;
    }

    public void FireProjectile(Quaternion projectileRotation)
    {
        // RKS TODO: Allocate references on Start for pooling

        // Fire the projectile from the weapon and initialize its stats
        ProjectileBase newProjectile = GameObject.Instantiate(_projectilePrefab, _firePoint.position, projectileRotation);
        newProjectile.Init(_projectileSpeed, _projectileLifetime);

        // Play the fire sound
        AudioManager.Instance.PlaySound(_projectileFireSfx);
    }
}
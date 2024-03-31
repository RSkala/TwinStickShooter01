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
    [SerializeField] float _projectileDamage;

    public SpriteRenderer SpriteRenderer { get; private set; }

    public float FireRate { get; protected set; }
    public ProjectileBase ProjectilePrefab => _projectilePrefab;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileLifetime => _projectileLifetime;
    public float ProjectileDamage => _projectileDamage;

    protected virtual void Start()
    {
        SpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        FireRate = 1.0f / _projectileShotsPerSecond;
    }

    public void FireProjectile(Quaternion projectileRotation,
                               PlayerController.SpreadGunSize spreadGunSize = PlayerController.SpreadGunSize.SingleBullet,
                               float spreadGunAngle = 0.0f)
    {
        // RKS TODO: Allocate references on Start for pooling

        // Always fire the first projectile straight from the weapon firepoint
        ProjectileBase newProjectile = GameObject.Instantiate(_projectilePrefab, _firePoint.position, projectileRotation);
        newProjectile.Init(_projectileSpeed, _projectileLifetime, _projectileDamage);

        // Handle Spread Gun
        int totalBulletsSpawned = 1;
        float angleMultiple = 1.0f;

        // In each loop iteration, spawn 2 bullets in both the left and right rotation directions
        while(totalBulletsSpawned < (int)spreadGunSize)
        {
            Quaternion leftRotation = projectileRotation * Quaternion.Euler(Vector3.forward * spreadGunAngle * angleMultiple);
            Quaternion rightRotation = projectileRotation * Quaternion.Euler(Vector3.forward * -spreadGunAngle * angleMultiple);

            ProjectileBase newLeftProjectile = GameObject.Instantiate(_projectilePrefab, _firePoint.position, leftRotation);
            ProjectileBase newRightProjectile = GameObject.Instantiate(_projectilePrefab, _firePoint.position, rightRotation);

            newLeftProjectile.Init(_projectileSpeed, _projectileLifetime, _projectileDamage);
            newRightProjectile.Init(_projectileSpeed, _projectileLifetime, _projectileDamage);

            angleMultiple += 1.0f;
            totalBulletsSpawned += 2;
        }

        // Play only a single fire sound regardless of how many projectiles were fired
        AudioManager.Instance.PlaySound(_projectileFireSfx);
    }

    public void ShowProjectileWeaponSprite() { SpriteRenderer.enabled = true; }
    public void HideProjectileWeaponSprite() { SpriteRenderer.enabled = false; }
}

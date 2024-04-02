using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class SatelliteWeaponBase : MonoBehaviour
{
    [Tooltip("Degrees per second which it rotates around the owning player")]
    [SerializeField] float _rotationSpeed;

    [Tooltip("Distance away from the 'owner' (basically the radius of rotation)")]
    [SerializeField] float _distanceFromOwner;

    [Tooltip("Clockwise or Counterwise rotation")]
    [SerializeField] RotationDirection _rotationDirection;

    [Tooltip("Whether or not to face the same X direction as the player's current projectile weapon")]
    [SerializeField] bool _useProjectileWeaponFacing = true;

    [Tooltip("Health of this satellite weapon, so they can be destroyed")]
    [SerializeField] float _maxHealth;

    [Tooltip("Particle that will play when this satellite is destroyed")]
    [SerializeField] protected ParticleSystem _deathParticlePrefab;

    Rigidbody2D _rigidbody2D;
    SpriteRenderer _spriteRenderer;

    float _curRotationAngle = 0.0f;
    float _currentHealth;

    // The Rigidbody2D of the GameObject this 'satellite' will rotate around
    Rigidbody2D _ownerRigidbody2D;

    enum RotationDirection
    {
        Clockwise,
        Counterclockwise
    }

    public void Init(Rigidbody2D ownerRigidbody2D)
    {
        _ownerRigidbody2D = ownerRigidbody2D;
    }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentHealth = _maxHealth;
    }

    void FixedUpdate()
    {
        // Multiply the rotation angle by 1 or -1 depending on direction
        float rotationDirMultiple = _rotationDirection == RotationDirection.Clockwise ? 1.0f : -1.0f;

        // Increment the rotation angle in the desired rotation direction
        _curRotationAngle += _rotationSpeed * Time.fixedDeltaTime * rotationDirMultiple;

        // Ensure the rotation angle stays within (-360, 360)
        if(_curRotationAngle >= 360.0f)
        {
            _curRotationAngle = 0.0f;
        }
        else if(_curRotationAngle <= -360.0f)
        {
            _curRotationAngle = 0.0f;
        }

        // Calculate the local X and Y positions with the current rotation
        // Using SOHCAHTOA and Polar Coordinates: (opp = y, adj = x, r = hyp)
        // x = r * cos(theta)  <= cos(theta) = x / r
        // y = r * sin(theta)  <= sin(theta) = y / r
        // r = _distanceFromOwner
        // theta = _curRotationAngle

        float xPos = _distanceFromOwner * Mathf.Cos(_curRotationAngle * Mathf.Deg2Rad);
        float yPos = _distanceFromOwner * Mathf.Sin(_curRotationAngle * Mathf.Deg2Rad);

        // Adjust the position with the owner's position (move the position to the owner's coordinate space)
        Vector2 newPosition = new Vector2(xPos, yPos) + _ownerRigidbody2D.position;
        _rigidbody2D.MovePosition(newPosition);
    }

    public bool IsActive => gameObject.activeSelf;
    public void SetActive(bool active) { gameObject.SetActive(active); }
    public Vector2 GetPosition => _rigidbody2D.position;

    public virtual void UpdateFacingDirection(GameManager.SpriteFacingDirection spriteFacingDirection)
    {
        if(!_useProjectileWeaponFacing)
        {
            return;
        }

        switch(spriteFacingDirection)
        {
            case GameManager.SpriteFacingDirection.Right: _spriteRenderer.flipX = false; break;
            case GameManager.SpriteFacingDirection.Left: _spriteRenderer.flipX = true; break;
            default: break;
        }
    }

    public void DealDamage(float damageAmount)
    {
        _currentHealth -= damageAmount;
        _currentHealth = Mathf.Max(0.0f, _currentHealth);
        if(_currentHealth <= 0.0f)
        {
            Destroy(gameObject);
            
            // Play Death Particle, if this enemy has one
            if(_deathParticlePrefab != null)
            {
                ParticleSystem deathParticle = GameObject.Instantiate(_deathParticlePrefab, transform.position, Quaternion.identity);
                deathParticle.Play();
            }
        }
    }
}

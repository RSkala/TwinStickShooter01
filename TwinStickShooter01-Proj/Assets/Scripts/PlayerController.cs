using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ---------------------------------------------------
    [Header("Player Movement")]

    [Tooltip("How quickly the player moves")]
    [SerializeField] float _moveSpeed;

    [Tooltip("How long the player stays in its dash state")]
    [SerializeField] float _dashTime;

    [Tooltip("How quickly the player moves while dashing")]
    [SerializeField] float _dashSpeed;

    // ---------------------------------------------------
    [Header("Projectile Weapon")]

    [Tooltip("The player's currently selected projectile weapon")]
    [SerializeField] WeaponBase _currentWeapon;

    [Tooltip("Projectile weapons will rotate around this point when aiming")]
    [SerializeField] Transform _projectileWeaponRotationPoint;

    [Tooltip("If enabled, the player's projectile weapon will keep firing as long as the right stick is held in a direction")]
    [SerializeField] bool _rightStickContinuousFire = true;

    // ---------------------------------------------------
    [Header("Melee Weapon")]

    [Tooltip("The player's currently selected melee weapon")]
    [SerializeField] WeaponMeleeBase _currentMeleeWeapon;

    [ Tooltip("Melee weapons will rotate around this point. Note that this only affects the facing direction.")]
    [SerializeField] Transform _meleeWeaponRotationPoint;

    // ---------------------------------------------------
    [Header("Satellite Weapon")]

    [Tooltip("The player's currently selected satellite weapon")]
    [SerializeField] SatelliteWeapon _satelliteWeapon;

    [Tooltip("Whether to enabled or disable player satellite weapons")]
    [SerializeField] bool _enableSatelliteWeapon;

    // ---------------------------------------------------

    // Player input values
    Vector2 _moveInput;
    Vector2 _lookInput;
    Vector2 _mouseLookPosition;
    Vector2 _dashInput;

    Rigidbody2D _rigidbody2D;
    SpriteRenderer _spriteRenderer;

    Camera _mainCamera;
    float _timeSinceLastShot;
    bool _useMouseLook = false;
    SpriteFacingDirection _playerFacingDirection = SpriteFacingDirection.Invalid;
    SpriteFacingDirection _gunFacingDirection = SpriteFacingDirection.Invalid;

    // Dash values
    bool _isDashing = false;
    float _dashTimeElapsed;

    // Melee values
    bool _isMeleeAttacking = false;

    enum SpriteFacingDirection
    {
        Invalid,
        Right,
        Left
    }

    void ResetTimeSinceLastShot() { _timeSinceLastShot = _currentWeapon.FireRate; }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _mainCamera = Camera.main;

        // Initialize "time since last shot" to the fire rate, so there is no delay on the very first shot
        ResetTimeSinceLastShot();

        // Sprite faces right by default
        _playerFacingDirection = SpriteFacingDirection.Right;

        // Default gun as pointing to the right
        _gunFacingDirection = SpriteFacingDirection.Right;

        // Enable/Disable Satellite Weapon and initialize it with this player as the owner
        _satelliteWeapon.SetActive(_enableSatelliteWeapon);
        _satelliteWeapon.Init(_rigidbody2D);

        // Start the melee weapon disabled and listen for the melee attack end animation event
        _currentMeleeWeapon.SetActive(false);
        _currentMeleeWeapon.eventMeleeAttackEnd.AddListener(OnMeleeAttackEnd);
    }

    void FixedUpdate()
    {
        // Update Dash
        if(_isDashing)
        {
            _dashTimeElapsed += Time.fixedDeltaTime;
            if(_dashTimeElapsed >= _dashTime)
            {
                _isDashing = false;
            }

            // Continue moving in the direction the player input when dash was activated 
            Vector2 movementDirection = _dashInput;
            Vector2 newPosition = _rigidbody2D.position + movementDirection * _dashSpeed * Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(newPosition);

            // No more movement or look updates while dashing
            return;
        }

        // Update Movement
        if(!_moveInput.Equals(Vector2.zero))
        {
            Vector2 movementDirection = _moveInput;
            Vector2 newPosition = _rigidbody2D.position + movementDirection * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(newPosition);

            // Update the player facing based on the player's movement input
            _playerFacingDirection = _moveInput.x >= 0.0f ? SpriteFacingDirection.Right : SpriteFacingDirection.Left;
        }

        // Update Look
        if(!_lookInput.Equals(Vector2.zero))
        {
            Vector3 cross = Vector3.Cross(Vector2.up, _lookInput);
            float flipValue = cross.z < 0.0f ? -1.0f : 1.0f;
            float rotateAngle = Vector2.Angle(Vector2.up, _lookInput) * flipValue;
            _projectileWeaponRotationPoint.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);

            // Update the gun facing based on the player's look input direction
            _gunFacingDirection = cross.z >= 0.0f ? SpriteFacingDirection.Left : SpriteFacingDirection.Right;

            // If "Right Stick Continuous Fire" is enabled, then fire bullets as long as the right stick is held in any direction (e.g. like Smash TV)
            // Do not fire if melee attacking
            if(_rightStickContinuousFire && !_isMeleeAttacking)
            {
                _timeSinceLastShot += Time.fixedDeltaTime;
                if(_timeSinceLastShot >= _currentWeapon.FireRate)
                {
                    _currentWeapon.FireProjectile(_projectileWeaponRotationPoint.rotation);
                    FireProjectileFromSatelliteWeapon();
                    _timeSinceLastShot = 0.0f;
                }
            }   
        }
        else
        {
            if(_useMouseLook)
            {
                // Get the direction from the player character to the mouse position
                Vector2 dirPlayerToMousePos = (_mouseLookPosition - _rigidbody2D.position).normalized;

                Vector3 cross = Vector3.Cross(Vector2.up, dirPlayerToMousePos);
                float flipValue = cross.z < 0.0f ? -1.0f : 1.0f;
                float rotateAngle = Vector2.Angle(Vector2.up, dirPlayerToMousePos) * flipValue;
                _projectileWeaponRotationPoint.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);

                // Update the gun facing based on the player's mouse cursor direction
                _gunFacingDirection = cross.z >= 0.0f ? SpriteFacingDirection.Left : SpriteFacingDirection.Right;
            }
        }

        // Update facing direction
        UpdatePlayerSpriteFacingDirection();

        // Update gun facing direction
        UpdateGunSpriteFacingDirection();

        // TEMP / TEST - Set the melee weapon rotation to match the projectile weapon rotation
        _meleeWeaponRotationPoint.rotation = _projectileWeaponRotationPoint.rotation;
    }

    void UpdatePlayerSpriteFacingDirection()
    {
        switch(_playerFacingDirection)
        {
            case SpriteFacingDirection.Right: _spriteRenderer.flipX = false; break;
            case SpriteFacingDirection.Left: _spriteRenderer.flipX = true; break;
            default: break;
        }
    }

    void UpdateGunSpriteFacingDirection()
    {
        switch(_gunFacingDirection)
        {
            case SpriteFacingDirection.Right: _currentWeapon.SpriteRenderer.flipY = false; break;
            case SpriteFacingDirection.Left: _currentWeapon.SpriteRenderer.flipY = true; break;
            default: break;
        }
    }

    void OnMove(InputValue inputValue)
    {
        _moveInput = inputValue.Get<Vector2>();
    }

    void OnLook(InputValue inputValue)
    {
        _lookInput = inputValue.Get<Vector2>();

        // The player is using their gamepad's right thumbstick for aiming, so do not use mouse look for aiming the gun
        _useMouseLook = false;

        // If the the look input was zeroed out (meaning the player stopped pressing the right thumbstick in a direction), then reset the time since last shot.
        // RKS TODO: This causes a bug where the user could "cheat" the fire rate by mashing the right thumbstick direction, since the time is reset.
        if(_lookInput.Equals(Vector2.zero))
        {
            ResetTimeSinceLastShot();
        }
    }

    void OnMouseMove(InputValue inputValue)
    {
        Vector3 mousePosition = inputValue.Get<Vector2>();

        // The 2D Orthographic camera's nearClipPlane needs to be used for the z, otherwise the "z" (forward) will be outside the bounds of our game view
        mousePosition.z = _mainCamera.nearClipPlane;
        Vector3 mouseWorldPoint = _mainCamera.ScreenToWorldPoint(mousePosition);
        _mouseLookPosition = mouseWorldPoint;

        // The player has moved their mouse, so use mouse look for the player's gun direction
        _useMouseLook = true;

        // Clear the lookInput (gamepad right thumbstick)
        _lookInput = Vector2.zero;
    }

    void OnFire(InputValue inputValue)
    {
        // Do not allow the player to fire while dashing
        if(_isDashing)
        {
            return;
        }

        // Do not allow the player to fire while melee attacking
        if(_isMeleeAttacking)
        {
            return;
        }

        // Always fire the first bullet straight in front of the barrel
        _currentWeapon.FireProjectile(_projectileWeaponRotationPoint.rotation);

        // Fire projectile from Satellite Weapon
        FireProjectileFromSatelliteWeapon();
    }

    void OnDash(InputValue inputValue)
    {
        // Only allow dashing if the player has some movement input
        if(!_moveInput.Equals(Vector2.zero))
        {
            // Use the player's current movement input to use for the dash input
            _dashInput = _moveInput;
            _isDashing = true;
            _dashTimeElapsed = 0.0f;
            AudioManager.Instance.PlaySound(AudioManager.SFX.Dash);
        }
    }

    void FireProjectileFromSatelliteWeapon()
    {
        // Fire a bullet from the satellite weapon, if enabled. Do not play a sound, as a sound was already played when the weapon was fired.
        if(_satelliteWeapon.IsActive)
        {
            ProjectileBase newProjectile = GameObject.Instantiate(_currentWeapon.ProjectilePrefab, _satelliteWeapon.GetPosition, _projectileWeaponRotationPoint.rotation);
            newProjectile.Init(_currentWeapon.ProjectileSpeed, _currentWeapon.ProjectileLifetime);
        }
    }

    void OnMelee(InputValue inputValue)
    {
        // Do not allow the player to swing the sword while dashing or already melee attacking
        if(_isDashing || _isMeleeAttacking)
        {
            return;
        }

        // Hide the projectile weapon sprite
        _currentWeapon.HideProjectileWeaponSprite();

        // Unhide the currently selected melee weapon and mark the player as melee attacking
        _currentMeleeWeapon.SetActive(true);
        _isMeleeAttacking = true;
    }

    void OnMeleeAttackEnd()
    {
        // show the projectile weapon sprite
        _currentWeapon.ShowProjectileWeaponSprite();

        // Mark the player as no longer melee attacking
        _isMeleeAttacking = false;

        // Reset time since last shot so the player can immediately fire again
        ResetTimeSinceLastShot();
    }
}

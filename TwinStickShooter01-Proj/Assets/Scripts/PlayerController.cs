using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ---------------------------------------------------
    [Header("Health")]
    [SerializeField] float _playerMaxHealth;

    // ---------------------------------------------------
    [Header("Player Movement")]

    [Tooltip("How quickly the player moves")]
    [SerializeField] float _moveSpeed;

    [Tooltip("How long the player stays in its dash state")]
    [SerializeField] float _dashTime;

    [Tooltip("How quickly the player moves while dashing")]
    [SerializeField] float _dashSpeed;

    [Tooltip("How long it takes to recharge the player's dash (in seconds)")]
    [SerializeField] float _dashRechargeTime;

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
    [Header("Movement Collision")]
    [SerializeField] ContactFilter2D movementContactFilter;
    [SerializeField] float collisionOffset = 0.05f;

    // ---------------------------------------------------
    [Header("Power Up")]

    [Tooltip("How long a picked up weapon powerup will be in effect. This applies to all weapon powerups.")]
    [SerializeField] float _powerupWeaponTime;

    // ---------------------------------------------------
    [Header("Spread Gun")]
    [SerializeField] SpreadGunSize _spreadGunSize = SpreadGunSize.SingleBullet;
    [SerializeField] [Range(0.1f, 90.0f)] float _spreadGunAngle = 10.0f;

    // ---------------------------------------------------
    [Header("UI")]
    [SerializeField] Slider _healthMeter;
    [SerializeField] Slider _dashRechargeMeter;
    [SerializeField] Slider _weaponPowerupTimeMeter; // Countdown timer. Only counts down WHILE firing.
    [SerializeField] Text _playerScore;

    // ---------------------------------------------------

    // Player input values
    Vector2 _moveInput;
    Vector2 _lookInput;
    Vector2 _mouseLookPosition;
    Vector2 _dashInput;

    // Components
    Rigidbody2D _rigidbody2D;
    SpriteRenderer _spriteRenderer;
    TrailRenderer _dashTrailRenderer;

    Camera _mainCamera;
    float _timeSinceLastShot;
    bool _useMouseLook = false;
    GameManager.SpriteFacingDirection _playerFacingDirection = GameManager.SpriteFacingDirection.Invalid;
    GameManager.SpriteFacingDirection _gunFacingDirection = GameManager.SpriteFacingDirection.Invalid;

    // Dash values
    bool _isDashing = false; // Whether or not the player is dashing
    float _dashTimeElapsed; // How long the player has been in their dashing state
    float _dashRechargeTimeElapsed; // Timer for recharging the dash ability

    // Melee values
    bool _isMeleeAttacking = false;

    // Movement Collision
    List<RaycastHit2D> _movementRaycastHitsDash = new List<RaycastHit2D>();
    List<RaycastHit2D> _movementRaycastHitsXDir = new List<RaycastHit2D>();
    List<RaycastHit2D> _movementRaycastHitsYDir = new List<RaycastHit2D>();

    // Satellite Weapon
    SatelliteWeaponBase _currentSatelliteWeapon;

    // Spread Gun Sizes
    public enum SpreadGunSize
    {
        SingleBullet = 1,
        ThreeBullets = 3,
        FiveBullets = 5,
        SevenBullets = 7,
        NineBullets = 9,
        ElevenBullets = 11
    }

    // Player's Health / Is Dead
    float _currentHealth;
    bool _isDead;

    // Weapon power ups
    bool _powerupWeaponActive = false; // Test: For now, just use SpreadGun
    float _powerupWeaponTimer;

    // TEMP/TEST: Just use the current active PlayerController
    public static PlayerController CurrentPlayerController { get; private set; }
    public SatelliteWeaponBase CurrentSatelliteWeapon => _currentSatelliteWeapon;

    void ResetTimeSinceLastShot() { _timeSinceLastShot = _currentWeapon.FireRate; }

    void Awake()
    {
        CurrentPlayerController = this;
    }

    void Start()
    {
        // Initialize Components
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _dashTrailRenderer = GetComponent<TrailRenderer>();

        _mainCamera = Camera.main;

        // Health
        if(_playerMaxHealth <= 0.0f)
        {
            Debug.LogError("Player health has not been set.");
        }

        // Set player's current health to the max health
        _currentHealth = _playerMaxHealth;
        _isDead = false;

        // Initialize "time since last shot" to the fire rate, so there is no delay on the very first shot
        ResetTimeSinceLastShot();

        // Sprite faces right by default
        _playerFacingDirection = GameManager.SpriteFacingDirection.Right;

        // Default gun as pointing to the right
        _gunFacingDirection = GameManager.SpriteFacingDirection.Right;

        // Start the melee weapon disabled and listen for the melee attack end animation event
        _currentMeleeWeapon.SetActive(false);
        _currentMeleeWeapon.eventMeleeAttackEnd.AddListener(OnMeleeAttackEnd);

        // The player should be allowed to dash immediately after spawning
        _dashRechargeTimeElapsed = _dashRechargeTime;

        // The player starts without a weapon powerup
        _weaponPowerupTimeMeter.gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if(_isDead)
        {
            return;
        }

        // The "Dash Trail Renderer" should only emit while the player is dashing
        _dashTrailRenderer.emitting = _isDashing;

        // Update Dash
        if(_isDashing)
        {
            _dashTrailRenderer.emitting = true; 

            // Check collision against walls
            int raycastCollisionCount = _rigidbody2D.Cast
            (
                _dashInput,
                movementContactFilter,
                _movementRaycastHitsDash,
                _dashSpeed * Time.fixedDeltaTime + collisionOffset
            );

            if(raycastCollisionCount > 0)
            {
                // Player collided with a wall while dashing. Immediately stop.
                _isDashing = false;
                return;
            }

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
        else
        {
            // The player is not dashing. Recharge the dash meter.
            _dashRechargeTimeElapsed += Time.fixedDeltaTime;
            _dashRechargeTimeElapsed = Mathf.Clamp(_dashRechargeTimeElapsed, 0.0f, _dashRechargeTime);

            // Update the dash meter in the UI. RKS TODO: Use a UnityEvent
            _dashRechargeMeter.value = _dashRechargeTimeElapsed > 0.0f ? _dashRechargeTimeElapsed / _dashRechargeTime : 0.0f;
        }

        // Update Movement
        if(!_moveInput.Equals(Vector2.zero))
        {
            // Check collision against walls in the X direction
            int raycastCollisionCountXDir = _rigidbody2D.Cast
            (
                new Vector2(_moveInput.x, 0.0f),
                movementContactFilter,
                _movementRaycastHitsXDir,
                _moveSpeed * Time.fixedDeltaTime + collisionOffset
            );

            // Check collision against walls in the Y direction
            int raycastCollisionCountYDir = _rigidbody2D.Cast
            (
                new Vector2(0.0f, _moveInput.y),
                movementContactFilter,
                _movementRaycastHitsYDir,
                _moveSpeed * Time.fixedDeltaTime + collisionOffset
            );

            // Create a new modified movement direction, depending on whether the player collided with walls.
            // Separating into two separate raycasts allows the player to "slide" along the wall in a non-collided direction.
            float modifiedMoveInputX = raycastCollisionCountXDir == 0 ? _moveInput.x : 0.0f;
            float modifiedMoveInputY = raycastCollisionCountYDir == 0 ? _moveInput.y : 0.0f;
            Vector2 movementDirection = new Vector2(modifiedMoveInputX, modifiedMoveInputY);

            // Move to the new position
            Vector2 newPosition = _rigidbody2D.position + movementDirection * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(newPosition);

            // Update the player facing based on the player's movement input
            _playerFacingDirection = _moveInput.x >= 0.0f ? GameManager.SpriteFacingDirection.Right : GameManager.SpriteFacingDirection.Left;
        }

        // Update Look
        if(!_lookInput.Equals(Vector2.zero))
        {
            Vector3 cross = Vector3.Cross(Vector2.up, _lookInput);
            float flipValue = cross.z < 0.0f ? -1.0f : 1.0f;
            float rotateAngle = Vector2.Angle(Vector2.up, _lookInput) * flipValue;
            _projectileWeaponRotationPoint.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);

            // Update the gun facing based on the player's look input direction
            _gunFacingDirection = cross.z >= 0.0f ? GameManager.SpriteFacingDirection.Left : GameManager.SpriteFacingDirection.Right;

            // If "Right Stick Continuous Fire" is enabled, then fire bullets as long as the right stick is held in any direction (e.g. like Smash TV)
            // Do not fire if melee attacking
            if(_rightStickContinuousFire && !_isMeleeAttacking)
            {
                _timeSinceLastShot += Time.fixedDeltaTime;
                if(_timeSinceLastShot >= _currentWeapon.FireRate)
                {
                    _currentWeapon.FireProjectile(_projectileWeaponRotationPoint.rotation, _spreadGunSize, _spreadGunAngle);
                    FireProjectileFromSatelliteWeapon();
                    _timeSinceLastShot = 0.0f;
                }

                // TEMP / TEST: Power up Triple Shot
                if(_powerupWeaponActive)
                {
                    _powerupWeaponTimer -= Time.fixedDeltaTime;
                    if(_powerupWeaponTimer <= 0.0f)
                    {
                        _spreadGunSize = SpreadGunSize.SingleBullet;
                        _powerupWeaponActive = false;
                        _weaponPowerupTimeMeter.gameObject.SetActive(false);
                    }

                    _weaponPowerupTimeMeter.value = _powerupWeaponTime > 0.0f ? _powerupWeaponTimer / _powerupWeaponTime : 0.0f;
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
                _gunFacingDirection = cross.z >= 0.0f ? GameManager.SpriteFacingDirection.Left : GameManager.SpriteFacingDirection.Right;
            }
        }

        // Update facing direction
        UpdatePlayerSpriteFacingDirection();

        // Update gun facing direction
        UpdateGunSpriteFacingDirection();

        // Update satellite weapon facing direction
        if(_currentSatelliteWeapon != null)
        {
            _currentSatelliteWeapon.UpdateFacingDirection(_gunFacingDirection);
        }

        // Set the melee weapon rotation depending on how the player is pressing their thumbsticks
        // Priority will go to the right thumbstick aiming. Otherwise, will use the movement direction.
        if(!_lookInput.Equals(Vector2.zero))
        {
            // The user is pressing the right thumbstick in a direction.
            _meleeWeaponRotationPoint.rotation = _projectileWeaponRotationPoint.rotation;
        }
        else
        {
            // The user is NOT pressing the right thumbstick. Use the player's current movement direction
            if(!_moveInput.Equals(Vector2.zero))
            {
                // Do not allow the player to change the direction of the melee attack WHILE melee attacking.
                // Note the rotation is updated when moving but not melee attacking, so the rotation will be correct.
                if(!_isMeleeAttacking)
                {
                    Vector3 cross = Vector3.Cross(Vector2.up, _moveInput);
                    float flipValue = cross.z < 0.0f ? -1.0f : 1.0f;
                    float rotateAngle = Vector2.Angle(Vector2.up, _moveInput) * flipValue;
                    _meleeWeaponRotationPoint.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);
                }
            }
        }

        _healthMeter.value = _playerMaxHealth > 0.0f ? _currentHealth / _playerMaxHealth : 0.0f;
        _playerScore.text = GameManager.Instance.TotalPoints.ToString();
    }

    void UpdatePlayerSpriteFacingDirection()
    {
        switch(_playerFacingDirection)
        {
            case GameManager.SpriteFacingDirection.Right: _spriteRenderer.flipX = false; break;
            case GameManager.SpriteFacingDirection.Left: _spriteRenderer.flipX = true; break;
            default: break;
        }
    }

    void UpdateGunSpriteFacingDirection()
    {
        switch(_gunFacingDirection)
        {
            case GameManager.SpriteFacingDirection.Right: _currentWeapon.SpriteRenderer.flipY = false; break;
            case GameManager.SpriteFacingDirection.Left: _currentWeapon.SpriteRenderer.flipY = true; break;
            default: break;
        }
    }

    void OnMove(InputValue inputValue)
    {
        // Do not allow the player to move while melee attacking (NOTE: This could be a good power-up / ability)
        if(_isMeleeAttacking)
        {
            _moveInput = Vector2.zero;
            return;
        }

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

        // Fire projectile from the equipped weapon
        _currentWeapon.FireProjectile(_projectileWeaponRotationPoint.rotation, _spreadGunSize, _spreadGunAngle);

        // Fire projectile from Satellite Weapon
        FireProjectileFromSatelliteWeapon();
    }

    void OnDash(InputValue inputValue)
    {
        // Do not allow the player to dash while the dash meter is recharging
        if(_dashRechargeTimeElapsed < _dashRechargeTime)
        {
            return;
        }

        // Only allow dashing if the player has some movement input
        if(!_moveInput.Equals(Vector2.zero))
        {
            // Use the player's current movement input to use for the dash input
            _dashInput = _moveInput;
            _isDashing = true;
            _dashTimeElapsed = 0.0f;
            _dashRechargeTimeElapsed = 0.0f;
            AudioManager.Instance.PlaySound(AudioManager.SFX.Dash);
        }
    }

    void FireProjectileFromSatelliteWeapon()
    {
        // Fire a bullet from the satellite weapon, if enabled. Do not play a sound, as a sound was already played when the weapon was fired.
        if(_currentSatelliteWeapon != null)
        {
            ProjectileBase newProjectile = GameObject.Instantiate(_currentWeapon.ProjectilePrefab, _currentSatelliteWeapon.GetPosition, _projectileWeaponRotationPoint.rotation);
            newProjectile.Init(_currentWeapon.ProjectileSpeed, _currentWeapon.ProjectileLifetime, _currentWeapon.ProjectileDamage);
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

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(GetType().Name + ".OnTriggerEnter2D - " + gameObject.name + ", other: " + other.gameObject.name);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log(GetType().Name + ".OnTriggerEnter2D - " + gameObject.name + ", collision: " + collision.gameObject.name);
    }

    public void DealDamage(float damageAmount)
    {
        if(_isDashing)
        {
            //Debug.Log("Player is invincible while dashing! No damage will be dealt.");
            return;
        }

        _currentHealth -= damageAmount;
        _currentHealth = Mathf.Max(0.0f, _currentHealth);
        if(_currentHealth <= 0.0f)
        {
            // Player is dead. Show game over screen.
            GameManager.Instance.ShowGameOverScreen();
            _isDead = true;
            Destroy(_currentSatelliteWeapon);
        }
    }

    public void AttachSatelliteWeapon(SatelliteWeaponBase satelliteWeaponPrefab)
    {
        // If the player is attempting to pickup a satellite weapon of the same type, ignore this. RKS TODO: Allowing more than one satellite would be a cool feature!
        if(_currentSatelliteWeapon != null && _currentSatelliteWeapon.GetType() == satelliteWeaponPrefab.GetType())
        {
            return;
        }

        _currentSatelliteWeapon = GameObject.Instantiate(satelliteWeaponPrefab, transform.position, Quaternion.identity);
        _currentSatelliteWeapon.Init(_rigidbody2D);
    }

    public void ActivateTripleShot()
    {
        _powerupWeaponTimer = 5.0f; // 5 seconds -- TODO: Pass from weapon
        _powerupWeaponTime = 5.0f; // 5 seconds -- TODO: Pass from weapon
        _powerupWeaponActive = true;
        _spreadGunSize = SpreadGunSize.ThreeBullets;
        _weaponPowerupTimeMeter.value = 1.0f;
        _weaponPowerupTimeMeter.gameObject.SetActive(true);
    }
}

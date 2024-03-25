using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float _moveSpeed;
    [SerializeField] Transform _gunRotationPoint;
    [SerializeField] Transform _gunFirepoint;
    [SerializeField] bool _rightStickContinuousFire = true;
    [SerializeField] float _shotsPerSecond;

    // Player input values
    Vector2 _moveInput;
    Vector2 _lookInput;
    Vector2 _mouseLookPosition;

    Rigidbody2D _rigidbody2D;
    Camera _mainCamera;
    float _timeSinceLastShot;
    float _continuousFireRate;

    bool _useMouseLook = false;

    void ResetTimeSinceLastShot() { _timeSinceLastShot = _continuousFireRate; }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;
        _continuousFireRate = 1.0f / _shotsPerSecond;

        // Initialize "time since last shot" to the fire rate, so there is no delay on the very first shot
        ResetTimeSinceLastShot();
    }

    void FixedUpdate()
    {
        // Update Movement
        if(!_moveInput.Equals(Vector2.zero))
        {
            Vector2 movementDirection = _moveInput;
            Vector2 newPosition = _rigidbody2D.position + movementDirection * _moveSpeed * Time.fixedDeltaTime;
            _rigidbody2D.MovePosition(newPosition);
        }

        // Update Look
        if(!_lookInput.Equals(Vector2.zero))
        {
            Vector3 cross = Vector3.Cross(Vector2.up, _lookInput);
            float flipValue = cross.z < 0.0f ? -1.0f : 1.0f;
            float rotateAngle = Vector2.Angle(Vector2.up, _lookInput) * flipValue;
            _gunRotationPoint.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);

            // If "Right Stick Continuous Fire" is enabled, then fire bullets as long as the right stick is held in any direction (e.g. like Smash TV)
            if(_rightStickContinuousFire)
            {
                _timeSinceLastShot += Time.fixedDeltaTime;
                if(_timeSinceLastShot >= _continuousFireRate)
                {
                    ProjectileController.Instance.SpawnProjectile(_gunFirepoint.position, _gunRotationPoint.rotation);
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
                _gunRotationPoint.rotation = Quaternion.Euler(0.0f, 0.0f, rotateAngle);
            }
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
        // Always fire the first bullet straight in front of the barrel
        ProjectileController.Instance.SpawnProjectile(_gunFirepoint.position, _gunRotationPoint.rotation);
    }
}

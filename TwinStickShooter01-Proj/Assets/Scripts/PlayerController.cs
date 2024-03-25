using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float _moveSpeed;
    Vector2 _moveInput;
    Rigidbody2D _rigidBody2D;

    void Start()
    {
        _rigidBody2D = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        // Update Movement
        if(!_moveInput.Equals(Vector2.zero))
        {
            Vector2 movementDirection = _moveInput;
            Vector2 newPosition = _rigidBody2D.position + movementDirection * _moveSpeed * Time.fixedDeltaTime;
            _rigidBody2D.MovePosition(newPosition);
        }
    }

    void OnMove(InputValue inputValue)
    {
        _moveInput = inputValue.Get<Vector2>();
    }

    void OnLook(InputValue inputValue)
    {
        //Debug.Log("OnLook - " + gameObject.name);
    }

    void OnFire(InputValue inputValue)
    {
        Debug.Log("OnFire - " + gameObject.name);
    }
}

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public abstract class ProjectileBase : MonoBehaviour
{
    Rigidbody2D _rigidbody2D;
    float _moveSpeed;
    float _lifetimeSeconds;
    float _timeAlive;
    public float Damage { get; protected set; }

    public void Init(float moveSpeed, float lifetimeSeconds, float damage)
    {
        _moveSpeed = moveSpeed;
        _lifetimeSeconds = lifetimeSeconds;
        Damage = damage;
    }

    void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _timeAlive = 0.0f;

        if(Mathf.Approximately(_moveSpeed, 0.0f))
        {
            Debug.LogWarning(GetType().Name + ".Start - _movementSpeed is zero (which means it likely has not been set in the Weapon or Init has not been called");
        }

        if(Mathf.Approximately(_lifetimeSeconds, 0.0f))
        {
            Debug.LogWarning(GetType().Name + ".Start - _lifeTimeSeconds is zero (which means it likely has not been set in the Weapon or Init has not been called");
        }
    }

    protected virtual void FixedUpdate()
    {
        // By default, move directly forward (2D up) direction
        // Use the "Up" vector as that is actually the forward vector in Unity 2D (Note: "forward" refers to the Z direction, i.e. in the camera facing direction)
        Vector2 movementDirection = _rigidbody2D.transform.up;
        Vector2 newPos = _rigidbody2D.position + movementDirection * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody2D.MovePosition(newPos);

        // Destroy owning GameObject if time alive has exceeded the lifetime
        _timeAlive += Time.fixedDeltaTime;
        if(_timeAlive >= _lifetimeSeconds)
        {
            Destroy(gameObject);
        }
    }

    public virtual void HandleCollisionWithEnemy()
    {
        // Just destroy the projectile
        Destroy(gameObject);
    }
}

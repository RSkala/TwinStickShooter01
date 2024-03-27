using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyBase : MonoBehaviour
{
    [SerializeField] float _maxHealth;
    [SerializeField] float _moveSpeed;

    Rigidbody2D _rigidbody2D;
    Collider2D _collider2D;
    SpriteRenderer _spriteRenderer;

    float _currentHealth;

    GameObject _target;
    Rigidbody2D _targetRigidbody2D;

    protected virtual void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

        _collider2D = GetComponent<Collider2D>();
        _collider2D.isTrigger = true;

        _spriteRenderer = GetComponent<SpriteRenderer>();

        _currentHealth = _maxHealth;

        // TEST: For now, just use the PlayerCharacter in the scene.
        SetTarget(GameObject.Find("PlayerCharacter"));
    }

    protected virtual void FixedUpdate()
    {
        if(_target == null)
        {
            return;
        }

        // Move toward the target
        Vector2 dirToTarget = (_targetRigidbody2D.position - _rigidbody2D.position).normalized;
        Vector2 moveDirection = dirToTarget * _moveSpeed * Time.fixedDeltaTime;
        Vector2 newPos = moveDirection + _rigidbody2D.position;
        _rigidbody2D.MovePosition(newPos);

        // Change facing depending on movement direction. Easiest way is just to check the X in the move direction.
        _spriteRenderer.flipX = moveDirection.x < 0.0f;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(GetType().Name + ".OnTriggerEnter - " + gameObject.name + ", other: " + other.gameObject.name);

        ProjectileBase projectile = other.gameObject.GetComponent<ProjectileBase>();
        if(projectile != null)
        {
            // This was a projectile. Kill this enemy. RKS TODO: Check instigator and damage amount
            Destroy(gameObject);

            // Handle projectile collision
            projectile.HandleCollisionWithEnemy();
        }

        // RKS TODO: Check melee weapon
    }

    public virtual void SetTarget(GameObject target)
    {
        _target = target;
        _targetRigidbody2D = _target.GetComponent<Rigidbody2D>();
    }
}

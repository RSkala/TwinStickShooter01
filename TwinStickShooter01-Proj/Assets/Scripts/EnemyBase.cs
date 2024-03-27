using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyBase : MonoBehaviour
{
    [SerializeField] float _maxHealth;
    [SerializeField] float _moveSpeed;

    Rigidbody2D _rigidbody2D;
    Collider2D _collider2D;
    float _currentHealth;

    protected virtual void Start()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

        _collider2D = GetComponent<Collider2D>();
        _collider2D.isTrigger = true;

        _currentHealth = _maxHealth;
    }

    protected virtual void FixedUpdate()
    {

    }

    protected virtual void OnTriggerEnter2D()
    {
        //Debug.Log(GetType().Name + ".OnTriggerEnter - " + gameObject.name);

        // TEST: Just destroy the enemy
        Destroy(gameObject);
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D), typeof(Collider2D))]
public class EnemyBase : MonoBehaviour
{
    [SerializeField] float _maxHealth;
    [SerializeField] float _moveSpeed;
    [SerializeField] protected float _damage; // Damage to deal to the player
    [SerializeField] int _pointValue; // Point value when player kills this enemy
    [SerializeField] protected ParticleSystem _deathParticlePrefab;

    // Whether or not to use navigation. Some enemies may ignore navigation (e.g. flyers, ghosts, etc)
    [SerializeField] protected bool _useNavigation = true;

    Rigidbody2D _rigidbody2D;
    Collider2D _collider2D;
    SpriteRenderer _spriteRenderer;
    NavMeshAgent _navMeshAgent;

    // The enemy's current health. Kill when health reaches zero
    float _currentHealth;

    protected GameObject _target;
    protected Rigidbody2D _targetRigidbody2D;

    protected virtual void Start()
    {
        // -------------------------------------------
        // Experimental - NavMesh Movement
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updateRotation = false;
        _navMeshAgent.updateUpAxis = false;
        _navMeshAgent.speed = _moveSpeed;
        _navMeshAgent.autoBraking = false;
        // -------------------------------------------

        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;

        _collider2D = GetComponent<Collider2D>();
        _collider2D.isTrigger = true;

        _spriteRenderer = GetComponent<SpriteRenderer>();

        _currentHealth = _maxHealth;

        // TEST: For now, just use the PlayerCharacter in the scene.
        SetTarget(PlayerController.CurrentPlayerController.gameObject);

        // DEBUG
        //Debug_StartNavigation();
    }

    static int kUsingNavigationCount = 0;
    void Debug_StartNavigation()
    {
        // Test: Have every other enemy use navigation, so I can see how navigation affects movement speed
        _useNavigation = ++kUsingNavigationCount % 2 == 0;
        if(_useNavigation)
        {
            _spriteRenderer.color = Color.red;
        }
        else
        {
            _navMeshAgent.enabled = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        if(_target == null)
        {
            return;
        }

        // Move toward the target
        Vector2 dirToTarget = (_targetRigidbody2D.position - _rigidbody2D.position).normalized;

        // Enemy movement depends on whether navigation is active or not. In the future, some enemies may ignore navigation.
        if(_useNavigation)
        {
            _navMeshAgent.SetDestination(_targetRigidbody2D.position);
        }
        else
        {
            Vector2 moveDirection = dirToTarget * _moveSpeed * Time.fixedDeltaTime;
            Vector2 newPos = moveDirection + _rigidbody2D.position;
            _rigidbody2D.MovePosition(newPos);   
        }

        // Change facing depending on movement direction. Easiest way is just to check the X in the move direction.
        //_spriteRenderer.flipX = moveDirection.x < 0.0f;
        _spriteRenderer.flipX = dirToTarget.x < 0.0f;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if(other.gameObject.TryGetComponent<PlayerController>(out var playerController))
        {
            // This enemy has touched the player. Deal damage to the player.
            playerController.DealDamage(_damage);
        }

        ProjectileBase projectile = other.gameObject.GetComponent<ProjectileBase>();
        if(projectile != null)
        {
            // Handle projectile collision
            projectile.HandleCollisionWithEnemy();

            // This was a projectile. Deal damage to this enemy. RKS TODO: Check instigator
            DealDamage(projectile.Damage);
        }

        // Check melee weapon
        //WeaponMeleeBase meleeWeapon = other.gameObject.GetComponent<WeaponMeleeBase>();
        WeaponMeleeBase meleeWeapon = other.gameObject.GetComponentInParent<WeaponMeleeBase>(); // Currently the melee weapon component is on the rotator
        if(meleeWeapon != null)
        {
            // This was a melee weapon. Damage this enemy.
            //Debug.Log("Dealing " + meleeWeapon.MeleeDamage + " Melee Damage to " + gameObject.name);
            DealDamage(meleeWeapon.MeleeDamage);
        }
    }

    public virtual void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("OnCollisionEnter2D - " + gameObject.name + " , collision: " + collision.gameObject.name);
    }

    public virtual void SetTarget(GameObject target)
    {
        _target = target;
        _targetRigidbody2D = _target.GetComponent<Rigidbody2D>();
    }

    protected virtual void PlayDeathSound() { }
    protected virtual void KillEnemy()
    {
        // For now, just destroy the gameobject
        Destroy(gameObject);

        // Play Death Sound
        PlayDeathSound();

        // Play Death Particle, if this enemy has one
        if(_deathParticlePrefab != null)
        {
            ParticleSystem deathParticle = GameObject.Instantiate(_deathParticlePrefab, _rigidbody2D.position, Quaternion.identity);
            deathParticle.Play();
        }

        // Update player stats
        GameManager.Instance.UpdateTotalEnemiesKilledAndPoints(_pointValue);
    }

    public void DealDamage(float damage)
    {
        // Subtract the damage from current health and ensure it doesn't go below zero
        _currentHealth = Mathf.Max(_currentHealth - damage, 0.0f);

        // Check current health and kill enemy if zero health.
        if(_currentHealth <= 0.0f)
        {
            KillEnemy();
        }
        else
        {
            StartCoroutine(DamageHitFlash());
        }
    }

    IEnumerator DamageHitFlash()
    {
        // This enemy was damaged but not killed. Show damage flash.
        _spriteRenderer.color = GameManager.Instance.EnemyDamageFlashColor;
        yield return new WaitForSeconds(Time.deltaTime * GameManager.Instance.DamageFlashFramesToWait);
        _spriteRenderer.color = Color.white; // On next frame/frames, return to original color
    }
}

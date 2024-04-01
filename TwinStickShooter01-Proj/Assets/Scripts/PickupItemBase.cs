using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public abstract class PickupItemBase : MonoBehaviour
{
    [Tooltip("What should happen to the scene pickup item when a player picks this item up")]
    [SerializeField] protected PickupBehavior _pickupBehavior = PickupBehavior.Destroy;
    
    protected SpriteRenderer _spriteRenderer;
    protected Collider2D _collider2D;
    
    protected bool _movingTowardUI = false;
    Vector2 _worldUIPosition;

    protected enum PickupBehavior
    {
        Destroy, // Remove from the scene on pickup
        MoveToPlayerHUD, // Move to the HUD of the player that picked up this item
    }

    protected virtual void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _collider2D = GetComponent<Collider2D>();
        _collider2D.isTrigger = true;
    }

    protected virtual void Update()
    {
        if(_movingTowardUI)
        {
            float movementAmount = GameManager.Instance.PickupItemUIMoveSpeed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(transform.position, _worldUIPosition, movementAmount);
            if(Vector2.Distance(transform.position, _worldUIPosition) < Mathf.Epsilon)
            {
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        // Only a player should be able to pick up an item (for now). RKS TODO: Restrict this in the layers.
        PlayerController enteredPlayer = other.gameObject.GetComponent<PlayerController>();
        if(enteredPlayer != null)
        {
            OnPlayerPickedUp(enteredPlayer);
        }
    }

    protected virtual void OnPlayerPickedUp(PlayerController enteredPlayer)
    {
        switch(_pickupBehavior)
        {
            case PickupBehavior.Destroy:
                Destroy(gameObject);
                break;

            case PickupBehavior.MoveToPlayerHUD:
                _movingTowardUI = true;
                _worldUIPosition = GameManager.Instance.PickupItemUIPosition.position;
                break;

            default:
                break;
        }
    }
}

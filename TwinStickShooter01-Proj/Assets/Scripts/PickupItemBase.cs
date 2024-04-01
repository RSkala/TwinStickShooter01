using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public abstract class PickupItemBase : MonoBehaviour
{
    [SerializeField] protected bool _destroyOnPickup = true;
    
    protected SpriteRenderer _spriteRenderer;
    protected Collider2D _collider2D;

    protected virtual void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        _collider2D = GetComponent<Collider2D>();
        _collider2D.isTrigger = true;
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
        // Remove this pickup item from the scene, if marked to do so.
        if(_destroyOnPickup)
        {
            Destroy(gameObject);
        }
    }
}

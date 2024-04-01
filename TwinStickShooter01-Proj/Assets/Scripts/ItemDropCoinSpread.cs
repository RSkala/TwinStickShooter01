using UnityEngine;

public class ItemDropCoinSpread : ItemDropBase
{
    [Header("ItemDropCoinSpread Fields")]
    [SerializeField] float _spreadRadius;
    [SerializeField, Range(1, 1000)] int _minCoinsToDrop;
    [SerializeField, Range(1, 1000)] int _maxCoinsToDrop;
    [SerializeField] float _timeBetweenDrops = 0.0f;

    float _itemDropTimer;
    int _numCoinsToDrop;
    int _numCoinsDropped;

    protected override void Start()
    {
        if(_minCoinsToDrop > _maxCoinsToDrop)
        {
            Debug.LogWarning("ItemDropCoinSpread.Start - _minCoinsToDrop > _maxCoinsToDrop");
        }
        
        DropItems();
    }

    protected override void Update()
    {
        if(Mathf.Approximately(_timeBetweenDrops, 0.0f))
        {
            // Drop all coins at once in the same frame
            for(int i = 0; i < _numCoinsToDrop; ++i)
            {
                SpawnCoinDrop();
            }
        }
        else
        {
            // Drop coins with a delay
            _itemDropTimer += Time.deltaTime;
            if(_itemDropTimer >= _timeBetweenDrops)
            {
                SpawnCoinDrop();
                _itemDropTimer = 0.0f;
            }
        }
        
        if(_numCoinsDropped >= _numCoinsToDrop)
        {
            // Remove this gameObject from the scene
            Destroy(gameObject);
        }
    }

    protected override void DropItems()
    {
        _isDroppingItems = true;
        _numCoinsToDrop = Random.Range(_minCoinsToDrop, _maxCoinsToDrop);
        _numCoinsDropped = 0;
        _itemDropTimer = _timeBetweenDrops;
    }

    Vector2 GetRandomPositionWithinRadius()
    {
        float randomRadius = Random.Range(0.0f, _spreadRadius);
        float randomAngle = Random.Range(0.0f, 360.0f);

        float xPos = randomRadius * Mathf.Cos(randomAngle * Mathf.Deg2Rad);
        float yPos = randomRadius * Mathf.Sin(randomAngle * Mathf.Deg2Rad);

        Vector2 coinPos = new Vector2(xPos, yPos);
        return coinPos;
    }

    void SpawnCoinDrop()
    {
        // Get a random position within the given radius
        Vector2 coinDropPositionLocal = GetRandomPositionWithinRadius();

        // Offset this position by the position of this object
        Vector2 coinDropPosition = new Vector2(transform.position.x, transform.position.y) + coinDropPositionLocal;

        // Create the new Coin Drop and increment the number created
        GameObject.Instantiate(_itemDropPrefab, coinDropPosition, Quaternion.identity);
        _numCoinsDropped++;
    }
}

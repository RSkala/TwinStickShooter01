using UnityEngine;

public class ItemDropCoinSpiral : ItemDropBase
{
    [Header("ItemDropCoinSpiral Fields")]
    [SerializeField] float _dropTime; // How long this stays active
    [SerializeField] float _timeBetweenDrops; // Number of seconds between each coin drop
    [SerializeField] float _radiusGrowthRate; // Units per second
    [SerializeField] float _angleGrowthRate; // Degrees per second
    [SerializeField] RotationDirection _rotationDirection; // Direction to rotate the spiral

    float _itemDropTimer;
    float _itemDropActiveTime;
    float _currentRadius;
    float _currentAngle;

    enum RotationDirection
    {
        Clockwise,
        Counterclockwise
    }

    protected override void Start()
    {
        DropItems();
    }

    protected override void Update()
    {
        _itemDropTimer += Time.deltaTime;
        if(_itemDropTimer >= _timeBetweenDrops)
        {
            SpawnCoinDrop();
            _itemDropTimer = 0.0f;
        }

        _currentRadius += _radiusGrowthRate * Time.deltaTime;
        _currentAngle += _angleGrowthRate * Time.deltaTime;

        //Debug.Log("_currentRadius: " + _currentRadius + ", _currentAngle: " + _currentAngle); // TODO: Add a circle for debugging

        _itemDropActiveTime += Time.deltaTime;
        if(_itemDropActiveTime > _dropTime)
        {
            Destroy(gameObject);
        }
    }

    protected override void DropItems()
    {
        _isDroppingItems = true;
        _currentRadius = 0.0f;
        _currentAngle = 0.0f;
        _itemDropTimer = _timeBetweenDrops;
        _itemDropActiveTime = 0.0f;
    }

    Vector2 GetPositionAtCurrentAngleAndRadius()
    {
        float rotationDirMultiple = _rotationDirection == RotationDirection.Clockwise ? -1.0f : 1.0f; // Negative rotation angle is to the right
        float rotationAngle = _currentAngle * rotationDirMultiple;

        float xPos = _currentRadius * Mathf.Cos(rotationAngle * Mathf.Deg2Rad);
        float yPos = _currentRadius * Mathf.Sin(rotationAngle * Mathf.Deg2Rad);

        Vector2 coinPos = new Vector2(xPos, yPos);
        return coinPos;
    }

    void SpawnCoinDrop()
    {
        // Get a random position within the given radius
        Vector2 coinDropPositionLocal = GetPositionAtCurrentAngleAndRadius();

        // Offset this position by the position of this object
        Vector2 coinDropPosition = new Vector2(transform.position.x, transform.position.y) + coinDropPositionLocal;

        // Create the new Coin Drop and increment the number created
        GameObject.Instantiate(_itemDropPrefab, coinDropPosition, Quaternion.identity);
    }
}

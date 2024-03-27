using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] EnemyBase _enemyToSpawnPrefab;
    [SerializeField] float _timeBetweenSpawns;
    [SerializeField] bool _useRandomSpawnDelay = true;

    float _timeSinceLastSpawn;

    void Start()
    {
        _timeSinceLastSpawn = _useRandomSpawnDelay ? Random.Range(0.0f, _timeBetweenSpawns) : 0.0f;
        Debug.Log("_timeSinceLastSpawn: " + _timeSinceLastSpawn + " - " + gameObject.name);
    }

    void Update()
    {
        _timeSinceLastSpawn += Time.deltaTime;
        if(_timeSinceLastSpawn >= _timeBetweenSpawns)
        {
            GameObject.Instantiate(_enemyToSpawnPrefab, transform.position, Quaternion.identity);
            _timeSinceLastSpawn = 0.0f;
        }   
    }
}

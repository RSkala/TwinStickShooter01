using System.Reflection;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Tooltip("No enemies will be spawned while this is true")]
    [field:SerializeField] public bool DisableEnemySpawning { get; private set; }

    [Tooltip("Default color to flash on a damaged enemy")]
    [field:SerializeField] public Color EnemyDamageFlashColor { get; private set; } // RKS TODO: This should be a material on the enemies

    [Tooltip("After damage flash, number of frames to wait before returning to original color")]
    [field:SerializeField] public int DamageFlashFramesToWait { get; private set; }

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if(Instance != null)
        {
            Debug.LogError(GetType().ToString() + "." + MethodBase.GetCurrentMethod().Name + " - Singleton Instance already exists!");
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    void Start()
    {
        if(DisableEnemySpawning)
        {
            Debug.LogWarning("Enemy spawning is DISABLED! Make sure this is what you wanted to do before looking at any spawning issues.");
        }
    }
}

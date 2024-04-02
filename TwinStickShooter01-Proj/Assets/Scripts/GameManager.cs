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

    [Tooltip("Position in the world to translate a pickedup item to (i.e. where the UI is located visually to the user)")]
    [field:SerializeField] public Transform PickupItemUIPosition { get; private set; }

    [Tooltip("Speed at which a picked up item should move towards the player's UI (when marked to do so)")]
    [field:SerializeField] public float PickupItemUIMoveSpeed { get; private set; }

    public static GameManager Instance { get; private set; }

    public int TotalEnemiesKilled { get; private set; }
    public int TotalPoints { get; private set; }

    public enum SpriteFacingDirection
    {
        Invalid,
        Right,
        Left
    }

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

    public void UpdateTotalEnemiesKilledAndPoints(int killedEnemyPointValue)
    {
        TotalEnemiesKilled++;
        TotalPoints += killedEnemyPointValue;
    }
}

using System.Reflection;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Tooltip("Default color to flash on a damaged enemy")]
    [field:SerializeField] public Color EnemyDamageFlashColor { get; private set; } // RKS TODO: This should be a material on the enemies
    [field:SerializeField] public int DamageFlashFramesToWait { get; private set; } // After damage flash, number of frames to wait before returning to original color

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
}

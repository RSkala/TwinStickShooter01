using System.Reflection;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] ProjectilePistolBullet _pistolBulletPrefab;
    [SerializeField] ProjectileArrow _projectileArrowPrefab;

    public static ProjectileController Instance { get; private set; }

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Debug.LogWarning(GetType().ToString() + "." + MethodBase.GetCurrentMethod().Name + " - Instance already exists!");
            Destroy(Instance.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {

    }

    public void SpawnProjectile(Vector2 projectilePosition, Quaternion projectileRotation)
    {
        // RKS TODO: Allocate references on Start for pooling
        //ProjectilePistolBullet newPistolBullet = GameObject.Instantiate(_pistolBulletPrefab, projectilePosition, projectileRotation);
        ProjectileArrow newPistolBullet = GameObject.Instantiate(_projectileArrowPrefab, projectilePosition, projectileRotation);
    }
}

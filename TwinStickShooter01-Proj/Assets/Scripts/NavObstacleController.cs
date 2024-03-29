using UnityEngine;

public class NavObstacleController : MonoBehaviour
{
    [SerializeField] bool _hideObstaclesInGame;
    [SerializeField] SpriteRenderer[] _obstacleSprites;

    void Start()
    {
        foreach(SpriteRenderer obstacleSprite in _obstacleSprites)
        {
            obstacleSprite.enabled = false;
        }
    }
}

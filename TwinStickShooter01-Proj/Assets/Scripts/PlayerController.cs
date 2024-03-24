using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    void Start()
    {
        
    }

    void FixedUpdate()
    {

    }

    void OnMove(InputValue inputValue)
    {
        Debug.Log("OnMove - " + gameObject.name);
    }

    void OnLook(InputValue inputValue)
    {
        Debug.Log("OnLook - " + gameObject.name);
    }

    void OnFire(InputValue inputValue)
    {
        Debug.Log("OnFire - " + gameObject.name);
    }
}

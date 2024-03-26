using UnityEngine;
using UnityEngine.Events;

public abstract class WeaponMeleeBase : MonoBehaviour
{
    [HideInInspector] public UnityEvent eventMeleeAttackStart;
    [HideInInspector] public UnityEvent eventMeleeAttackEnd;

    SpriteRenderer _spriteRenderer;

    protected virtual void Awake()
    {
        Debug.Log(GetType().ToString() + ".Awake - " + gameObject.name);
    }

    protected virtual void Start()
    {
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    protected virtual void OnDestroy()
    {
        eventMeleeAttackStart.RemoveAllListeners();
        eventMeleeAttackEnd.RemoveAllListeners();
    }

    void MeleeAttackAnimStart()
    {
        Debug.Log(GetType().ToString() + ".MeleeAttackAnimStart - " + gameObject.name);
    }

    void MeleeAttackAnimEnd()
    {
        // The melee attack animation has ended. Notify listeners and hide the melee weapon gameobject.
        eventMeleeAttackEnd.Invoke();
        SetActive(false);
    }

    public void SetActive(bool active) { gameObject.SetActive(active); }
}

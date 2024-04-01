using System;
using System.Reflection;
using UnityEngine;

public abstract class ItemDropBase : MonoBehaviour
{
    [Header("ItemDropBase Fields")]
    [SerializeField] protected GameObject _itemDropPrefab;

    protected bool _isDroppingItems = false;

    protected virtual void Start() {}
    protected virtual void Update() {}
    protected virtual void DropItems() => throw new NotImplementedException(GetType().Name + "." + MethodBase.GetCurrentMethod().Name + " not implemented.");
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryComponent : MonoBehaviour
{

    protected RectTransform _rt;

    protected virtual void Awake()
    {
        _rt = this.GetComponent<RectTransform>();

    }

    public abstract void ChangeRectTransform(Vector2 size);
}

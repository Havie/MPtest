using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventoryComponent : MonoBehaviour
{

    protected RectTransform _rt;

    protected virtual void Awake()
    {
        VerifyRT();

    }

    protected bool VerifyRT()
    {
        if(_rt==null)
            _rt = this.GetComponent<RectTransform>();
        return _rt != null;
    }

    public abstract void ChangeRectTransform(Vector2 size);
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryViewport : InventoryComponent
{


    public override void ChangeRectTransform(Vector2 size)
    {
        //Debug.Log("[InventoryViewport] Changed BG size to " + size);
        if(_rt)
        _rt.sizeDelta = size;
        else
            Debug.Log($"<color=yellow> why is rt missing for </color> {this.gameObject.name}");

    }
}

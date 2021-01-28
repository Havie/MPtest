using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryViewport : InventoryComponent
{


    public override void ChangeRectTransform(Vector2 size)
    {
        //Debug.Log("[InventoryViewport] Changed BG size to " + size);
        _rt.sizeDelta = size;
    }
}

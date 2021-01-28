using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySendButton : InventoryComponent
{

    public override void ChangeRectTransform(Vector2 size)
    {
        _rt.sizeDelta = new Vector2(size.x, _rt.sizeDelta.y);
    }
}

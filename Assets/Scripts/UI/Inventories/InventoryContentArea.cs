using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryContentArea : InventoryComponent
{

    public override void ChangeRectTransform(Vector2 size)
    {
     //   Debug.Log("[InventoryContentArea] Changed BG size to " + size);
     if(VerifyRT()) ///Can be null if ur in kitting and it was never enabled?
        _rt.sizeDelta = size;
     }
}

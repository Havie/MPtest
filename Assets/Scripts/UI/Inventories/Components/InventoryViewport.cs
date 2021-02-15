using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryViewport : InventoryComponent
{

    [SerializeField] protected InventoryScrollbar _scrollbarVert;
    [SerializeField] protected InventoryScrollbar _scrollbarHoriz;


    public override void ChangeRectTransform(Vector2 size)
    {
        //Debug.Log("[InventoryViewport] Changed BG size to " + size);
        if(VerifyRT())
        _rt.sizeDelta = size;
        else
            Debug.Log($"<color=yellow> why is rt missing for </color> {this.gameObject.name}");

        if (_scrollbarVert)
            _scrollbarVert.ChangeRectTransform(size);
        if (_scrollbarHoriz)
            _scrollbarHoriz.ChangeRectTransform(size);
    }
}

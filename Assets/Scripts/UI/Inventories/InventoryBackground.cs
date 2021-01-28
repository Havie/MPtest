using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBackground : InventoryComponent
{

    [SerializeField] RectTransform _optionalChildrenItems =default;
    [SerializeField] Transform _optionalSendButton = default;



    public override void ChangeRectTransform(Vector2 size)
    {
        //Debug.Log("Changed BG size to " + size);
        _rt.sizeDelta = size;

        if(_optionalChildrenItems)
            _optionalChildrenItems.sizeDelta = new Vector2(size.x, _optionalChildrenItems.sizeDelta.y);

        if (_optionalSendButton)
        {

            _optionalSendButton.localPosition = transform.localPosition  - new Vector3(0, size.y, 0);
        }
    }

    public Vector2 GetRectSize() => _rt.sizeDelta;

}

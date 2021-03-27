using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class InventoryScrollbar : InventoryComponent
{
    public enum eScrollType { VERTICAL, HORIZONTAL }
    [SerializeField] eScrollType _type;

    public override void ChangeRectTransform(Vector2 size)
    {
        if (VerifyRT())
        {
            if (_type == eScrollType.VERTICAL)
                _rt.sizeDelta = new Vector2(_rt.sizeDelta.x, size.y);
            else if (_type == eScrollType.HORIZONTAL)
                _rt.sizeDelta = new Vector2(size.x, _rt.sizeDelta.y);
        }
        else
        {
            Debug.Log($"<color=yellow> why is rt missing for </color> {this.gameObject.name}");
            return;
        }


    }
}

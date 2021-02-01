using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBackground : InventoryComponent
{

    [SerializeField] RectTransform _optionalChildrenItems = default;
    [SerializeField] protected InventoryMask _mask;


    //[SerializeField] Transform _optionalSendButton = default;
    void Start()
    {
        if(_mask && VerifyRT())
            _mask.CalculateReducedSize(_rt.sizeDelta.y);
    }

    public override void ChangeRectTransform(Vector2 size)
    {


        //Debug.Log("Changed BG size to " + size);
        if (VerifyRT())
        {
            _rt.sizeDelta = size; ///Make sure this called before Mask
            if (_mask)
                _mask.ChangeRectTransform(size);
        }
        else
            Debug.Log($"<color=yellow> why is rt missing for </color> {this.gameObject.name}");

        if (_optionalChildrenItems)
            _optionalChildrenItems.sizeDelta = new Vector2(size.x, _optionalChildrenItems.sizeDelta.y);

        ///If you child the button to this object, and set its pivot on rectTrans to be bottom right,
        ///it will do this automatically:
        //if (_optionalSendButton)
        //{

        //    _optionalSendButton.localPosition = transform.localPosition  - new Vector3(0, size.y, 0);
        //}
    }

    public Vector2 GetRectSize() => _rt.sizeDelta;

}

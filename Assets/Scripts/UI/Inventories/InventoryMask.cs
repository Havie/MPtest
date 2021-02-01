using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryMask : InventoryComponent
{
    [SerializeField] InventoryBackground _bg = default;
    [SerializeField] InventoryViewport _viewPort = default;
    float _reducedSizeY;
    protected override void Awake()
    {
        base.Awake();
        if (_viewPort == null)
            _viewPort = this.GetComponentInChildren<InventoryViewport>();
    }

    void Start()
    {
        CalculateReducedSize();
    }

    private void CalculateReducedSize() ///This cant happen if the BG has been previously set/changed before Start, becuz of this UIManager now needs to toggle the inventory on/off to let load
    {
        if (VerifyRT())
            _reducedSizeY = _bg.GetRectSize().y - _rt.sizeDelta.y;
    }

    public override void ChangeRectTransform(Vector2 size)
    {

        ///Mask needs to be slightly smalller than content/bg
        Vector2 reducedSize = new Vector2(size.x, size.y - _reducedSizeY);

        //Debug.Log($"[InventoryMask] Changed InventoryMask size to<color=green> {reducedSize}</color> vs: actual: <color=red> {size}</color>  becuz <color=yellow>_Y={_reducedSizeY} </color>  _bgRectSize.y={_bg.GetRectSize().y} vs _rtsizeDelta.y{_rt.sizeDelta.y}");


        if (VerifyRT())
            _rt.sizeDelta = reducedSize;
        else
            Debug.Log($"<color=yellow> why is rt missing for </color> {this.gameObject.name}");


        if (_viewPort)
            _viewPort.ChangeRectTransform(reducedSize);

    }
}

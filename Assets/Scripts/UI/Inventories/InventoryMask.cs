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
        _reducedSizeY = _bg.GetRectSize().y - _rt.sizeDelta.y;
    }

    public override void ChangeRectTransform(Vector2 size)
    {
        //Debug.Log("[InventoryMask] Changed BG size to " + size);
        ///Mask needs to be slightly smalller than content/bg


        Vector2 reducedSize = new Vector2(size.x, size.y - _reducedSizeY);

        if(_rt)
        _rt.sizeDelta = reducedSize;
        else
            Debug.Log($"<color=yellow> why is rt missing for </color> {this.gameObject.name}");


        if (_viewPort)
            _viewPort.ChangeRectTransform(reducedSize);
    }
}

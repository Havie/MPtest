using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryContentArea : InventoryComponent
{
    [SerializeField] InventoryBackground _bg = default;
    float _reducedSizeY;

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

        Vector2 reducedSize = new Vector2(size.x, size.y - _reducedSizeY);

        //   Debug.Log("[InventoryContentArea] Changed BG size to " + size);
        if (VerifyRT()) ///Can be null if ur in kitting and it was never enabled?
            _rt.sizeDelta = reducedSize;
    }

    public float GetReducedYSize => _reducedSizeY;
}

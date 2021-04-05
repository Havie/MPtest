using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDeadZone : InventoryComponent
{
    [Header("Make Sure the Image component of the deadzone is on but alpha is 0, otherwise it wont raycast")]
    [Tooltip("The location where an item will reset to if placed in a deadzone")]
    [SerializeField] Transform _safePlace = default;

   public Transform GetSafePosition { get; private set; }



    void Start()
    {
        if (_safePlace)
            GetSafePosition = _safePlace.transform;
        else
            UIManager.DebugLogWarning($"saFeplace not set up for {this.gameObject.transform.parent.gameObject.name}");
    }


    public override void ChangeRectTransform(Vector2 size)
    {
        if (VerifyRT())
            _rt.sizeDelta = size;
    }

    public void TryScaleSizeWithInventory(Vector2 parentSize)
    {
        Debug.Log($"Set deadzone size = {parentSize}");
        if (VerifyRT())
        {
            _rt.sizeDelta = parentSize;
        }
    }
}

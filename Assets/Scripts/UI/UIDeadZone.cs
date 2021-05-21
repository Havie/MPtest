using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDeadZone : InventoryComponent
{
    [Header("Make Sure the Image component of the deadzone is on but alpha is 0, otherwise it wont raycast")]
    [Tooltip("The location where an item will reset to if placed in a deadzone")]
    [SerializeField] Transform _safePlace = default;
    [SerializeField] float _extraPaddingX = 0;
    [SerializeField] float _extraPaddingY = 0;

    [Header("Inventory")] ///This is sort of hacky, but cant assign an Interface in inspector, and no shared base class
    [SerializeField] Transform _inventoryGameObject;
    private IInventoryManager _relatedInventory;

    public Transform GetSafePosition { get; private set; }



    void Start()
    {
        if (_safePlace)
            GetSafePosition = _safePlace.transform;
        else
            UIManager.DebugLogWarning($"saFeplace not set up for {this.gameObject.transform.parent.gameObject.name}");

        _relatedInventory = _inventoryGameObject.GetComponent<IInventoryManager>();
    }

    public bool TryAssignItem(int id, int count, List<QualityData> qualities)
    {
       return _relatedInventory==null ?  false :  _relatedInventory.TryAssignItem(id, count, qualities);
    }
    public bool TryAssignItem(ObjectController oc, int count)
    {
        if (oc == null)
            return false;

        /// get ID from controller
        int id = (int)oc._myID;

        ///Get Object Quality from controller
        QualityOverall overallQuality = oc.GetComponent<QualityOverall>();
        if (overallQuality != null)
        {
            List<QualityObject> qualities = overallQuality.Qualities;
            List<QualityData> qualityData = new List<QualityData>();
            foreach (var item in qualities)
            {
                qualityData.Add(QualityConvertor.ConvertToData(item));
            }
            return TryAssignItem(id, count, qualityData);
        }


        return false;
    }


    public override void ChangeRectTransform(Vector2 size)
    {
        if (VerifyRT())
            _rt.sizeDelta = size;
    }

    public void TryScaleSizeWithInventory(Vector2 parentSize)
    {
        //Debug.Log($"Set deadzone size = {parentSize}");
        if (VerifyRT())
        {
            _rt.sizeDelta = new Vector2(parentSize.x + _extraPaddingX, parentSize.y + _extraPaddingY);
        }
    }
}

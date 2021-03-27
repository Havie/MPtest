using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class DefectInventory : UIInventoryManager
{
    [Header("Event")]
    [SerializeField] DefectEvent _defectEvent;

    #region InitalSetup
    protected override void Start()
    {
        if (IsInitalized)
            return;

        base.Start();
        //Debug.LogWarning("(s)SLOTS SIZE=" + _slots.Length);

    }




    /************************************************************************************************************/
    #region batchSizeMethods

    protected override List<int> DetermineWorkStationBatchSize()
    {
        var gm = GameManager.instance;
        int batchSize = gm._batchSize;

        return StationItemParser.ParseItemsAsDefect(batchSize, gm.CurrentWorkStationManager, gm._workStation);
        //return ParseItems(wm, myWS, false) * BATCHSIZE;

    }

    #endregion
    /************************************************************************************************************/

    /**Generates the Inventory with correct dimensions based on Game Settings. */
    protected override void GenerateInventory(List<int> itemIDs)
    {
        _INVENTORYSIZE = itemIDs.Count;
        _slots = new UIInventorySlot[_INVENTORYSIZE];
        IsInitalized = true;
        //Debug.LogError($"{_inventoryType} slotsize ={ _slots.Length}");


        //Debug.Log($"{this.transform.gameObject.name}{_inventoryType}, {_INVENTORYSIZE} resulted in {_xMaxRows}");

        //Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        //cache a conditions for forloop situations
        bool cond = GameManager.instance._autoSend; //used By eInvType.OUT
        WorkStationManager wm =GameManager.Instance.CurrentWorkStationManager;
        WorkStation myWS = GameManager.instance._workStation;
        //getAPrefix for naming our buttons in scene Hierarchy

        _prefix = "Defect";

        //Any slots added after this will be kept track of in an extra list incase we ever want to reset to base amount
        _extraSlots = new List<UIInventorySlot>(); //Instantiated before for loop becuz CreateNewslot uses its Count
        for (int i = 0; i < _INVENTORYSIZE; ++i)
        {
            //Add slot component to our list
            _slots[i] = CreateNewSlot();
            _slots[i].transform.localScale = new Vector3(-1, 1, 1);
        }
        //SetUpBatchOutput(wm, myWS);


    }

    public override void ItemAssigned(UIInventorySlot slot)
    {
        //FireEvent
        _defectEvent.Raise(new DefectWrapper((int)GameManager.instance._workStation._myStation, slot.GetItemID()));

    }



    #endregion
}

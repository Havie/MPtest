
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InInventory : UIInventoryManager
{
    #region InitalSetup


    protected override void Start()
    {
        if (IsInitalized)
            return;

        base.Start();

        if (_INVENTORYSIZE == 0)
        {
            UIManager.HideInInventory();
        }
    }

    /************************************************************************************************************/
    #region batchSizeMethods

    protected override List<int> DetermineWorkStationBatchSize()
    {
        var gm = GameManager.instance;
        _batchSize = gm._batchSize;
        return StationItemParser.ParseItemsAsIN(_batchSize, gm._isStackable, gm.CurrentWorkStationManager, gm._workStation);
       // return ParseItems(wm, myWS, false) * BATCHSIZE;
    }

    private void SetUpStartingItems()
    {
        //ParseItems(GameManager.Instance.CurrentWorkStationManager, GameManager.Instance._workStation, true);

        var gm = GameManager.instance;
        List<int> itemIDs = StationItemParser.ParseItemsAsIN(gm._batchSize, gm._isStackable, gm.CurrentWorkStationManager, gm._workStation);
        if (gm._batchSize==1)
        {
            ///if its pull count should only be one for Kanban
            if (itemIDs.Count != 1)
                Debug.Log($"Pull system item count is : <color=yellow> {itemIDs.Count} </color>, expecting 1");
            AddItemToSlot(itemIDs[0], null, true);
        }
        if (gm.StartWithWIP) ///TODO if pull, show as REQ item
        {
            foreach (int itemID in itemIDs)
            {
                AddItemToSlot(itemID, null, false);
            }
        }
        
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

        ///Determine layout

        //Debug.Log($"{this.transform.gameObject.name}{_inventoryType}, {_INVENTORYSIZE} resulted in {_xMaxRows}");

        ///Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        ///getAPrefix for naming our buttons in scene Hierarchy
        _prefix = "In";


        ///Any slots added after this will be kept track of in an extra list incase we ever want to reset to base amount
        _extraSlots = new List<UIInventorySlot>(); //Instantiated before for loop becuz CreateNewslot uses its Count

        for (int i = 0; i < _INVENTORYSIZE; ++i)
        {
            //Add slot component to our list
            _slots[i] = CreateNewSlot();
        }

        SetUpStartingItems();
    }

    #endregion


    public override void SlotStateChanged(UIInventorySlot slot)
    {
        ///Has to be override cause no way to set the TorF flag on UIInventoryManager parent w 3 stations without an enum and were staying away from enums w inheritance like this
        if (_batchSize == 1)
        {
            ClientSend.Instance.KanbanChanged(true, !slot.GetInUse(), slot.RequiredID, slot.Qualities);
        }
    }

}

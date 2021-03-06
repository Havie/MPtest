﻿
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[DefaultExecutionOrder(10001)] // load late after Injector...
///I am very confused why the IN and OUT inventories load after this by default, and how this one started getting
///called early and now requires a DefaultExecutionOrder +1 after injector
public class StationInventory : UIInventoryManager
{
    #region InitalSetup
    protected override void Start()
    {
        if (IsInitalized)
            return;

        base.Start();

        ///seralize this instead on UIManagerGame
       // UIManagerGame.Instance.SetInventoryStation(this);
    }


    /************************************************************************************************************/
    #region batchSizeMethods

    protected override List<int> DetermineWorkStationBatchSize()
    {
        var gm = GameManager.instance;

        WorkStationManager wm = gm.CurrentWorkStationManager;
        int batchSize = gm._batchSize;
        WorkStation myWS = gm._workStation;
        bool isStackable = gm._isStackable;
       // Debug.Log($"Station { gm._workStation}");
        return StationItemParser.ParseItemsAsStation(batchSize, isStackable, wm, myWS);
    }

    private void SetUpInfiniteItems(WorkStationManager wm, WorkStation myWS, bool isStackable=true)
    {
       
        foreach(var itemID in StationItemParser.ParseItemsAsStation(GameManager.instance._batchSize, isStackable, wm, myWS))
        {
            AssignInfiniteItem(itemID);
        }
    }

    #endregion
    /************************************************************************************************************/

    /**Generates the Inventory with correct dimensions based on Game Settings. */
    protected override void GenerateInventory(List<int> itemIDs)
    {
        //if (NotStackableAndNotKitting())
        if (NotStackableOrKitting())
            return;

        _INVENTORYSIZE = itemIDs.Count;
        _slots = new UIInventorySlot[_INVENTORYSIZE];
        IsInitalized = true;

        //Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        WorkStationManager wm = GameManager.Instance.CurrentWorkStationManager;
        WorkStation myWS = GameManager.Instance._workStation;
        //getAPrefix for naming our buttons in scene Hierarchy
        _prefix = "Station";

        //_extraSlots = new List<UIInventorySlot>(); //Instantiated before for loop becuz CreateNewslot uses its Count
        _canAssignExtraSlots = false;
        for (int i = 0; i < _INVENTORYSIZE; ++i)
        {
            //Add slot component to our list
            _slots[i] = CreateNewSlot();
        }
        SetUpInfiniteItems(wm, myWS);
        //ParseItemList(wm, myWS, true);

        if (_slots.Length == 0)
        {
            Destroy(this.gameObject);
        }
    }


    /// <summary> Kitting no longer gets a station inv since items drop in now </summary>
    private bool NotStackableOrKitting()
    {

        if (!_STACKABLE)
        {
            Destroy(this.gameObject); //good enough for now might need to go higher to parents
            return true;
        }

        //UIManager.DebugLog("Station is stackable so enabling personal inventory, TODO remove these items from calculation of in invetory/send inventory");
        return false;
    }


    /**Determines the size of the content area based on how many items/rows we have. The overall size affects scrolling */
    protected override void SetSizeOfContentArea()
    {
        if (_deadZone)
        {
            ///TODO Set the size of the deadZone:
            var width = _maxCellSize * _INVENTORYSIZE;
            var height = _maxCellSize;
            _deadZone.TryScaleSizeWithInventory(new Vector2(width, height));
        }
        
    }

    public override void SlotStateChanged(UIInventorySlot slot)
    {
        ///override todo nothing, (could count hardcore WIP here someday?)
    }

    private void AssignInfiniteItem(int itemID)
    {
        foreach (UIInventorySlot slot in _slots)
        {
            if (!slot.GetInUse())
            {
                slot.AssignItem(itemID, int.MaxValue, null);
                return;
            }

        }
        foreach (UIInventorySlot slot in _extraSlots)
        {
            if (!slot.GetInUse())
            {
                slot.AssignItem(itemID, int.MaxValue, null);
                return;
            }

        }
        //fell thru so we are full
        Debug.Log("we fell thru");
        UIInventorySlot nSlot = CreateNewSlot();
        nSlot.AssignItem(itemID, int.MaxValue, null);
        _extraSlots.Add(nSlot);
    }


    #endregion
}

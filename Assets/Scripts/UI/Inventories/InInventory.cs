﻿
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

        _inventoryType = eInvType.IN;
        GetGameManagerData();
        if (_INVENTORYSIZE == 0)
            UIManager.HideInInventory();
        else
            GenInventory();


    }

    private void GetGameManagerData()
    {
        _INVENTORYSIZE = DetermineWorkStationBatchSize();
        _STACKABLE = GameManager.Instance._isStackable;
        _ADDCHAOTIC = GameManager.Instance._addChaotic;
        GameManager.Instance.SetInventoryIn(this);
    }


    /************************************************************************************************************/
    #region batchSizeMethods

    private int DetermineWorkStationBatchSize()
    {
        WorkStationManager wm = GameManager.Instance.CurrentWorkStationManager;
        int BATCHSIZE = GameManager.Instance._batchSize;
        //if (BATCHSIZE == 1)
        //    TurnOffScrollBars();
        WorkStation myWS = GameManager.Instance._workStation;

        return ParseItems(wm, myWS, false) * BATCHSIZE;
    }

    protected virtual int ParseItems(WorkStationManager wm, WorkStation myWS, bool AddToSlotOnFind)
    {

        int count = 0;
        int[] stationSequence = getProperSequence(wm, myWS);
        var stationList = wm.GetStationList();
        ///Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        // Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        int BATCHSIZE = GameManager.Instance._batchSize;
        if (BATCHSIZE == 1)
        {
            ///look at the last tasks final items in the station before mine
            if (startingIndex > 1) /// no kitting on pull, so second station
            {
                WorkStation ws = stationList[startingIndex - 1];
                Task lastTask = ws._tasks[ws._tasks.Count - 1];
                // Debug.Log($"# of items at Task:{lastTask} is {lastTask._finalItemID.Count}");
                count += lastTask._finalItemID.Count;
                if (AddToSlotOnFind)
                {
                    foreach (var item in lastTask._finalItemID)
                    {
                        AddItemToSlot((int)item, null, false);
                    }
                }
            }
        }
        else  ///look at all final items for station before me , and the basic items from kitting[1]
        {
            var listItems = FindObjectsAtKittingStation(stationList[1]);
            // Debug.Log("Staring index=+" + startingIndex);
            ///foreach station between us and kitting, if listItem contains a requiredItem, remove it
            if (startingIndex > 2) //1 = kitting
            {
                for (int i = 0; i < startingIndex; i++)
                {
                    WorkStation ws = stationList[i];
                    foreach (Task t in ws._tasks)
                    {
                        foreach (var item in t._requiredItemIDs)
                        {
                            int itemId = (int)item;
                            // Debug.Log($"_requiredItems.. Station::{ws} --> Task::{t}  --> Item{item} #{itemId}");
                            if (listItems.Contains(itemId))
                                listItems.Remove(itemId);
                        }
                        ///were at prior station
                        if (i == startingIndex - 1)
                        {
                            ///Add the final items from station prior to me
                            foreach (var item in t._finalItemID)
                            {
                                int itemId = (int)item;
                                listItems.Add(itemId);
                                // Debug.Log($"_finalItems....Station::{ws} --> Task::{t}  --> Item{item} #{itemId}");

                            }
                        }
                    }
                }
            }
            ///finally we can add what we found
            if (AddToSlotOnFind)
            {
                foreach (var item in listItems)
                {
                    for (int j = 0; j < BATCHSIZE; j++)
                        AddItemToSlot((int)item, null, false);
                }
            }
            count += listItems.Count;

        }
        //Debug.Log($"The # of INV items will be : {count}");
        return count;
    }

    private List<int> FindObjectsAtKittingStation(WorkStation ws)
    {
        if (!ws.isKittingStation())
            Debug.LogError("Wrong order, kitting isnt @ index 1");

        List<int> items = new List<int>();
        foreach (Task t in ws._tasks)
        {
            foreach (var item in t._finalItemID)
                items.Add((int)item);
        }

        return items;
    }

    private void SetUpStartingItems()
    {
        ParseItems(GameManager.Instance.CurrentWorkStationManager, GameManager.Instance._workStation, true);
    }


    #endregion
    /************************************************************************************************************/

    /**Generates the Inventory with correct dimensions based on Game Settings. */
    private void GenInventory()
    {

        _slots = new UIInventorySlot[_INVENTORYSIZE];
        IsInitalized = true;

        //Debug.LogError($"{_inventoryType} slotsize ={ _slots.Length}");

        ///Determine layout
        _xMaxPerRow = _INVENTORYSIZE;
        if (_INVENTORYSIZE > _maxItemsPerRow && _inventoryType != eInvType.STATION)
            _xMaxPerRow = (_INVENTORYSIZE / _maxItemsPerRow) + 1;

        if (_xMaxPerRow > _maxItemsPerRow)
            _xMaxPerRow = _maxItemsPerRow;

        //Debug.Log($"{this.transform.gameObject.name}{_inventoryType}, {_INVENTORYSIZE} resulted in {_xMaxRows}");

        ///Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        ///getAPrefix for naming our buttons in scene Hierarchy
        _prefix = "in_";


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
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class InInventory : UIInventoryManager
{
    #region InitalSetup
    private void Start()
    {
        if (_bSlotPREFAB == null)
            _bSlotPREFAB = Resources.Load<GameObject>("Prefab/UI/bSlot");
        if (!_optionalSendButton)
        {
            var go = GameObject.FindGameObjectWithTag("SendButton");
            if (go != null)
            {
                _optionalSendButton = go.GetComponent<Button>();
                _optionalSendButton.interactable = false;
            }
        }
        _inventoryType = eInvType.IN;
        GetGameManagerData();
        GenInventory();
        //Debug.LogWarning("(s)SLOTS SIZE=" + _slots.Length);
    }

    private void Update()
    {
        //Testing
        if (_inventoryType == eInvType.IN && Input.GetKeyDown(KeyCode.A))
        {
            foreach (var s in _slots)
                s.AssignItem(1, 1);

            AddItemToSlot(2, false);
        }
    }

    private void GetGameManagerData()
    {
        _INVENTORYSIZE = DetermineWorkStationBatchSize();
        _STACKABLE = GameManager.instance._isStackable;
        _ADDCHAOTIC = GameManager.instance._addChaotic;
        GameManager.instance.SetInventoryIn(this);
    }


    /************************************************************************************************************/
    #region batchSizeMethods

    private int DetermineWorkStationBatchSize()
    {
        WorkStationManager wm = UIManager.instance._workstationManager;
        bool test = false;
        int BATCHSIZE = GameManager.instance._batchSize;
        WorkStation myWS = GameManager.instance._workStation;
        int count = 0;
        //if batch size =1 , then IN = # of required Items produced previous station
        if (BATCHSIZE == 1) //assume batchsize=1 enabled stackable Inv and StationINV is turned on
        {
            int[] stationSequence = getProperSequence(wm, myWS);//really feels like a doubly linked list might be better?
            var stationList = wm.GetStationList();
            //Figure out myplace in Sequence 
            int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
            if (startingIndex > 1) // Might be right, return 0 if kitting becuz there is no in iNVT at kitting?
            {
                WorkStation stationBeforeMe = stationList[startingIndex - 1];
                foreach (Task t in stationBeforeMe._tasks)
                {
                    count += t._requiredItemIDs.Count;
                }
            }
            else
                Debug.LogWarning("!!!!...Does this happen?, if not can move this into the same SumSequence() function call");

            return count * BATCHSIZE;
        }
        else //Sum the total required items from self + all subseqential workstations, and * BATCH_SIZE
        {
            return ParseItems(wm, myWS, false) * BATCHSIZE;
        }
    }

    protected virtual int ParseItems(WorkStationManager wm, WorkStation myWS, bool AddToSlotOnFind)
    {
        int count = 0;
        int[] stationSequence = getProperSequence(wm, myWS);
        var stationList = wm.GetStationList();
        //Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        int BATCHSIZE = GameManager.instance._batchSize;
        if (BATCHSIZE == 1)
        {
            //look at all required items that arent basic at my own station
            foreach (var task in myWS._tasks)
            {
                foreach (var item in task._requiredItemIDs)
                {
                    if (!BuildableObject.Instance.IsBasicItem(item)) //decide if basic item 
                    {
                        int itemId = (int)item;
                        ++count;
                        if (AddToSlotOnFind)
                        {
                            AddItemToSlot((int)item, false);
                            // Debug.LogWarning($" (2)...Task::{t} adding item:{item} #{itemId}");
                        }

                    }
                }
            }
        }
        else  //look at all final items for station before me , and the basic items from kitting[1]
        {
            var listItems = FindObjectsAtKittingStation(stationList[1]);
            Debug.LogError("Staring index=+" + startingIndex);
            //foreach station between us and kitting, if listItem contains a requiredItem, remove it
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
                            Debug.Log($"_requiredItems.. Station::{ws} --> Task::{t}  --> Item{item} #{itemId}");
                            if (listItems.Contains(itemId))
                                listItems.Remove(itemId);
                        }
                        //were at prior station
                        if (i == startingIndex - 1)
                        {
                            //Add the final items from station prior to me
                            foreach (var item in t._finalItemID)
                            {
                                int itemId = (int)item;
                                listItems.Add(itemId);
                                Debug.Log($"_finalItems....Station::{ws} --> Task::{t}  --> Item{item} #{itemId}");

                            }
                        }
                    }
                }
            }
            //finally we can add what we found
            if (AddToSlotOnFind)
            {
                foreach (var item in listItems)
                {
                    for (int j = 0; j < BATCHSIZE; j++)
                        AddItemToSlot((int)item, false);
                }
            }
            count += listItems.Count;

        }
         Debug.Log($"The # of INV items will be : {count}");
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
            {
                items.Add((int)item);

            }
        }

        return items;
    }

    private void SetUpStartingItems()
    {
        ParseItems(UIManager.instance._workstationManager, GameManager.instance._workStation, true);
        //int BATCHSIZE = GameManager.instance._batchSize;
        //foreach (var task in GameManager.instance._workStation._tasks)
        //{
        //    int count = BATCHSIZE;
        //    foreach (var item in task._requiredItemIDs)
        //    {
        //        for (int i = 0; i < count; i++)
        //        {
        //            //Debug.Log("Add item" + (int)item);
        //            AddItemToSlot((int)item, false);
        //        }

        //    }
        //}
    }


    #endregion
    /************************************************************************************************************/

    /**Generates the Inventory with correct dimensions based on Game Settings. */
    private void GenInventory()
    {

        _slots = new UIInventorySlot[_INVENTORYSIZE];
        //Debug.LogError($"{_inventoryType} slotsize ={ _slots.Length}");

        //Determine layout
        _xMaxPerRow = _INVENTORYSIZE;
        if (_INVENTORYSIZE > 4 && _inventoryType != eInvType.STATION)
            _xMaxPerRow = (_INVENTORYSIZE / 4) + 1;

        if (_xMaxPerRow > 10)
            _xMaxPerRow = 10;

        //Debug.Log($"{this.transform.gameObject.name}{_inventoryType}, {_INVENTORYSIZE} resulted in {_xMaxRows}");

        //Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        //getAPrefix for naming our buttons in scene Hierarchy
        _prefix = "in_";


        //Any slots added after this will be kept track of in an extra list incase we ever want to reset to base amount
        _extraSlots = new List<UIInventorySlot>(); //Instantiated before for loop becuz CreateNewslot uses its Count

        for (int i = 0; i < _INVENTORYSIZE; ++i)
        {
            //Add slot component to our list
            _slots[i] = CreateNewSlot();
        }

        SetUpStartingItems();
    }


    /**Determines the size of the content area based on how many items/rows we have. The overall size affects scrolling */
    private void SetSizeOfContentArea()
    {
        if (_xMaxPerRow == 0)
            return;
        RectTransform rt = this.GetComponent<RectTransform>();

        rt.sizeDelta = new Vector2((_xMaxPerRow * _cellPadding) + (_cellPadding / 2), ((((_INVENTORYSIZE / _xMaxPerRow) + 1) * _cellPadding) + (_cellPadding)));

    }

  

    #endregion
}

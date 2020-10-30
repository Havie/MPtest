using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.UI;

public class StationInventory : UIInventoryManager
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
        _inventoryType = eInvType.STATION;
        GetGameManagerData();
        GenInventory();
        //Debug.LogWarning("(s)SLOTS SIZE=" + _slots.Length);
    }



    private void GetGameManagerData()
    {
        _INVENTORYSIZE = DetermineWorkStationBatchSize();
        _STACKABLE = GameManager.instance._isStackable;
        _ADDCHAOTIC = GameManager.instance._addChaotic;
        GameManager.instance.SetInventoryStation(this);
    }


    /************************************************************************************************************/
    #region batchSizeMethods

    private int DetermineWorkStationBatchSize()
    {
        WorkStationManager wm = UIManager.instance._workstationManager;


        int BATCHSIZE = GameManager.instance._batchSize;
        WorkStation myWS = GameManager.instance._workStation;
        int count = 0;
        //if batch size ==1 then just the required # of items at this station (pull)
        if (BATCHSIZE == 1)
        {
            foreach (Task t in myWS._tasks)
                count += t._requiredItemIDs.Count;

            return count;
        }
        //else were kitting and its 
        else  //Sum the total required items from  all subseqential workstations (not mult BATCH_SIZE cuz INF slots)
        {
            return ParseItemList(wm, myWS, false);
        }
    }

    protected int ParseItemList(WorkStationManager wm, WorkStation myWS, bool addAsInfiniteItem)
    {
        int count = 0;
        int[] stationSequence = getProperSequence(wm, myWS);
        var stationList = wm.GetStationList();
        //Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        List<int> seenItems = new List<int>();

        if (myWS.isKittingStation()) // look at everyones required items
        {
            //Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
            for (int i = startingIndex; i < stationSequence.Length; i++)
            {
                WorkStation ws = stationList[i]; /// think this is in order?
                foreach (Task t in ws._tasks)
                {
                    //verify no duplicates
                    foreach (var item in t._requiredItemIDs)
                    {
                        if (BuildableObject.Instance.IsBasicItem(item)) //only want basic parts
                        {
                            int itemId = (int)item;
                            if (!seenItems.Contains(itemId))
                            {
                                seenItems.Add(itemId);
                                ++count;
                                Debug.Log($"Kitting_requiredItems.. Station::{ws} --> Task::{t}  --> Item{item} #{itemId}");
                                if(addAsInfiniteItem)
                                {
                                    AssignInfiniteItem(itemId);
                                }
                            }
                        }
                    }

                }

            }
        }
        else // just look at my own items
        {
            foreach (var task in myWS._tasks)
            {
                foreach (var item in task._requiredItemIDs)
                {
                    if (BuildableObject.Instance.IsBasicItem(item)) 
                    {
                        int itemId = (int)item;
                        if (!seenItems.Contains(itemId))
                        {
                            seenItems.Add(itemId);
                            ++count;
                        }
                    }
                }
            }
        }
        //Debug.Log($"The # of INV items will be : {count}");
        return count;
    }
    #endregion
    /************************************************************************************************************/

    /**Generates the Inventory with correct dimensions based on Game Settings. */
    private void GenInventory()
    {
        if (NotStackableAndNotKitting())
            return;

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

        //cache a conditions for forloop situations
        bool cond = GameManager.instance._autoSend; //used By eInvType.OUT
        Dictionary<Task, int> seenTasks = new Dictionary<Task, int>(); //used By eInvType.STATION
        List<int> seenItems = new List<int>();
        WorkStationManager wm = UIManager.instance._workstationManager;
        WorkStation myWS = GameManager.instance._workStation;
        //getAPrefix for naming our buttons in scene Hierarchy
        _prefix = "station_";

        //Any slots added after this will be kept track of in an extra list incase we ever want to reset to base amount
        _extraSlots = new List<UIInventorySlot>(); //Instantiated before for loop becuz CreateNewslot uses its Count

        for (int i = 0; i < _INVENTORYSIZE; ++i)
        {
            //Add slot component to our list
            _slots[i] = CreateNewSlot();

           // SetInfiniteItem(wm, myWS, seenTasks, seenItems, i);
        }
        ParseItemList(wm, myWS, true);
    }

    private bool NotStackableAndNotKitting()
    {
        if (_inventoryType == eInvType.STATION && !_STACKABLE)
        {
            var ws = GameManager.instance._workStation;
            if (!ws.isKittingStation())
            {
                Destroy(this.gameObject); //good enough for now might need to go higher to parents
                Debug.Log($"{ws._stationName}  is kittingStation={ws.isKittingStation()} , and isSTACKABLE={_STACKABLE},  Destroying station inv (unused)");
                return true;
            }
        }
        return false;
    }

    /**Sets the stackable items at this stations inventory based off the required items needed at this station */
    private void SetInfiniteItem(WorkStationManager wm, WorkStation myWS, Dictionary<Task, int> seenTasks, List<int> seenItems, int i)
    {
        //Debug.LogWarning($"SetINF@{i} , slotsize={_slots.Length}");

        //need to look at the tasks in all workstations after me  (ignore self because required items in next staton are same as mine)
        int[] stationSequence = getProperSequence(wm, myWS);//really feels like a doubly linked list might be better?
        var stationList = wm.GetStationList();
        //Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        // Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        if (GameManager.instance._batchSize == 1)
        {
            WorkStation mwWS = GameManager.instance._workStation;
            AddItemFromTasksAtOwnStation(myWS, seenTasks, i, seenItems);
        }
        else
        {
            if (!GameManager.instance._workStation.isKittingStation())
                Debug.LogWarning("This happened for a non kitting station, dont think this shud");

            AddItemFromTasksAtSubsequentialStations(startingIndex, stationSequence, stationList, seenTasks, i, seenItems);
        }
    }
    /** Tries to add items from current station and all subsequential work station tasks*/
    private bool AddItemFromTasksAtSubsequentialStations(int startingIndex, int[] stationSequence, List<WorkStation> stationList, Dictionary<Task, int> seenTasks, int i, List<int> seenItems)
    {
        for (int j = startingIndex + 1; j < stationSequence.Length; j++)
        {
            WorkStation ws = stationList[j]; /// think this is in order?
            foreach (Task t in ws._tasks)
            {
                if (TryToAssignIfiniteItem(seenTasks, t, seenItems, i))
                    return true;
            }
        }
        return false;
    }
    /**Only tries to add items from tasks at own workstation */
    private bool AddItemFromTasksAtOwnStation(WorkStation ws, Dictionary<Task, int> seenTasks, int i, List<int> seenItems)
    {
        foreach (Task t in ws._tasks)
        {
            if (TryToAssignIfiniteItem(seenTasks, t, seenItems, i))
                return true;
        }
        return false;
    }

    /**Tries to assign an unseen item from a task. Dictonary is used to keep track of last item index seen in task. */
    private bool TryToAssignIfiniteItem(Dictionary<Task, int> seenTasks, Task t, List<int> seenItems, int i)
    {
        if (seenTasks.ContainsKey(t))
        {
            int amntOfItemsSeen = seenTasks[t];
            int requiredItemsInTask = t._requiredItemIDs.Count;
            //Debug.Log($"Amount of items seen in Task {t} is {amntOfItemsSeen} vs {requiredItemsInTask}");
            if (amntOfItemsSeen < requiredItemsInTask)
            {
                seenTasks.Remove(t);
                seenTasks.Add(t, amntOfItemsSeen + 1);
                if (BuildableObject.Instance.IsBasicItem(t._requiredItemIDs[amntOfItemsSeen]))
                {
                    int id = (int)t._requiredItemIDs[amntOfItemsSeen];
                    //Debug.Log($"(1) Assigned INFINITE Item ID {id}, from Station:{ws}::task::{t}::item::{t._requiredItemIDs[amntOfItemsSeen]}");
                    // Debug.Log($"(1) Trying to Assign INFINITE Item ID {id}, task::{t}::item::{t._requiredItemIDs[amntOfItemsSeen]}");

                    if (!seenItems.Contains(id))
                    {
                        AssignInfiniteItem(id, i, seenItems);
                        return true;
                    }
                    else //keep looking in this task for more items since weve already seen one of these 
                    {
                        TryToAssignIfiniteItem(seenTasks, t, seenItems, i);
                    }
                }
               // else
                  //  Debug.Log($" FAILED id={ (int)t._requiredItemIDs[amntOfItemsSeen]} , {BuildableObject.Instance.GetItemNameByID((int)t._requiredItemIDs[amntOfItemsSeen])} ... from task::{t}");
            }
        }
        else
        {
            seenTasks.Add(t, 1);
            if (BuildableObject.Instance.IsBasicItem(t._requiredItemIDs[0]))
            {
                int id = (int)t._requiredItemIDs[0];
                // Debug.Log($"(0) Assigned INFINITE Item ID {id}, from Station:{ws}::task::{t}::item::{t._requiredItemIDs[0]}");
                // Debug.Log($"(0) Trying to Assign INFINITE Item ID {id}, ::task::{t}::item::{t._requiredItemIDs[0]}");

                if (!seenItems.Contains(id))
                {
                    AssignInfiniteItem(id, i, seenItems);
                    return true;
                }
            }
            else
            {
                //Debug.Log($" FAILED id={ (int)t._requiredItemIDs[0]}, {BuildableObject.Instance.GetItemNameByID((int)t._requiredItemIDs[0])} ... from task::{t}");
                TryToAssignIfiniteItem(seenTasks, t, seenItems, i);
            }
        }

        return false;
    }
    /** Assigns the item to a a given slot and adds item to our list so it cant be readded twice*/
    private void AssignInfiniteItem(int id, int i, List<int> seenItems)
    {
        _slots[i].AssignItem(id, int.MaxValue);
        seenItems.Add(id);
        //Debugging 
       /* string seen = "";
        foreach (var item in seenItems)
        {
            seen += " , " + item;
        }
        Debug.Log($" ADDED {id } to SLOT{i},  seen INF so far:" + seen);*/
    }

    private void AssignInfiniteItem(int itemID)
    {
        foreach (UIInventorySlot slot in _slots)
        {
            if (!slot.GetInUse())
            {
                slot.AssignItem(itemID, int.MaxValue);
                return;
            }
               
        }
        foreach (UIInventorySlot slot in _extraSlots)
        {
            if (!slot.GetInUse())
            {
                slot.AssignItem(itemID, int.MaxValue);
                return;
            }
               
        }
        //fell thru so we are full
        Debug.Log("we fell thru");
        UIInventorySlot nSlot = CreateNewSlot();
        nSlot.AssignItem(itemID, int.MaxValue);
        _extraSlots.Add(nSlot);
    }
    /**Determines the size of the content area based on how many items/rows we have. The overall size affects scrolling */
    private void SetSizeOfContentArea()
    {
        if (_xMaxPerRow == 0)
            return;
        RectTransform rt = this.GetComponent<RectTransform>();

        rt.sizeDelta = new Vector2((_xMaxPerRow * _cellPadding) + (_cellPadding / 2), ((((_INVENTORYSIZE / _xMaxPerRow) + 1) * _cellPadding) + (_cellPadding)));
        // fix for a really weird issue with off center inv
        this.transform.localPosition = new Vector3(this.transform.localPosition.x / 2, this.transform.localPosition.y, this.transform.localPosition.z);
    }




    #endregion
}

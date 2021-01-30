
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StationInventory : UIInventoryManager
{
    #region InitalSetup
    protected override void Start()
    {
        if (IsInitalized)
            return;

        if (_bSlotPREFAB == null)
            _bSlotPREFAB = Resources.Load<GameObject>("Prefab/UI/bSlot");

        _inventoryType = eInvType.STATION;
        GetGameManagerData();
        GenInventory();
        //Debug.LogWarning("(s)SLOTS SIZE=" + _slots.Length);

    }

    private void GetGameManagerData()
    {
        _INVENTORYSIZE = DetermineWorkStationBatchSize();
        _STACKABLE = GameManager.Instance._isStackable;
        _ADDCHAOTIC = GameManager.Instance._addChaotic;
        GameManager.Instance.SetInventoryStation(this);
    }


    /************************************************************************************************************/
    #region batchSizeMethods

    private int DetermineWorkStationBatchSize()
    {
        WorkStationManager wm = UIManager.Instance._workstationManager;


        int BATCHSIZE = GameManager.Instance._batchSize;
        WorkStation myWS = GameManager.Instance._workStation;
        int count = 0;
        //if batch size ==1 then just the required # of items at this station (pull)
        if (BATCHSIZE == 1)
        {
            foreach (Task t in myWS._tasks)
            {
                count += t._requiredItemIDs.Count;
                Debug.Log($"batch size is 1 and itemCount ={t._requiredItemIDs.Count} for Task:{t}");
            }

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
                                //Debug.Log($"Kitting_requiredItems.. Station::{ws} --> Task::{t}  --> Item{item} #{itemId}");
                                if (addAsInfiniteItem)
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
                            if (addAsInfiniteItem)
                            {
                                AssignInfiniteItem(itemId);
                            }
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
        //if (NotStackableAndNotKitting())
        if (NotStackableOrKitting())
            return;

        _slots = new UIInventorySlot[_INVENTORYSIZE];
        IsInitalized = true;
        //Debug.LogError($"{_inventoryType} slotsize ={ _slots.Length}");

        //Determine layout
        _xMaxPerRow = _INVENTORYSIZE;

        if (_xMaxPerRow > 10) /// might be an issue when we have multiple Items to make, will revist
            _xMaxPerRow = 10;

        //Debug.Log($"{this.transform.gameObject.name}{_inventoryType}, {_INVENTORYSIZE} resulted in {_xMaxRows}");

        //Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        //cache a conditions for forloop situations
        Dictionary<Task, int> seenTasks = new Dictionary<Task, int>(); //used By eInvType.STATION
        List<int> seenItems = new List<int>();
        WorkStationManager wm = UIManager.Instance._workstationManager;
        WorkStation myWS = GameManager.Instance._workStation;
        //getAPrefix for naming our buttons in scene Hierarchy
        _prefix = "station_";

        //Any slots added after this will be kept track of in an extra list incase we ever want to reset to base amount
        _extraSlots = new List<UIInventorySlot>(); //Instantiated before for loop becuz CreateNewslot uses its Count

        for (int i = 0; i < _INVENTORYSIZE; ++i)
        {
            //Add slot component to our list
            _slots[i] = CreateNewSlot();


        }
        ParseItemList(wm, myWS, true);
    }


    /// <summary> Kitting used to get a station inv before change to make items drop in </summary>
    private bool NotStackableAndNotKitting()
    {
        if (_inventoryType == eInvType.STATION && !_STACKABLE)
        {
            var ws = GameManager.Instance._workStation;
            if (!ws.isKittingStation())
            {
                Destroy(this.gameObject); //good enough for now might need to go higher to parents
                UIManager.Instance.DebugLogWarning($"{ws._stationName}  is kittingStation={ws.isKittingStation()} , and isSTACKABLE={_STACKABLE},  Destroying station inv (unused)");
                return true;
            }
        }
        UIManager.Instance.DebugLog("Station is stackable so enabling personal inventory, TODO remove these items from calculation of in invetory/send inventory");
        return false;
    }

    /// <summary> Kitting no longer gets a station inv since items drop in now </summary>
    private bool NotStackableOrKitting()
    {
        if (!_STACKABLE)
        {
            Destroy(this.gameObject); //good enough for now might need to go higher to parents
            return true;
        }
        else if (_inventoryType == eInvType.STATION)
        {
            var ws = GameManager.Instance._workStation;
            if (ws.isKittingStation())
            {
                Destroy(this.gameObject); //good enough for now might need to go higher to parents
                return true;
            }
        }
        UIManager.Instance.DebugLog("Station is stackable so enabling personal inventory, TODO remove these items from calculation of in invetory/send inventory");
        return false;
    }


    /**Determines the size of the content area based on how many items/rows we have. The overall size affects scrolling */
    protected override void SetSizeOfContentArea()
    {
        if (_xMaxPerRow == 0)
            return;
        RectTransform rt = this.GetComponent<RectTransform>();

        rt.sizeDelta = new Vector2((_xMaxPerRow * _cellPadding) + (_cellPadding / 2), ((((_INVENTORYSIZE / _xMaxPerRow) + 1) * _cellPadding)));
        // fix for a really weird issue with off center inv
        //this.transform.localPosition = new Vector3(this.transform.localPosition.x / 2, this.transform.localPosition.y, this.transform.localPosition.z);
        ///*Note since bSlots are anchored to top corner for IN/OUT, when they come in for station things get weird, thus the Station content pane is offset to fix this

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

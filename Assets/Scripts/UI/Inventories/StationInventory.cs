
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

        base.Start();

        GameManager.Instance.SetInventoryStation(this);
        Debug.LogWarning("(s)SLOTS SIZE=" + _slots.Length);

    }




    /************************************************************************************************************/
    #region batchSizeMethods

    protected override List<int> DetermineWorkStationBatchSize()
    {
        var gm = GameManager.instance;

        WorkStationManager wm = gm.CurrentWorkStationManager;
        int batchSize = gm._batchSize;
        WorkStation myWS = gm._workStation;
        Debug.Log($"STATION WS = {myWS} ");
        return StationItemParser.ParseItemsAsStation(batchSize, wm, myWS);
    }

    private void SetUpInfiniteItems(WorkStationManager wm, WorkStation myWS)
    {

        foreach(var itemID in StationItemParser.ParseItemsAsStation(GameManager.instance._batchSize, wm, myWS))
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
        //Debug.LogError($"{_inventoryType} slotsize ={ _slots.Length}");


        //Debug.Log($"{this.transform.gameObject.name}{_inventoryType}, {_INVENTORYSIZE} resulted in {_xMaxRows}");

        //Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        //cache a conditions for forloop situations
        Dictionary<Task, int> seenTasks = new Dictionary<Task, int>(); //used By eInvType.STATION
        List<int> seenItems = new List<int>();
        WorkStationManager wm = GameManager.Instance.CurrentWorkStationManager;
        WorkStation myWS = GameManager.Instance._workStation;
        //getAPrefix for naming our buttons in scene Hierarchy
        _prefix = "Station";

        //Any slots added after this will be kept track of in an extra list incase we ever want to reset to base amount
        _extraSlots = new List<UIInventorySlot>(); //Instantiated before for loop becuz CreateNewslot uses its Count

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

        int batchSize = GameManager.instance._batchSize;
        var ws = GameManager.Instance._workStation;
        if (ws.isKittingStation() && batchSize!=1)
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

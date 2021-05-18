using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class OutInventory : UIInventoryManager
{
    [Header("Components")]
    [SerializeField] protected Button _sendButton;

    [Header("Event")]
    [SerializeField] BatchEvent _batchSentEvent;


    [Header("Dev Option")]
    [SerializeField] bool _enableFirstSend;


    #region InitalSetup
    protected override void Start()
    {
        if (IsInitalized)
            return;

        if (!_sendButton)
        {
            Debug.Log($"<color=yellow> Unassigned send button.. hope for the best");
            _sendButton = this.GetComponentInChildren<Button>();

        }
        ///IDK if this is ideal, but i keep forgetting to set this if we remove it
        if (_sendButton.onClick.GetPersistentEventCount() == 0)
        {
            _sendButton.onClick.RemoveAllListeners();
        }
        _sendButton.onClick.AddListener(SendBatch);
        ///Disable SendButton till batch is ready:
        if (!_enableFirstSend)
        {
            _sendButton.interactable = false;
        }
        base.Start();
        //Debug.LogWarning("(s)SLOTS SIZE=" + _slots.Length);

    }



    /************************************************************************************************************/
    #region batchSizeMethods

    protected override List<int> DetermineWorkStationBatchSize()
    {
        var gm = GameManager.instance;
        _batchSize = gm._batchSize;
        ///if batch size =1 , then IN = # of produced Items at station
        if (_batchSize == 1) ///assume batchsize=1 enabled stackable Inv and StationINV is turned on
        {
            _sendButton.gameObject.SetActive(false); ///turn off the send button
        }

        return StationItemParser.ParseItemsAsOUT(_batchSize, gm._isStackable, gm.CurrentWorkStationManager, gm._workStation);
        // return ParseItems(wm, myWS, false) * BATCHSIZE;

    }

    private void SetUpStartingItems(List<int> itemIDs)
    {
        foreach (var itemID in itemIDs)
        {
            AddItemToSlot(itemID, null, true);
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

        //Determine layout

        //Debug.Log($"{this.transform.gameObject.name}{_inventoryType}, {_INVENTORYSIZE} resulted in {_xMaxRows}");

        //Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        //cache a conditions for forloop situations
        bool cond = GameManager.instance._autoSend; //used By eInvType.OUT

        //getAPrefix for naming our buttons in scene Hierarchy
        _prefix = "Out";

        //Any slots added after this will be kept track of in an extra list incase we ever want to reset to base amount
        _extraSlots = new List<UIInventorySlot>(); //Instantiated before for loop becuz CreateNewslot uses its Count

        for (int i = 0; i < _INVENTORYSIZE; ++i)
        {
            //Add slot component to our list
            _slots[i] = CreateNewSlot();
            _slots[i].SetAsOutSlot();
            _slots[i].transform.localScale = new Vector3(1, 1, 1);
        }

        SetUpStartingItems(itemIDs);
    }



    /************************************************************************************************************/

    public override void SlotStateChanged(UIInventorySlot slot)
    {
        if (_batchSize == 1)
        {
            ClientSend.Instance.KanbanChanged(false, !slot.GetInUse(), slot.RequiredID, slot.Qualities);
        }
        CheckIfBatchIsReady();
    }


    /** When an item gets assigned to the batch tell the manager*/
    public void CheckIfBatchIsReady()
    {
        //If all buttons hold the correct items , we can send
        if (_sendButton)
            _sendButton.interactable = true;
        foreach (var slot in _slots)
        {
            if (!slot.GetInUse())
            {
                if (_sendButton)
                    _sendButton.interactable = false;
                return;
            }
        }
    }

    public void SendBatch()
    {
        //Debug.Log($"heared send batch {this.gameObject.name} ");
        int count = 0;
        foreach (var slot in _slots)
        {
            if (slot.SendData())
                ++count;
        }

        bool allowSendWrongItems = false;

        if (allowSendWrongItems)
        {
            foreach (var slot in _extraSlots)
            {
                if (slot.SendData())
                    ++count;
            }
        }

        if (_sendButton)
            _sendButton.interactable = false;

        ///TaskComplete 
        /// TODO might want to identify our station ID somehow else
        if (_batchSentEvent)
        {
            WorkStation ws = GameManager.Instance._workStation;
            _batchSentEvent.Raise(new BatchWrapper((int)ws._myStation, count, ws.IsShippingStation()));
        }
        ///If any items in extra slots, should be assigned to the next batch
        ParseExtraSlotsAndReassignIfNeeded();
    }

    private void ParseExtraSlotsAndReassignIfNeeded()
    {
        ///This happens in kitting where the player can add extra stuff for the next batch,
        ///and it gets added as new "extra" slot . Upon sending the batch, we need to 
        ///move these extra items out of extra slots, and into right place for next batch
        List<UIInventorySlot> _slotsToBeRemoved = new List<UIInventorySlot>();
        foreach (var item in _extraSlots)
        {
            if (item.GetInUse())
            {
                for (int i = 0; i < _slots.Length; i++)
                {
                    var slot = _slots[i];
                    if (TryToAdd(slot, item.GetItemID(), item.Qualities, false)) //item.RequiresCertainID() ? think just false works
                    {
                        _slotsToBeRemoved.Add(item);
                        break;
                    }
                }
            }
        }

        foreach (var emptySlot in _slotsToBeRemoved)
        {
            _extraSlots.Remove(emptySlot);
            Destroy(emptySlot.gameObject);
            --_INVENTORYSIZE; ///Not even sure this matters, past initial slot creation
        }
            SetSizeOfContentArea(); ///adjust scrollable area
    }


    #endregion
}

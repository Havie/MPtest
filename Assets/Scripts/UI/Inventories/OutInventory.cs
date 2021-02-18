using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutInventory : UIInventoryManager
{
    [Header("Event")]
    [SerializeField] BatchEvent _batchSentEvent;

    #region InitalSetup
    protected override void Start()
    {
        if (IsInitalized)
            return;

        base.Start();
        if (!_sendButton)
        {
            if (_optionalSendButton)
            {
                _sendButton = _optionalSendButton.GetComponent<Button>();
                _sendButton.interactable = false;
            }
        }
      
        _inventoryType = eInvType.OUT;
        GetGameManagerData();
        GenInventory();
        //Debug.LogWarning("(s)SLOTS SIZE=" + _slots.Length);

    }


    private void GetGameManagerData()
    {
        _INVENTORYSIZE = DetermineWorkStationBatchSize();
        _STACKABLE = GameManager.Instance._isStackable;
        _ADDCHAOTIC = GameManager.Instance._addChaotic;
        GameManager.Instance.SetInventoryOut(this);

    }


    /************************************************************************************************************/
    #region batchSizeMethods

    private int DetermineWorkStationBatchSize()
    {
        var gm = GameManager.instance;
        int batchSize = gm._batchSize;
        ///if batch size =1 , then IN = # of produced Items at station
        if (batchSize == 1) ///assume batchsize=1 enabled stackable Inv and StationINV is turned on
        {
            _sendButton.gameObject.SetActive(false); ///turn off the send button
        }

        return StationItemParser.ParseItemsAsOUT(batchSize, gm.CurrentWorkStationManager, gm._workStation).Count;
       // return ParseItems(wm, myWS, false) * BATCHSIZE;

    }

    private void SetUpBatchOutput(WorkStationManager wm, WorkStation myWS)
    {
        //ParseItems(wm, myWS, true);
        int BATCHSIZE = GameManager.Instance._batchSize;
        foreach (var item in  StationItemParser.ParseItemsAsOUT(BATCHSIZE, wm, myWS))
        {
            AddItemToSlot((int)item, null, true);
        }

    }

 
    #endregion
    /************************************************************************************************************/

    /**Generates the Inventory with correct dimensions based on Game Settings. */
    private void GenInventory()
    {
        _slots = new UIInventorySlot[_INVENTORYSIZE];
        IsInitalized = true;
        //Debug.LogError($"{_inventoryType} slotsize ={ _slots.Length}");

        //Determine layout
        _xMaxPerRow = _INVENTORYSIZE;
        if (_INVENTORYSIZE > _maxItemsPerRow && _inventoryType != eInvType.STATION)
            _xMaxPerRow = (_INVENTORYSIZE / _maxItemsPerRow) + 1;

        if (_xMaxPerRow > _maxItemsPerRow)
            _xMaxPerRow = _maxItemsPerRow;
        //Debug.Log($"{this.transform.gameObject.name}{_inventoryType}, {_INVENTORYSIZE} resulted in {_xMaxRows}");

        //Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        //cache a conditions for forloop situations
        bool cond = GameManager.instance._autoSend; //used By eInvType.OUT
        WorkStationManager wm =GameManager.Instance.CurrentWorkStationManager;
        WorkStation myWS = GameManager.instance._workStation;
        //getAPrefix for naming our buttons in scene Hierarchy

        _prefix = "out_";

        //Any slots added after this will be kept track of in an extra list incase we ever want to reset to base amount
        _extraSlots = new List<UIInventorySlot>(); //Instantiated before for loop becuz CreateNewslot uses its Count

        for (int i = 0; i < _INVENTORYSIZE; ++i)
        {
            //Add slot component to our list
            _slots[i] = CreateNewSlot();
            _slots[i].SetAutomatic(cond);
            _slots[i].transform.localScale = new Vector3(-1, 1, 1);
        }
        SetUpBatchOutput(wm, myWS);


    }



    /************************************************************************************************************/

    public override void ItemAssigned(UIInventorySlot slot)
    {
        CheckIfBatchIsReady();
    }


    /** When an item gets assigned to the batch tell the manager*/
    public void CheckIfBatchIsReady()
    {
        ///TMP off
        /*
         foreach (var slot in _slots)
         {
             if (!slot.GetInUse())
             {
                 if (_optionalSendButton)
                     _optionalSendButton.interactable = false;
                 return;
             }
         }
        */
        //If all buttons hold the correct items , we can send
        if (_sendButton)
            _sendButton.interactable = true;


    }

    public void SendBatch()
    {
        Debug.Log($"heared send batch {this.gameObject.name} ");
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
    }


    #endregion
}

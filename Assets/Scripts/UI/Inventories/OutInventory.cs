using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutInventory : UIInventoryManager
{
    [Header("Specifications")]
    [SerializeField] int _maxItemsPerRow = 4;
    [SerializeField] int _maxColSize = 425;

    #region InitalSetup
    protected override void Start()
    {
        if (IsInitalized)
            return;

        if (_bSlotPREFAB == null)
            _bSlotPREFAB = Resources.Load<GameObject>("Prefab/UI/bSlot");
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
        WorkStationManager wm = GameManager.Instance.CurrentWorkStationManager;
        int BATCHSIZE = GameManager.Instance._batchSize;
        WorkStation myWS = GameManager.Instance._workStation;
        ///if batch size =1 , then IN = # of produced Items at station
        if (BATCHSIZE == 1) ///assume batchsize=1 enabled stackable Inv and StationINV is turned on
        {
            _sendButton.gameObject.SetActive(false); ///turn off the send button
           // TurnOffScrollBars();
        }

        return ParseItems(wm, myWS, false) * BATCHSIZE;

    }

    private void SetUpBatchOutput(WorkStationManager wm, WorkStation myWS)
    {
        ParseItems(wm, myWS, true);
    }

    private int ParseItems(WorkStationManager wm, WorkStation myWS, bool AddToSlot)
    {
        int count = 0;
        int BATCHSIZE = GameManager.Instance._batchSize;
        //Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        if (BATCHSIZE == 1) /// PULL SYSTEM
        {
            ///get last task at my station and put in its final item:
            Task t = myWS._tasks[myWS._tasks.Count - 1];
            count += t._finalItemID.Count;
            if (AddToSlot)
            {
                foreach (var item in t._finalItemID)
                {
                    // Debug.LogError($" Making {item} required");
                    AddItemToSlot((int)item, null, true);
                }
            }

        }
        if (BATCHSIZE > 1)  ///Sum the total required items (not self) all subseqential workstations, and * BATCH_SIZE
        {
            int[] stationSequence = getProperSequence(wm, myWS);
            var stationList = wm.GetStationList();
            ///Figure out myplace in Sequence 
            int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
            bool firstTime = true;
            bool isKittingStation = myWS.isKittingStation();
            ///shud only b the final item of last task
            for (int i = startingIndex; i < stationSequence.Length; i++)
            {
                WorkStation ws = stationList[i];
                for (int c = 0; c < ws._tasks.Count; ++c)
                {
                    Task t = ws._tasks[c]; ///get the current task 
                    if (!isKittingStation) ///look at the immediate output for next station final ID, then pass on basic items for others
                    {
                        if (firstTime) ///add my own output, keep this in the inital loop so we dont have to check if kitting again outside, and created item is on top
                        {
                            if (c == ws._tasks.Count - 1) /// look at the last task at this station
                            {
                                foreach (var item in t._finalItemID) // final produced items
                                {
                                    if (!BuildableObject.Instance.IsBasicItem(item)) ///decide if basic item 
                                    {
                                        int itemId = (int)item;
                                        ++count;
                                        if (AddToSlot)
                                        {
                                            for (int j = 0; j < BATCHSIZE; j++)
                                            {
                                                AddItemToSlot((int)item, null, true);
                                            }
                                            // Debug.LogWarning($" (1)...Task::{t} adding item:{item} #{itemId}");
                                        }

                                    }
                                }
                            }
                        }
                        else //add the batch items to pass along to other stations
                        {
                            foreach (var item in t._requiredItemIDs) /// look at all of its required items
                            {
                                if (BuildableObject.Instance.IsBasicItem(item)) ///decide if basic item 
                                {
                                    int itemId = (int)item;
                                    ++count;
                                    if (AddToSlot)
                                    {
                                        for (int j = 0; j < BATCHSIZE; j++)
                                        {
                                            AddItemToSlot((int)item, null, true);
                                        }
                                        // Debug.LogWarning($" (2)...Task::{t} adding item:{item} #{itemId}");
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in t._requiredItemIDs) /// look at all of its _requiredItemIDs items
                        {
                            if (BuildableObject.Instance.IsBasicItem(item)) ///decide if basic item 
                            {
                                int itemId = (int)item;

                                ++count;
                                if (AddToSlot)
                                    for (int j = 0; j < BATCHSIZE; j++)
                                    {
                                        AddItemToSlot((int)item, null, true);
                                    }

                            }

                        }
                    }

                }
                firstTime = false;
            }
        }

        // Debug.Log($"The  # of OUTINV items will be : {count} * { GameManager.instance._batchSize}");
        return count;
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


    /**Determines the size of the content area based on how many items/rows we have. The overall size affects scrolling */
    protected override void SetSizeOfContentArea()
    {
        if (_xMaxPerRow == 0)
            return;

        Vector2 size = Vector2.zero;


        if (GameManager.instance._batchSize == 1) ///turn off the pesky vert scroll bars
            size = new Vector2(_cellPadding, _cellPadding); ///will need to change if we add more than 1 item
        else
            size = new Vector2(((float)_xMaxPerRow * (float)_cellPadding) + (_cellPadding * 0), (((((float)_INVENTORYSIZE / (float)_xMaxPerRow)) * _cellPadding) + (_cellPadding *0)));

        //((((_INVENTORYSIZE / _xMaxPerRow)) * _cellPadding) + (_cellPadding /2))

        // Debug.Log($" {(float)_INVENTORYSIZE } / {(float)_xMaxPerRow} = <color=green>{((float)_INVENTORYSIZE / (float)_xMaxPerRow)}</color>  then w cellapdding = {((((float)_INVENTORYSIZE / (float)_xMaxPerRow)) * _cellPadding)} ");

        if (_content)
            _content.ChangeRectTransform(size);

        ///Recalibrate
        if (size.y > _maxColSize)
            size.y = _maxColSize;

        if (_bg) ///Make sure this called before Mask
            _bg.ChangeRectTransform(size);
        if (_mask)
            _mask.ChangeRectTransform(size);
        if (_scrollbarVert)
            _scrollbarVert.ChangeRectTransform(size);
        if (_scrollbarHoriz)
            _scrollbarHoriz.ChangeRectTransform(size);
        if (_optionalSendButton)
            _optionalSendButton.ChangeRectTransform(size);
        //if (_optionalSendButton)
        //    _optionalSendButton.transform.position = FindBelowBG();

            // Debug.Log($"(X:{(_xMaxPerRow * _cellPadding) + (_cellPadding / 2)} , Y: {((((_INVENTORYSIZE / _xMaxPerRow)) * _cellPadding) + (_cellPadding))} ) {_INVENTORYSIZE} / {_xMaxPerRow} = {(_INVENTORYSIZE / _xMaxPerRow)} Mod1:: {_INVENTORYSIZE-1 % _xMaxPerRow }");
    }

    private Vector3 FindBelowBG()
    {
        RectTransform rt = _bg.GetComponent<RectTransform>();
        Vector3 worldPosTop = rt.position;
        worldPosTop.y = worldPosTop.y - rt.sizeDelta.y;

        return worldPosTop;
    }


    #endregion
}

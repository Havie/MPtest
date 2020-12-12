using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OutInventory : UIInventoryManager
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
        _inventoryType = eInvType.OUT;
        GetGameManagerData();
        GenInventory();
        //Debug.LogWarning("(s)SLOTS SIZE=" + _slots.Length);
    }


    private void GetGameManagerData()
    {
        _INVENTORYSIZE = DetermineWorkStationBatchSize();
        _STACKABLE = GameManager.instance._isStackable;
        _ADDCHAOTIC = GameManager.instance._addChaotic;
        GameManager.instance.SetInventoryOut(this);

    }


    /************************************************************************************************************/
    #region batchSizeMethods

    private int DetermineWorkStationBatchSize()
    {
        WorkStationManager wm = UIManager.instance._workstationManager;
        int BATCHSIZE = GameManager.instance._batchSize;
        WorkStation myWS = GameManager.instance._workStation;
        ///if batch size =1 , then IN = # of produced Items at station
        if (BATCHSIZE == 1) ///assume batchsize=1 enabled stackable Inv and StationINV is turned on
        {
            _optionalSendButton.gameObject.SetActive(false); ///turn off the send button
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
        int BATCHSIZE = GameManager.instance._batchSize;
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
                    AddItemToSlot((int)item, true);
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
                                                AddItemToSlot((int)item, true);
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
                                            AddItemToSlot((int)item, true);
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
                                        AddItemToSlot((int)item, true);
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
        //Debug.LogError($"{_inventoryType} slotsize ={ _slots.Length}");

        //Determine layout
        _xMaxPerRow = _INVENTORYSIZE;
        if (_INVENTORYSIZE > 4)
            _xMaxPerRow = (_INVENTORYSIZE / 4) + 1;

        if (_xMaxPerRow > 3)
            _xMaxPerRow = 3;

        //Debug.Log($"{this.transform.gameObject.name}{_inventoryType}, {_INVENTORYSIZE} resulted in {_xMaxRows}");

        //Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        //cache a conditions for forloop situations
        bool cond = GameManager.instance._autoSend; //used By eInvType.OUT
        WorkStationManager wm = UIManager.instance._workstationManager;
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
        RectTransform rt = this.GetComponent<RectTransform>();

        if (GameManager.instance._batchSize == 1) ///turn off the pesky vert scroll bars
            rt.sizeDelta = new Vector2(_cellPadding, _cellPadding); ///will need to change if we add more than 1 item
        else
            rt.sizeDelta = new Vector2((_xMaxPerRow * _cellPadding) + (_cellPadding / 2), ((((_INVENTORYSIZE / _xMaxPerRow)) * _cellPadding)  + (_cellPadding)));

       //OLD
        // rt.sizeDelta = new Vector2((_xMaxPerRow * _cellPadding) + (_cellPadding / 2), ((((_INVENTORYSIZE / _xMaxPerRow) + 1) * _cellPadding) + (_cellPadding)));


        //Debug.Log($"(out) _INVENTORYSIZ={_INVENTORYSIZE} / _xMaxPerRow={_xMaxPerRow}  =  {((_INVENTORYSIZE / _xMaxPerRow) + 1)}");
    
    }



    #endregion
}

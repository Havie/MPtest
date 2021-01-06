using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


//https://www.youtube.com/watch?v=Oba1k4wRy-0 //Tutorial
[DefaultExecutionOrder(10000)] ///make this load late to let other things get set up first
public class UIInventoryManager : MonoBehaviour
{
    #region GameManager Parameters
    protected int _INVENTORYSIZE;
    protected bool _STACKABLE;
    protected bool _ADDCHAOTIC;
    #endregion
    protected GameObject _bSlotPREFAB;
    [SerializeField] GameObject _scrollBarVert = default;   
    [SerializeField] GameObject _scrollBarHoriz = default;
    public enum eInvType { IN, OUT, STATION };
    protected eInvType _inventoryType;
    protected Button _optionalSendButton;
    protected UIInventorySlot[] _slots;
    protected List<UIInventorySlot> _extraSlots; //incase we want to reset to base amount

    protected int _xMaxPerRow;
    protected int _cellPadding = 75;
    protected float _startingX = 37.5f;
    protected float _startingY = -37.5f;

    protected string _prefix;


    #region Helper Initilization Methods for extended classes
    protected void PrintASequence(int[] sequence, string seqName)
    {
        string p = "";
        for (int i = 0; i < sequence.Length; ++i)
        {
            p += $" , {sequence[i]}";
        }
         //Debug.Log(seqName+ ": " + p);
    }
   
    
    /** This is kind of a mess, thinking of making a doubly linked list class at some point*/
    protected int[] getProperSequence(WorkStationManager wm, WorkStation myWS)
    {
        int[] sequence = new int[wm.GetStationCount() + 1];
        //Debug.LogWarning("sequence size=" + wm.GetStationCount() + 1);
        foreach (WorkStation ws in wm.GetStationList())
        {
            //figure out sequence (*Exclude staiton 0 cuz SELF testing*)
            //each ws knows where its sending stuff , so we need to build the path?
            //Debug.Log($"{(int)ws._myStation} = {(int)ws._sendOutputToStation}");
            if ((int)ws._myStation != 0)
                sequence[(int)ws._myStation] = (int)ws._sendOutputToStation;

        }
        PrintASequence(sequence, "initial");
        //Find End:
        int endIndex = -1;
        for (int i = 1; i < sequence.Length - 1; ++i)
        {
            if (sequence[i] == (int)WorkStation.eStation.NONE)
            {
                endIndex = i;
                break;
            }
        }
        //Debug.Log("endIndex=" + endIndex);
        //Rebuild from backwards:
        int[] backwardSequence = new int[wm.GetStationCount()];
        backwardSequence[0] = endIndex;
        int totalSeen = 1;
        while (totalSeen != backwardSequence.Length)
        {
            // Debug.Log($"(while)::endIndex= {endIndex}  and val at that index= {sequence[endIndex]} " );
            for (int i = 1; i < sequence.Length - 1; i++)
            {
                if (sequence[i] == endIndex)
                {
                    backwardSequence[totalSeen] = i;
                    endIndex = i;
                    break;
                }
            }

            ++totalSeen;
        }
        PrintASequence(backwardSequence, "backwards");
        int[] finalSequence = new int[wm.GetStationCount()];
        for (int i = 0; i < backwardSequence.Length; i++)
        {
            //Debug.Log($"final:{i} = bs({backwardSequence.Length - 1 - i}):{backwardSequence[backwardSequence.Length - 1 - i]} ");
            finalSequence[i] = backwardSequence[backwardSequence.Length - 1 - i];
        }
        PrintASequence(finalSequence, "final");
        return finalSequence;
    }

    protected int FindPlaceInSequence(int[] sequence, int myStationID)
    {
        int index = 0;
        for (int i = 0; i < sequence.Length; i++)
        {
            if (sequence[i] == myStationID)
                return i;
        }

        return index;
    }

    protected virtual int SumSequence(int BATCHSIZE, WorkStationManager wm, WorkStation myWS, bool reqItemsOverFinalItems, bool includeSelf, bool excludeDuplicates)
    {
        int count = 0;
        int[] stationSequence = getProperSequence(wm, myWS);
        var stationList = wm.GetStationList();
        //Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        List<int> seenItems = new List<int>();
        if (!includeSelf)
            ++startingIndex;
        //Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        for (int i = startingIndex; i < stationSequence.Length; i++)
        {
            WorkStation ws = stationList[i]; /// think this is in order?
            foreach (Task t in ws._tasks)
            {
                if (reqItemsOverFinalItems)
                {
                    if (!excludeDuplicates)
                        count += t._requiredItemIDs.Count;
                    else   //verify no duplicates
                    {
                        foreach (var item in t._requiredItemIDs)
                        {
                            if (BuildableObject.Instance.IsBasicItem(item)) // cant do across board, will cause issue w OUT/IN
                            {
                                int itemId = (int)item;
                                if (_inventoryType == eInvType.STATION)
                                    Debug.Log($"chosenItemID={itemId}");
                                if (!seenItems.Contains(itemId))
                                {
                                    seenItems.Add(itemId);
                                    ++count;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (!excludeDuplicates)
                        count += t._finalItemID.Count;
                    else  //verify no duplicates
                    {
                        foreach (var item in t._finalItemID)
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
        }
        // Debug.Log($"The # of INV items will be : {count}");
        return count;
    }

    protected virtual void SetSizeOfContentArea()
    {
        if (_xMaxPerRow == 0)
            return;
        RectTransform rt = this.GetComponent<RectTransform>();


        if (GameManager.instance._batchSize == 1) ///turn off the pesky vert scroll bars
            rt.sizeDelta = new Vector2(_cellPadding, _cellPadding); ///will need to change if we add more than 1 item
        else
            rt.sizeDelta = new Vector2((_xMaxPerRow * _cellPadding) + (_cellPadding / 2), ((((_INVENTORYSIZE / _xMaxPerRow)) * _cellPadding) - (_cellPadding / 2)));

    }

    protected void TurnOffScrollBars()
    {
        if (_scrollBarVert)
            _scrollBarVert.SetActive(false);
        if (_scrollBarHoriz)
            _scrollBarHoriz.SetActive(false);
    }
    #endregion

    #region RunTimeOperations
    protected Vector2 NextSlotLocation(int slotSize)
    {
        if (_xMaxPerRow == 0)
        {
            Debug.LogWarning("max rows=0?");
            _xMaxPerRow++;
        }
        int yoff = slotSize / _xMaxPerRow;
        int xoff = slotSize % _xMaxPerRow;
        float yLoc = _startingY - (_cellPadding * yoff);
        float xLoc = _startingX + ((xoff * _cellPadding));

        /*if (_inventoryType == eInvType.STATION)
        {
            Debug.Log($"Prediction2 @{slotSize} ={_inventoryType}::XlocEND={xLoc}, ylocEND={yLoc} , xMaxRows={_xMaxPerRow}" +
            $"(Extra stuff): slotlen:{slotSize}, xMaxRows:{_xMaxPerRow} , yoff:{yoff} xoff{xoff}");
        }*/
        return new Vector2(xLoc, yLoc);

    }

    protected UIInventorySlot CreateNewSlot()
    {
        Vector2 location = Vector2.zero;
        int slotSize = -1;
        //Determine if theres any free spots in main slots 
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null)
            {
                slotSize = (i);
                break;
            }
        }
        if (slotSize == -1)
            slotSize = (_slots.Length + _extraSlots.Count);

        location = NextSlotLocation(slotSize);

        GameObject newButton = Instantiate(_bSlotPREFAB) as GameObject;
        newButton.SetActive(true);
        newButton.transform.SetParent(this.transform, false);
        newButton.transform.localPosition = new Vector3(location.x, location.y, 0);
        newButton.name = "bSlot_" + _prefix + " #" + slotSize;
        //Add slot component to our list
        var newSlot = newButton.GetComponent<UIInventorySlot>();
        //Set the slots manager:
        newSlot.SetManager(this);
        return newSlot;
    }

    public bool HasFreeSlot()
    {
        foreach (var slot in _slots)
        {
            if (slot.GetInUse() == false)
                return true;
        }
        foreach (var slot in _extraSlots)
        {
            if (slot.GetInUse() == false)
                return true;
        }
        return false;
    }

    /**Used by IN-inventory with no specific slot in mind */
    public void AddItemToSlot(int itemID, List<ObjectQuality> qualities, bool makeRequired)
    {
        //if(_inventoryType==eInvType.OUT)
        // Debug.Log($"Adding Item to slot {itemID}");

        if (!_ADDCHAOTIC)
        {
            foreach (UIInventorySlot slot in _slots)
            {
                if (TryToAdd(slot, itemID, qualities, makeRequired))
                    return;
            }
            foreach (UIInventorySlot slot in _extraSlots)
            {
                if (TryToAdd(slot, itemID, qualities, makeRequired))
                    return;
            }
            //fell thru so we are full
            Debug.Log($"we fell thru ..creating new slot q valid={qualities ==null}");
            UIInventorySlot nSlot = CreateNewSlot();
            nSlot.AssignItem(itemID, 1, qualities);
            _extraSlots.Add(nSlot);
            ++_INVENTORYSIZE;
            SetSizeOfContentArea(); ///adjust scrollable area
        }
        else
        {
            AddChaotic(itemID, qualities, makeRequired);

        }
    }

    protected void AddChaotic(int itemID, List<ObjectQuality> qualities, bool makeRequired)
    {
        List<UIInventorySlot> _available = new List<UIInventorySlot>();
        //Search through our initial slots and save any that can accept this itemID
        foreach (UIInventorySlot slot in _slots)
        {
            if (_slots != null)
            {
                if (slot.RequiresCertainID())
                {
                    if (slot.RequiredID == itemID)
                        _available.Add(slot);
                }
                else if (slot.GetInUse())
                {
                    _available.Add(slot);
                }
            }

        }
        //If we didnt find any in main, search our extra slots
        if (_available.Count == 0)
        {
            foreach (UIInventorySlot slot in _extraSlots)
            {
                if (_slots != null)
                {
                    if (slot.RequiresCertainID())
                    {
                        if (slot.RequiredID == itemID)
                            _available.Add(slot);
                    }
                    else if (slot.GetInUse())
                    {

                        _available.Add(slot);
                    }
                }
            }
        }

        //Add to a random available slot
        if (_available.Count > 0)
        {
            if (makeRequired)
                _available[UnityEngine.Random.Range(0, _available.Count - 1)].SetRequiredID(itemID);
            else
                _available[UnityEngine.Random.Range(0, _available.Count - 1)].AssignItem(itemID, 1, qualities );
        }
        else //create an additional slot to add to 
        {
            UIInventorySlot nSlot = CreateNewSlot();
            if (makeRequired)
                nSlot.SetRequiredID(itemID);
            else
                nSlot.AssignItem(itemID, 1, qualities);

            _extraSlots.Add(nSlot);

        }
    }

    protected bool TryToAdd(UIInventorySlot slot, int itemID, List<ObjectQuality> qualities, bool makeRequired)
    {
        if (!slot.GetInUse())
        {
            if (makeRequired)
            {
                if (!slot.RequiresCertainID())
                {
                    slot.SetRequiredID(itemID);
                    return true;
                }
            }
            else //normal add 
            {
                if (slot.RequiresCertainID())
                {
                    if (slot.RequiredID == itemID)
                    {
                        slot.AssignItem(itemID, 1, qualities);
                        return true;
                    }
                   
                }
                else
                {
                    slot.AssignItem(itemID, 1, qualities);
                    return true;
                }
            }
        }

        return false;
    }

    public void SetImportant(GameObject button)
    {
        button.transform.SetAsLastSibling();
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
        if (_optionalSendButton)
            _optionalSendButton.interactable = true;


    }

    public void SendBatch()
    {
        Debug.Log("heared send batch");
        foreach (var slot in _slots)
        {
            slot.SendData();
        }
        if (_optionalSendButton)
            _optionalSendButton.interactable = false;
    }

    #endregion
}

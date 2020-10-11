using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


//https://www.youtube.com/watch?v=Oba1k4wRy-0 //Tutorial
public class UIInventoryManager : MonoBehaviour
{
    #region GameManager Parameters
    private int _INVENTORYSIZE;
    private bool _STACKABLE;
    private bool _ADDCHAOTIC;
    #endregion
    [SerializeField] GameObject _bSlotPREFAB;
    //[SerializeField] GridLayoutGroup _layoutGroup;
    //[SerializeField] Sprite[] _iconSprites;
    public enum eInvType { IN, OUT, STATION };
    [SerializeField] eInvType _inventoryType;
    private UIInventorySlot[] _slots;

    private int _xMaxRows;
    private int _cellPadding = 75;
    private float _startingX = 37.5f;
    private float _startingY = -37.5f;

  
    #region setup
    private void Start()
    {
        if (_bSlotPREFAB == null)
            _bSlotPREFAB = Resources.Load<GameObject>("Prefab/UI/bSlot");
        GetGameManagerData();
        GenInventory();
        //Debug.LogWarning("(s)SLOTS SIZE=" + _slots.Length);
    }

    private void GetGameManagerData()
    {
        _INVENTORYSIZE = DetermineWorkStationBatchSize2();
        _STACKABLE = GameManager.instance._isStackable;
        _ADDCHAOTIC = GameManager.instance._addChaotic;
        if (_inventoryType == eInvType.IN)
            GameManager.instance.SetInventoryIn(this);
        else if (_inventoryType == eInvType.OUT)
            GameManager.instance.SetInventoryOut(this);
        else
            GameManager.instance.SetInventoryStation(this);
    }


    /************************************************************************************************************/
    #region batchSizeMethods
    private int DetermineWorkStationBatchSizeOLD()
    {
        //TMP need to calculate batch sizes based on workstation tasks
        if (_inventoryType == eInvType.IN)
            return GameManager.instance._inventorySize;
        else if (_inventoryType == eInvType.OUT)
            return GameManager.instance._inventorySize;
        else
        {
            int amnt = 0;
            WorkStation ws = GameManager.instance._workStation;
            foreach (Task t in ws._tasks)
                amnt += t._requiredItemIDs.Count;
            //Debug.Log("the amount of req items for this station is " + amnt);
            return amnt;
        }
    }

    private void PrintASequence(int[] sequence, string seqName)
    {
        string p = "";
        for (int i = 0; i < sequence.Length ; ++i)
        {
            p += $" , {sequence[i]}";
        }
       // Debug.Log(seqName+ ": " + p);
    }
    /** This is kind of a mess, thinking of making a doubly linked list class at some point*/
    private int[] getProperSequence(WorkStationManager wm, WorkStation myWS)
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

    private int FindPlaceInSequence(int[] sequence , int myStationID)
    {
        int index = 0;
        for (int i = 0; i < sequence.Length; i++)
        {
            if (sequence[i] == myStationID)
                return i;
        }

        return index;
    }
    private int DetermineWorkStationBatchSize2()
    {
        WorkStationManager wm = UIManager.instance._workstationManager;
        bool test = false;
        if (_inventoryType == eInvType.IN || test)
        {
            int BATCHSIZE = GameManager.instance._batchSize;
            WorkStation myWS = GameManager.instance._workStation;
            int count = 0;
            //if batch size =1 , then IN = # of required Items produced previous station
            if (BATCHSIZE==1) //assume batchsize=1 enabled stackable Inv and StationINV is turned on
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
                    Debug.LogWarning("Does this happen?");

                return count * BATCHSIZE;
            }
            else //Sum the total required items from self + all subseqential workstations, and * BATCH_SIZE
            {
                int[] stationSequence = getProperSequence(wm, myWS);//really feels like a doubly linked list might be better?
                var stationList = wm.GetStationList();
                //Figure out myplace in Sequence 
                int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
                Debug.Log(myWS._myStation + " @ "+ (int)myWS._myStation +"  id  is at index in sequence= " + startingIndex);
                for (int i = startingIndex; i < stationSequence.Length; i++)
                {
                    WorkStation ws = stationList[i]; /// think this is in order?
                    foreach (Task t in ws._tasks)
                    {
                        count += t._requiredItemIDs.Count;
                    }
                }
                Debug.Log($"The # of IN items will be : {count}");
                return count* BATCHSIZE;
            }
       
        }
        else if (_inventoryType == eInvType.OUT)
        {
            int BATCHSIZE = GameManager.instance._batchSize;
            WorkStation myWS = GameManager.instance._workStation;
            int count = 0;
            //if batch size =1 , then IN = # of produced Items at station
            if (BATCHSIZE == 1) //assume batchsize=1 enabled stackable Inv and StationINV is turned on
            {
                foreach (Task t in myWS._tasks)
                    count += t._finalItemID.Count;

                return count;
            }
            else //Sum the total required items (not self) all subseqential workstations, and * BATCH_SIZE
            {
                int[] stationSequence = getProperSequence(wm, myWS);//really feels like a doubly linked list might be better?
                var stationList = wm.GetStationList();
                //Figure out myplace in Sequence 
                int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
                Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
                for (int i = startingIndex; i < stationSequence.Length; i++)
                {
                    WorkStation ws = stationList[i]; /// think this is in order?
                    foreach (Task t in ws._tasks)
                    {
                        count += t._requiredItemIDs.Count;
                    }
                }
                Debug.Log($"The # of IN items will be : {count}");
                return count * BATCHSIZE;
            }
            

        }
        else
        {
            int BATCHSIZE = GameManager.instance._batchSize;
            WorkStation myWS = GameManager.instance._workStation;
            int count = 0;
            //if batch size ==1 then just the required # of items at this station (pull)
            if (BATCHSIZE==1)
            {
                foreach (Task t in myWS._tasks)
                    count += t._requiredItemIDs.Count;

                return count;
            }
            //else were kitting and its 
            else  //Sum the total required items from  all subseqential workstations (not mult BATCH_SIZE cuz INF slots)
            {
                /*int[] stationSequence = getProperSequence(wm, myWS);//really feels like a doubly linked list might be better?
                var stationList = wm.GetStationList();
                //Figure out myplace in Sequence 
                int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
                Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
                for (int i = startingIndex+1; i < stationSequence.Length; i++)
                {
                    WorkStation ws = stationList[i]; /// think this is in order?
                    foreach (Task t in ws._tasks)
                    {
                        count += t._requiredItemIDs.Count;
                    }
                }
                Debug.Log($"The # of INV items will be : {count}");*/
                return SumSequence(BATCHSIZE, wm, myWS, true, false);
            }

          
        }

    }
    private int SumSequence(int BATCHSIZE, WorkStationManager wm, WorkStation myWS, bool reqItemsOverFinalItems, bool includeSelf)
    {
        int count = 0;
        int[] stationSequence = getProperSequence(wm, myWS);
        var stationList = wm.GetStationList();
        //Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        if (!includeSelf)
            ++startingIndex;
        Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        for (int i = startingIndex ; i < stationSequence.Length; i++)
        {
            WorkStation ws = stationList[i]; /// think this is in order?
            foreach (Task t in ws._tasks)
            {
                
                if(reqItemsOverFinalItems)
                    count += t._requiredItemIDs.Count;
                else
                    count += t._finalItemID.Count;
            }
        }
        Debug.Log($"The # of INV items will be : {count}");
        return count;
    }
    #endregion
    /************************************************************************************************************/


    private void GenInventory()
    {
        if (DisableIfNotStackableAndNotKitting())
            return;
        
        _slots = new UIInventorySlot[_INVENTORYSIZE];
        Debug.LogError($"{_inventoryType} slotsize ={ _slots.Length}");

        //Determine layout
        if (_INVENTORYSIZE > 4)
            _xMaxRows = (_INVENTORYSIZE / 4) + 1;
        else
            _xMaxRows = _INVENTORYSIZE;

        if (_xMaxRows > 10)
            _xMaxRows = 10;

        //Debug.Log($"{this.transform.gameObject.name}{_inventoryType}, {_INVENTORYSIZE} resulted in {_xMaxRows}");

        //Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        //cache a conditions for forloop situations
        bool cond = GameManager.instance._autoSend; //used By eInvType.OUT
        Dictionary<Task, int> _seenTasks = new Dictionary<Task, int>(); //used By eInvType.STATION
        WorkStationManager wm = UIManager.instance._workstationManager;
        WorkStation myWS = GameManager.instance._workStation;
        //getAPrefix for naming our buttons in scene Hierarchy
        string prefix;
        if (_inventoryType == eInvType.IN)
            prefix = "in_";
        else if (_inventoryType == eInvType.OUT)
            prefix = "out_";
        else
            prefix = "station_";


        float xLoc = _startingX;
        float yLoc = _startingY;
        int loopOffset = 1;
        if (_inventoryType == eInvType.STATION)
            loopOffset = 0;
        for (int i = loopOffset; i < _INVENTORYSIZE- loopOffset; ++i)
        {
            GameObject newButton = Instantiate(_bSlotPREFAB) as GameObject;
            newButton.SetActive(true);
            newButton.transform.SetParent(this.transform, false);
            newButton.transform.localPosition = new Vector3(xLoc, yLoc, 0);
            //Update COL/ROWs
            xLoc += _cellPadding;
            if (i % _xMaxRows == 0 && _inventoryType != eInvType.STATION) //turned off for bot UI for now , just be 1 row since I cant make this work for all 3 cases
            {
                yLoc -= _cellPadding;
                xLoc = _startingX;
            }
            //Rename in scene hierarchy
            newButton.name = "bSlot_" + prefix + " #" + i;
            //Add slot component to our list
            _slots[i+ loopOffset] = newButton.GetComponent<UIInventorySlot>();
            //Set the slots manager:
            _slots[i+ loopOffset].SetManager(this);
            if (_inventoryType == eInvType.OUT) //Set our out slots to auto send or not
                _slots[i+ loopOffset].SetAutomatic(cond);
            else if (_inventoryType == eInvType.STATION) //Set our infinite station items
                SetInfiniteItem(wm, myWS, _seenTasks, _slots, i+ loopOffset);
        }
    }

    private bool  DisableIfNotStackableAndNotKitting()
    {
        if (_inventoryType == eInvType.STATION && !_STACKABLE)
        {
            var ws = GameManager.instance._workStation;
            if (!ws.isKittingStation())
            {
                Destroy(this.gameObject); //good enough for now might need to go higher
                Debug.Log($"{ws._stationName}  is not a kitting station??? {ws.isKittingStation()}" );
                return true;
            }
        }
        return false;
    }

    private void SetInfiniteItem(WorkStationManager wm, WorkStation myWS, Dictionary<Task, int> _seenTasks, UIInventorySlot[] _slots, int i)
    {
        //Debug.Log($"SetINF@{i} , slotsize={_slots.Length}");

        //need to look at the tasks in all workstations after me  (ignore self because required items in next staton are same as mine)
        int[] stationSequence = getProperSequence(wm, myWS);//really feels like a doubly linked list might be better?
        var stationList = wm.GetStationList();
        //Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        for (int j = startingIndex+1; j < stationSequence.Length; j++)
        {
            WorkStation ws = stationList[j]; /// think this is in order?
            foreach (Task t in ws._tasks)
            {
                if (_seenTasks.ContainsKey(t))
                {
                    int amntOfItemsSeen = _seenTasks[t];
                    int requiredItemsInTask = t._requiredItemIDs.Count;
                    if (amntOfItemsSeen < requiredItemsInTask)
                    {
                        _seenTasks.Remove(t);
                        _seenTasks.Add(t, amntOfItemsSeen + 1);
                        //Debug.Log($"Wtf is going on here ( {amntOfItemsSeen}, {t._requiredItemIDs[amntOfItemsSeen]})");
                        int id = (int)t._requiredItemIDs[amntOfItemsSeen];
                        Debug.Log($"(1) Assigned Item ID {id}, from Station:{ws}::task::{t}::item::{t._requiredItemIDs[amntOfItemsSeen]}");
                        _slots[i].AssignItem(id, int.MaxValue);
                        return;
                    }
                }
                else
                {
                    _seenTasks.Add(t, 1);
                    int id = (int)t._requiredItemIDs[0];
                    Debug.Log($"(0) Assigned Item ID {id}, from Station:{ws}::task::{t}::item::{t._requiredItemIDs[0]}");
                    _slots[i].AssignItem(id, int.MaxValue);
                    return;
                }
            }
        }
        /*
        foreach (Task t in tasks)
        {
            if (_seenTasks.ContainsKey(t))
            {
                int amountSeen = _seenTasks[t];
                if (amountSeen <= t._requiredItemIDs.Count)
                {
                    _seenTasks.Remove(t);
                    _seenTasks.Add(t, amountSeen + 1);
                    Debug.Log($"Wtf is going on here ( {amountSeen}, {t._requiredItemIDs[amountSeen]})");
                    int id = (int)t._requiredItemIDs[amountSeen];
                    _slots[i].AssignItem( id, int.MaxValue);
                    return;
                }
            }
            else
            {
                _seenTasks.Add(t, 1);
                int id = (int)t._requiredItemIDs[0];
                _slots[i].AssignItem( id, int.MaxValue);
                return;
            }
        }*/

    }

    private void SetSizeOfContentArea()
    {
        RectTransform rt = this.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2((_xMaxRows * _cellPadding) + (_cellPadding / 2), (((_INVENTORYSIZE / _xMaxRows) * _cellPadding) + (_cellPadding / 2)));

    }



    #endregion


    public bool HasFreeSlot()
    {
        foreach (var slot in _slots)
        {
            if (slot.GetInUse() == false)
                return true;
        }
        return false;
    }


    public void AddItemToSlot(int itemID)
    {
        if (!_ADDCHAOTIC)
        {
            foreach (UIInventorySlot slot in _slots)
            {
                if (!slot.GetInUse())
                {
                    slot.AssignItem( itemID, 1);
                    return;
                }
            }
        }
        else
        {
            List<UIInventorySlot> _available = new List<UIInventorySlot>();
            foreach (UIInventorySlot slot in _slots)
            {
                if (slot != null && !slot.GetInUse())
                    _available.Add(slot);
            }
            _available[UnityEngine.Random.Range(0, _available.Count - 1)].AssignItem( itemID, 1);
        }
    }

    public void SetImportant(GameObject button)
    {
        button.transform.SetAsLastSibling();
    }
}

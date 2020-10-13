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
    GameObject _bSlotPREFAB;
    //[SerializeField] GridLayoutGroup _layoutGroup;
    //[SerializeField] Sprite[] _iconSprites;
    public enum eInvType { IN, OUT, STATION };
    [SerializeField] eInvType _inventoryType;
    private Button _optionalSendButton;
    private UIInventorySlot[] _slots;
    private List<UIInventorySlot> _extraSlots; //incase we want to reset to base amount

    private int _xMaxRows;
    private int _cellPadding = 75;
    private float _startingX = 37.5f;
    private float _startingY = -37.5f;

    private string _prefix;

    /** Note: If world canvas is turned on at start of scene this script will bug out because work station is not assigned yet
    * Can solve this later if it becomes a real problem */
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
                s.AssignItem(1,1);

            AddItemToSlot(2);
        }
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
                return SumSequence(BATCHSIZE, wm, myWS, true, true) * BATCHSIZE;
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
                return SumSequence(BATCHSIZE, wm, myWS, true, false) * BATCHSIZE;
            }


        }
        else
        {
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
                return SumSequence(BATCHSIZE, wm, myWS, true, false);
            }


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

    private int SumSequence(int BATCHSIZE, WorkStationManager wm, WorkStation myWS, bool reqItemsOverFinalItems, bool includeSelf)
    {
        int count = 0;
        int[] stationSequence = getProperSequence(wm, myWS);
        var stationList = wm.GetStationList();
        //Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        if (!includeSelf)
            ++startingIndex;
        //Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
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
       // Debug.Log($"The # of INV items will be : {count}");
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
        if (_inventoryType == eInvType.IN)
            _prefix = "in_";
        else if (_inventoryType == eInvType.OUT)
            _prefix = "out_";
        else
            _prefix = "station_";

        //Any slots added after this will be kept track of in an extra list incase we ever want to reset to base amount
        _extraSlots = new List<UIInventorySlot>(); //Instantiated before for loop becuz CreateNewslot uses its Count

        for (int i = 0; i < _INVENTORYSIZE; ++i)
        {
            //Add slot component to our list
            _slots[i] = CreateNewSlot();
    
            if (_inventoryType == eInvType.OUT) //Set our out slots to auto send or not
                _slots[i].SetAutomatic(cond);
            else if (_inventoryType == eInvType.STATION) //Set our infinite station items
                SetInfiniteItem(wm, myWS, _seenTasks, _slots, i);
        }
        if (_inventoryType == eInvType.IN)
            SetUpStartingItems();
        else if (_inventoryType == eInvType.OUT)
            SetUpBatchOutput(wm, myWS);
    }

    private bool  NotStackableAndNotKitting()
    {
        if (_inventoryType == eInvType.STATION && !_STACKABLE)
        {
            var ws = GameManager.instance._workStation;
            if (!ws.isKittingStation())
            {
                Destroy(this.gameObject); //good enough for now might need to go higher to parents
                Debug.Log($"{ws._stationName}  is not a kitting station??? {ws.isKittingStation()}" );
                return true;
            }
        }
        return false;
    }

    /**Sets the stackable items at this stations inventory based off the required items needed at this station */
    private void SetInfiniteItem(WorkStationManager wm, WorkStation myWS, Dictionary<Task, int> _seenTasks, UIInventorySlot[] _slots, int i)
    {
        //Debug.Log($"SetINF@{i} , slotsize={_slots.Length}");

        //need to look at the tasks in all workstations after me  (ignore self because required items in next staton are same as mine)
        int[] stationSequence = getProperSequence(wm, myWS);//really feels like a doubly linked list might be better?
        var stationList = wm.GetStationList();
        //Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
       // Debug.Log(myWS._myStation + " @ " + (int)myWS._myStation + "  id  is at index in sequence= " + startingIndex);
        if(GameManager.instance._batchSize==1)
        {
            WorkStation mwWS = GameManager.instance._workStation;
            SortBySelfStations(myWS, _seenTasks, i);
        }
        else
        {
            if (!GameManager.instance._workStation.isKittingStation())
                Debug.LogWarning("This happened for a non kitting station, dont think this shud");

            SortByAllStations(startingIndex, stationSequence,  stationList, _seenTasks, i);
        }
    }
    private bool SortByAllStations(int startingIndex, int[] stationSequence, List<WorkStation> stationList, Dictionary<Task, int> _seenTasks, int i )
    {
        for (int j = startingIndex + 1; j < stationSequence.Length; j++)
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
                        //Debug.Log($"(1) Assigned Item ID {id}, from Station:{ws}::task::{t}::item::{t._requiredItemIDs[amntOfItemsSeen]}");
                        _slots[i].AssignItem(id, int.MaxValue);
                        return true;
                    }
                }
                else
                {
                    _seenTasks.Add(t, 1);
                    int id = (int)t._requiredItemIDs[0];
                    //Debug.Log($"(0) Assigned Item ID {id}, from Station:{ws}::task::{t}::item::{t._requiredItemIDs[0]}");
                    _slots[i].AssignItem(id, int.MaxValue);
                    return true;
                }
            }
        }
        return false;
    }
    private bool SortBySelfStations( WorkStation ws, Dictionary<Task, int> _seenTasks, int i)
    {
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
                    //Debug.Log($"(1) Assigned Item ID {id}, from Station:{ws}::task::{t}::item::{t._requiredItemIDs[amntOfItemsSeen]}");
                    _slots[i].AssignItem(id, int.MaxValue);
                    return true;
                }
            }
            else
            {
                _seenTasks.Add(t, 1);
                int id = (int)t._requiredItemIDs[0];
                //Debug.Log($"(0) Assigned Item ID {id}, from Station:{ws}::task::{t}::item::{t._requiredItemIDs[0]}");
                _slots[i].AssignItem(id, int.MaxValue);
                return true;
            }
        }
        return false;
    }
    /**Determines the size of the content area based on how many items/rows we have. The overall size affects scrolling */
    private void SetSizeOfContentArea()
    {
        if (_xMaxRows == 0)
            return;
        RectTransform rt = this.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2((_xMaxRows * _cellPadding) + (_cellPadding / 2), (((_INVENTORYSIZE / _xMaxRows) * _cellPadding) + (_cellPadding / 2)));

    }

    private void SetUpStartingItems()
    {
        int BATCHSIZE = GameManager.instance._batchSize;
        foreach(var task in GameManager.instance._workStation._tasks)
        {
            int count = BATCHSIZE;
            foreach (var item in task._requiredItemIDs)
            {
                for (int i = 0; i < count; i++)
                {
                    Debug.Log("Add item" +(int)item );
                    AddItemToSlot((int)item); //seems like this is going to the OUT slots???
                }
               
            }
        }
    }
    private void SetUpBatchOutput(WorkStationManager wm, WorkStation myWS)
    {
        //Need to assign (# of items produced at this station + # of items required at subsequent stations) *BATCHSIZE
        int[] stationSequence = getProperSequence(wm, myWS);//really feels like a doubly linked list might be better?
        var stationList = wm.GetStationList();
        //Figure out myplace in Sequence 
        int startingIndex = FindPlaceInSequence(stationSequence, (int)myWS._myStation);
        int BATCHSIZE = GameManager.instance._batchSize;
        for (int i = startingIndex; i < stationList.Count; i++)
        {
            WorkStation ws = stationList[i];
            foreach (var task in ws._tasks)
            {
                if(ws!=myWS)
                {
                    foreach (var item in task._requiredItemIDs)
                    {
                        for (int j = 0; j < BATCHSIZE; j++)
                        {
                            AddItemToSlot((int)item);
                        }
                    }
                }
                else
                {
                    foreach (var item in task._finalItemID)
                    {
                        for (int j = 0; j < BATCHSIZE; j++)
                        {
                            AddItemToSlot((int)item);
                        }
                    }
                }
            }
        }
    }


    #endregion

    #region RunTimeOperations
    private Vector2 NextSlotLocation(int slotSize)
    {
        if(_xMaxRows==0)
        {
            Debug.LogWarning("max rows=0?");
            _xMaxRows++;
        }
        int yoff = slotSize / _xMaxRows;
        int xoff = slotSize % _xMaxRows;
        float yLoc = _startingY - (_cellPadding*yoff);
        float xLoc = _startingX + ((xoff* _cellPadding));
        /*
            Debug.Log($"Prediction2 @{slotSize} ={_inventoryType}::XlocEND={xLoc}, ylocEND={yLoc} , xMaxRows={_xMaxRows}" +
            $"(Extra stuff): slotlen:{slotSize}, xMaxRows:{_xMaxRows} , yoff:{yoff} xoff{xoff}");
        */
        return new Vector2(xLoc, yLoc);

    }
  
    private UIInventorySlot CreateNewSlot()
    {
        Vector2 location = Vector2.zero;
        int slotSize=-1;
        //Determine if theres any free spots in main slots 
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null)
            {
                slotSize=(i);
                break;
            }
        }
        if(slotSize==-1)
            slotSize =(_slots.Length + _extraSlots.Count);

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
        foreach(var slot in _extraSlots)
        {
            if (slot.GetInUse() == false)
                return true;
        }
        return false;
    }

    /**Used by IN-inventory with no specific slot in mind */
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
            foreach (UIInventorySlot slot in _extraSlots)
            {
                if (!slot.GetInUse())
                {
                    slot.AssignItem(itemID, 1);
                    return;
                }
            }
            //fell thru so we are full
            UIInventorySlot nSlot = CreateNewSlot();
            nSlot.AssignItem(itemID, 1);
            _extraSlots.Add(nSlot);
        }
        else
        {
            List<UIInventorySlot> _available = new List<UIInventorySlot>();
            foreach (UIInventorySlot slot in _slots)
            {
                if (slot != null && !slot.GetInUse())
                    _available.Add(slot);
            }
            if (_available.Count > 0)
                _available[UnityEngine.Random.Range(0, _available.Count - 1)].AssignItem(itemID, 1);
            else
            {
                foreach (UIInventorySlot slot in _extraSlots)
                {
                    if (slot != null && !slot.GetInUse())
                        _available.Add(slot);
                }
                if (_available.Count > 0)
                    _available[UnityEngine.Random.Range(0, _available.Count - 1)].AssignItem(itemID, 1);
                else
                {
                    UIInventorySlot nSlot = CreateNewSlot();
                    nSlot.AssignItem(itemID, 1);
                    _extraSlots.Add(nSlot);
                }
            }
           
        }
    }

    public void SetImportant(GameObject button)
    {
        button.transform.SetAsLastSibling();
    }

    /** When an item gets assigned to the batch tell the manager*/
    public void CheckIfBatchIsReady()
    {
        foreach (var slot in _slots)
        {
            if (!slot.GetInUse())
            {
                if(_optionalSendButton)
                    _optionalSendButton.interactable = false;
                return;
            }
        }
        //If all buttons hold the correct items , we can send
        if (_optionalSendButton)
            _optionalSendButton.interactable = true;


    }

    public void SendBatch()
    {

    }

    #endregion
}

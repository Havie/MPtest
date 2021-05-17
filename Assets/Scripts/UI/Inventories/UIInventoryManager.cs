using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//https://www.youtube.com/watch?v=Oba1k4wRy-0 //Tutorial
[DefaultExecutionOrder(10000)] ///make this load late to let other things get set up first
public abstract class UIInventoryManager : MonoBehaviour, IInventoryManager
{
    /// <summary>
    /// I realllly want to re-write this class with dependency injection,
    /// and tweak some other design problems,
    /// but gotta keep moving the project along, and not fix what isnt broken
    /// </summary>

    [Header("Components")]
    [SerializeField] protected Image _gridParent;
    [SerializeField] protected UIDeadZone _deadZone;
    [SerializeField] protected GridLayoutGroup _gridLayoutGrp;
    [SerializeField] protected TextMeshProUGUI _labelText;
    protected GameObject _bSlotPREFAB;


    [Header("Specifications")]
    [SerializeField] protected int _maxHeight = 600; ///Probably shouldnt touch
    [SerializeField] protected int _widthPadding = 73;
    [SerializeField] protected int _heightPadding = 77;
    [SerializeField] protected float _minCellSize = 75f;
    [SerializeField] protected float _maxCellSize = 125f;
    [SerializeField] protected float _cellScaler = 5f;

    private float _gridCellWidth;
    private float _gridCellHeight;
    private int _numberOfColumns;
    private int _numberOfRows = 1;
    private int _currColCount = 0;

    #region GameManager Parameters
    protected int _INVENTORYSIZE;
    protected bool _STACKABLE;
    protected bool _ADDCHAOTIC;
    protected int _batchSize;
    #endregion

    protected UIInventorySlot[] _slots = new UIInventorySlot[0];
    protected List<UIInventorySlot> _extraSlots = new List<UIInventorySlot>(); //incase we want to reset to base amount
    protected bool _canAssignExtraSlots = true;
    protected string _prefix;

    /************************************************************************************************************************/
    public bool IsInitalized { get; protected set; }


    protected virtual void Start()
    {
        if (_gridLayoutGrp == null)
            _gridLayoutGrp = this.GetComponentInChildren<GridLayoutGroup>(false);

        ReconfigureGLG();

        if (_bSlotPREFAB == null)
            _bSlotPREFAB = Resources.Load<GameObject>("Prefab/UI/bSlot");

        var gm = GameManager.Instance;
        _STACKABLE = gm._isStackable;
        _ADDCHAOTIC = gm._addChaotic;
        _canAssignExtraSlots = gm._batchSize != 1;
        GenerateInventory(DetermineWorkStationBatchSize());
    }

    /************************************************************************************************************************/
    #region Helper Initilization Methods for extended classes
    protected abstract List<int> DetermineWorkStationBatchSize();
    protected abstract void GenerateInventory(List<int> itemIDs);

    private void ReconfigureGLG()
    {
        if (_gridLayoutGrp == null)
            return;

        var slotCount = _slots.Length + _extraSlots.Count;
        var batchSize = GameManager.instance._batchSize;

        if (slotCount == 1)
        {
            _gridLayoutGrp.constraintCount = 1;
            _gridLayoutGrp.cellSize = new Vector2(_maxCellSize, _maxCellSize);
        }
        else
        {
            float cellSize = 0;

            _gridLayoutGrp.constraintCount = 2;
            cellSize = _minCellSize;

            //Debug.Log($"SlotCount={slotCount}");
            //if (slotCount < 5)
            //{
            //    _gridLayoutGrp.constraintCount = 2;
            //    cellSize = _maxCellSize;
            //}
            //else
            //{
            //    _gridLayoutGrp.constraintCount = 3;
            //    cellSize = _minCellSize;
            //}
            //float cellSize = _maxCellSize - (_cellScaler * slotCount);
            //Debug.Log($"made up cellsize for {slotCount} ={cellSize}");

            //if (cellSize < _minCellSize)
            //    cellSize = _minCellSize;

            _gridLayoutGrp.cellSize = new Vector2(cellSize, cellSize);
            //Debug.Log($"Set cellSize to {cellSize}");
        }

        _numberOfColumns = _gridLayoutGrp.constraintCount;
        _gridCellWidth = _gridLayoutGrp.cellSize.x;
        _gridCellHeight = _gridLayoutGrp.cellSize.y;
    }
    /**Determines the size of the content area based on how many items/rows we have. The overall size affects scrolling */
    protected virtual void SetSizeOfContentArea()
    {
        ReconfigureGLG();
        // 73 is the combined width of element padding and scrollbar
        float parentWidth = (_gridCellWidth * _numberOfColumns) + _widthPadding;
        // 77 is the combined height of element padding and text label
        float parentHeight = (_gridCellHeight * _numberOfRows) + _heightPadding;

        // activates scroll bar if element is too tall
        if (parentHeight > _maxHeight)
        {
            parentHeight = _maxHeight;
        }

        //Debug.Log($"<color=blue>_gridCellWidth= {_gridCellWidth}, _numberOfColumns={_numberOfColumns} </color>," +
        //    $"<color=yellow>_gridCellHeight= {_gridCellHeight}, _numberOfRows={_numberOfRows}</color>" + $"<color=blue>_currColCount= {_currColCount} </color>");

        //Debug.Log($"maxH= {_maxHeight} vs parentHeight={parentHeight}, Rect={new Vector2(parentWidth, parentHeight)}");
        // sets calculated width and height
        var newSize = new Vector2(parentWidth, parentHeight);
        _gridParent.rectTransform.sizeDelta = newSize;
        _deadZone.TryScaleSizeWithInventory(newSize);


    }
    #endregion

    #region RunTimeOperations

    protected UIInventorySlot CreateNewSlot()
    {

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

        GameObject newButton = Instantiate(_bSlotPREFAB);
        newButton.SetActive(true);
        newButton.transform.SetParent(_gridLayoutGrp.transform);
        newButton.transform.localScale = new Vector3(1, 1, 1);
        newButton.name = "bSlot_" + _prefix + " #" + slotSize;
        //Add slot component to our list
        var newSlot = newButton.GetComponent<UIInventorySlot>();
        //Set the slots manager:
        newSlot.SetManager(this);
        SetSizeOfContentArea();

        if (_currColCount == _numberOfColumns && _numberOfColumns != 1)
        {
            _numberOfRows++;
            _currColCount = 0;
        }
        _currColCount++;

        return newSlot;
    }

    public bool HasFreeSlot()
    {
        foreach (var slot in _slots)
        {
            if (slot.GetInUse() == false)
                return true;
        }
        if (_canAssignExtraSlots)
        {
            foreach (var slot in _extraSlots)
            {
                if (slot.GetInUse() == false)
                    return true;
            }
        }
        return false;
    }

    /**Used by all inventories initially to make required, and by IN-inventory with no specific slot in mind */
    public bool AddItemToSlot(int itemID, List<QualityObject> qualities, bool makeRequired)
    {

        if (!IsInitalized)
            Start();

        if (_ADDCHAOTIC)
        {
            return AddChaotic(itemID, qualities, makeRequired);
        }
        else //Normal add
        {
            foreach (UIInventorySlot slot in _slots)
            {
                if (TryToAdd(slot, itemID, qualities, makeRequired))
                    return true;
            }
            if (_canAssignExtraSlots)
            {
                foreach (UIInventorySlot slot in _extraSlots)
                {
                    if (TryToAdd(slot, itemID, qualities, makeRequired))
                        return true;
                }
                //fell thru so we are full
                //Debug.Log($"we fell thru ..creating new slot q valid={qualities == null}");
                UIInventorySlot nSlot = CreateNewSlot();
                nSlot.AssignItem(itemID, 1, qualities);
                _extraSlots.Add(nSlot);
                ++_INVENTORYSIZE; ///Not even sure this matters anymore past initial slot creation
                SetSizeOfContentArea(); ///adjust scrollable area
                return true;
            }

        }

        return false;
    }

    protected bool AddChaotic(int itemID, List<QualityObject> qualities, bool makeRequired)
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
                else if (!slot.GetInUse())
                    _available.Add(slot);
            }

        }
        //If we didnt find any in main, search our extra slots
        if (_available.Count == 0 && _canAssignExtraSlots)
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
                _available[UnityEngine.Random.Range(0, _available.Count - 1)].AssignItem(itemID, 1, qualities);
            return true;
        }
        else if (_canAssignExtraSlots) //create an additional slot to add to 
        {
            UIInventorySlot nSlot = CreateNewSlot();
            if (makeRequired)
                nSlot.SetRequiredID(itemID);
            else
                nSlot.AssignItem(itemID, 1, qualities);

            _extraSlots.Add(nSlot);
            return true;

        }
        return false;
    }

    protected bool TryToAdd(UIInventorySlot slot, int itemID, List<QualityObject> qualities, bool makeRequired)
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
        // button.transform.SetAsLastSibling();
    }

    public virtual void SlotStateChanged(UIInventorySlot slot) { }

    public int MaxSlots()
    {
        return _slots.Length;
    }

    public int SlotsInUse()
    {
        int count = 0;
        foreach (var item in _slots)
        {
            if (item !=null && item._inUse)
                ++count;
        }

        return count;
    }

    public List<UIInventorySlot> GetAllSlotsInUse()
    {
        if (!IsInitalized)
            Start();
        List<UIInventorySlot> retList = new List<UIInventorySlot>();
        foreach (var item in _slots)
        {
            if (item.GetInUse())
                retList.Add(item);
        }
        foreach (var item in _extraSlots)
        {
            if (item.GetInUse())
                retList.Add(item);
        }
        return retList;
    }

    public bool TryAssignItem(int id, int count, List<QualityObject> qualities)
    {
        ///Some problems here with COUNT
        return AddItemToSlot(id, qualities, false);
    }
    
    public void KanbanInventoryChanged(bool isEmpty, int itemID, List<QualityObject> qualityData)
    {
        ///NB: should only have 1 slot in kanban , but if sim changes, we can use the itemID to look
        ///thru the other slots and find the one with the right slot.RequiredItemID
        _slots[0].SharedKanbanSlotChanged(isEmpty, qualityData);
    }
    #endregion
}




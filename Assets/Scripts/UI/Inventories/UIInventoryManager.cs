using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//https://www.youtube.com/watch?v=Oba1k4wRy-0 //Tutorial
[DefaultExecutionOrder(10000)] ///make this load late to let other things get set up first
public abstract class UIInventoryManager : MonoBehaviour
{
    /// <summary>
    /// I realllly want to re-write this class with dependency injection,
    /// and tweak some other design problems,
    /// but gotta keep moving the project along, and not fix what isnt broken
    /// </summary>

    [Header("Components")]
    [SerializeField] protected Image _gridParent;
    [SerializeField] protected GridLayoutGroup _gridLayoutElement;
    [SerializeField] protected TextMeshProUGUI _labelText;
    protected GameObject _bSlotPREFAB;


    [Header("Specifications")]
    [SerializeField] protected int _maxHeight =350;
    [SerializeField] protected int _widthPadding = 73;
    [SerializeField] protected int _heightPadding = 77;

    private float _gridCellWidth;
    private float _gridCellHeight;
    private int _numberOfColumns;
    private int _numberOfRows;
    private int _currColCount = 0;

    #region GameManager Parameters
    protected int _INVENTORYSIZE;
    protected bool _STACKABLE;
    protected bool _ADDCHAOTIC;
    #endregion

    protected UIInventorySlot[] _slots;
    protected List<UIInventorySlot> _extraSlots; //incase we want to reset to base amount

    protected string _prefix;


    /************************************************************************************************************************/
    public bool IsInitalized { get; protected set; }


    protected virtual void Start()
    {
        _numberOfColumns = _gridLayoutElement.constraintCount;
        _gridCellWidth = _gridLayoutElement.cellSize.x;
        _gridCellHeight = _gridLayoutElement.cellSize.y;

        if (_bSlotPREFAB == null)
            _bSlotPREFAB = Resources.Load<GameObject>("Prefab/UI/bSlot");

        _STACKABLE = GameManager.Instance._isStackable;
        _ADDCHAOTIC = GameManager.Instance._addChaotic;
        GenerateInventory(DetermineWorkStationBatchSize());
        if (_labelText)
            _labelText.text = _prefix;

    }

    /************************************************************************************************************************/
    #region Helper Initilization Methods for extended classes
    protected abstract List<int> DetermineWorkStationBatchSize();
    protected abstract void GenerateInventory(List<int> itemIDs);
    /**Determines the size of the content area based on how many items/rows we have. The overall size affects scrolling */
    protected virtual void SetSizeOfContentArea()
    {
      
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
        //    $"<color=yellow>_gridCellHeight= {_gridCellHeight}, _numberOfRows={_numberOfRows}</color>");

        //Debug.Log($"maxH= {_maxHeight} vs parentHeight={parentHeight}, Rect={new Vector2(parentWidth, parentHeight)}");
        // sets calculated width and height
        _gridParent.rectTransform.sizeDelta = new Vector2(parentWidth, parentHeight);


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


        if (_currColCount == _numberOfColumns)
        {
            _numberOfRows++;
            _currColCount = 0;
        }
        GameObject newButton = Instantiate(_bSlotPREFAB);
        newButton.SetActive(true);
        newButton.transform.SetParent(_gridLayoutElement.transform);
        newButton.transform.localScale = new Vector3(1, 1, 1);
        newButton.name = "bSlot_" + _prefix + " #" + slotSize;
        //Add slot component to our list
        var newSlot = newButton.GetComponent<UIInventorySlot>();
        //Set the slots manager:
        newSlot.SetManager(this);
        SetSizeOfContentArea();
        ++_currColCount;
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

    /**Used by all inventories initially to make required, and by IN-inventory with no specific slot in mind */
    public void AddItemToSlot(int itemID, List<QualityObject> qualities, bool makeRequired)
    {
        //if(_inventoryType==eInvType.OUT)
        // Debug.Log($"Adding Item to slot {itemID}");

        if (!IsInitalized)
            Start();

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
            //Debug.Log($"we fell thru ..creating new slot q valid={qualities == null}");
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

    protected void AddChaotic(int itemID, List<QualityObject> qualities, bool makeRequired)
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
                _available[UnityEngine.Random.Range(0, _available.Count - 1)].AssignItem(itemID, 1, qualities);
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

    public virtual void ItemAssigned(UIInventorySlot slot){}

    public int MaxSlots()
    {
        return _slots.Length;
    }

    public int SlotsInUse()
    {
        int count = 0;
        foreach (var item in _slots)
        {
            if (item._inUse)
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

    #endregion
}




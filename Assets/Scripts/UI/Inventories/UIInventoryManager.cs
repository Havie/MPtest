using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;


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
    [SerializeField] protected InventoryBackground _bg;
    [SerializeField] protected InventoryContentArea _content;
    [SerializeField] protected InventorySendButton _optionalSendButton;
    protected Button _sendButton;

    [Header("Specifications")]
    [SerializeField] protected int _maxItemsPerRow = 3;
    [SerializeField] protected int _maxColSize = 425;


    #region GameManager Parameters
    protected int _INVENTORYSIZE;
    protected bool _STACKABLE;
    protected bool _ADDCHAOTIC;
    #endregion
    protected GameObject _bSlotPREFAB;

    protected UIInventorySlot[] _slots;
    protected List<UIInventorySlot> _extraSlots; //incase we want to reset to base amount

    protected int _xMaxPerRow;
    protected int _cellPadding = 75;
    protected float _startingX = 37.5f;
    protected float _startingY = -37.5f;

    protected string _prefix;


    /************************************************************************************************************************/
    public bool IsInitalized { get; protected set; }


    protected virtual void Start()
    {
        if (_bSlotPREFAB == null)
            _bSlotPREFAB = Resources.Load<GameObject>("Prefab/UI/bSlot");

        _STACKABLE = GameManager.Instance._isStackable;
        _ADDCHAOTIC = GameManager.Instance._addChaotic;
        _INVENTORYSIZE = DetermineWorkStationBatchSize();
        GenerateInventory();

    }

    /************************************************************************************************************************/
    #region Helper Initilization Methods for extended classes
    protected abstract int DetermineWorkStationBatchSize();
    protected abstract void GenerateInventory();
    /**Determines the size of the content area based on how many items/rows we have. The overall size affects scrolling */
    protected virtual void SetSizeOfContentArea()
    {
        if (_xMaxPerRow == 0)
            return;

        Vector2 size;
        float extraCellpaddingX = DetermineXPadding();
        float extraCellpaddingY = DetermineYPadding();

        //Debug.Log($"x={extraCellpaddingX} , y={extraCellpaddingY}");

        if (GameManager.instance._batchSize == 1) ///turn off the pesky vert scroll bars
            size = new Vector2(_cellPadding, _cellPadding); ///will need to change if we add more than 1 item
        else
            size = new Vector2(((float)_xMaxPerRow * (float)_cellPadding) + (_cellPadding * extraCellpaddingX), (((((float)_INVENTORYSIZE / (float)_xMaxPerRow)) * _cellPadding) + (_cellPadding * extraCellpaddingY)));

        //((((_INVENTORYSIZE / _xMaxPerRow)) * _cellPadding) + (_cellPadding /2))

        // Debug.Log($" {(float)_INVENTORYSIZE } / {(float)_xMaxPerRow} = <color=green>{((float)_INVENTORYSIZE / (float)_xMaxPerRow)}</color>  then w cellapdding = {((((float)_INVENTORYSIZE / (float)_xMaxPerRow)) * _cellPadding)} ");

        UpdateComponentRects(size);
        DetermineBestScale();

        ///LOCAL FUNCTIONS
        void UpdateComponentRects(Vector2 v2Size)
        {
            if (_content)
            {
                v2Size.y += _content.GetReducedYSize;
                _content.ChangeRectTransform(v2Size);
            }

            ///Recalibrate
            if (v2Size.y > _maxColSize)
                v2Size.y = _maxColSize;

            if (_bg) ///Make sure this called before Mask
                _bg.ChangeRectTransform(v2Size);
            if (_optionalSendButton)
                _optionalSendButton.ChangeRectTransform(v2Size);
        }
        float DetermineXPadding()
        {
            ///Adding padding here results in the items all being anchored wrongly to the left, 
            ///cant see to figure out how to center them in NextSlotLocation()
            if (_xMaxPerRow < _maxItemsPerRow)
                return 0;
            else
                return 0;
        }
        float DetermineYPadding()
        {
            ///Need to return the difference of whatver padding required to make _maxColSize (425):
            //var retVal = _maxColSize/ (((((float)_INVENTORYSIZE / (float)_xMaxPerRow)) * _cellPadding) + _cellPadding );
            /// the difference required to made y height the size of 2 slots:
            var retVal = (_cellPadding * 2) / (((((float)_INVENTORYSIZE / (float)_xMaxPerRow)) * _cellPadding) + _cellPadding);

            if (((_INVENTORYSIZE / _xMaxPerRow) * _cellPadding) <= _cellPadding * 2)
                return retVal;
            else
                return 0.5f;
        }
        void DetermineBestScale()
        {
            ///TODO scale the UI UP a bit based on how small the inventory size is 

            ///float scale = ((float)12 / (float)_INVENTORYSIZE);


            ///i NEED AN expoential curve,that the closer u get to ONE the harder cap on being 1.5 
            ///

            float max = 1.25f;
            float min = 0.75f;
            float scale = min + (2.45f / _INVENTORYSIZE);  ///Smaller the top # the smaller the inventory size

            if (scale > max)
            {
                if (_INVENTORYSIZE == 1)
                    scale = 1.50f;
                else
                    scale = max;
            }

            transform.localScale = new Vector3(scale, scale, scale);
            //Debug.Log($"[{_inventoryType}] _INVENTORYSIZE={_INVENTORYSIZE} --> {scale}");

        }
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
        newButton.transform.SetParent(_content.transform, false);
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
        button.transform.SetAsLastSibling();
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




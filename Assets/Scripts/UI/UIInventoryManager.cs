using System.Collections;
using System.Collections.Generic;
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
        _INVENTORYSIZE = DetermineWorkStationBatchSize();
        _STACKABLE = GameManager.instance._isStackable;
        _ADDCHAOTIC = GameManager.instance._addChaotic;
        if (_inventoryType == eInvType.IN)
            GameManager.instance.SetInventoryIn(this);
        else if (_inventoryType == eInvType.OUT)
            GameManager.instance.SetInventoryOut(this);
        else
            GameManager.instance.SetInventoryStation(this);
    }

    private int DetermineWorkStationBatchSize()
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


    private void GenInventory()
    {
       if(_inventoryType==eInvType.STATION && !_STACKABLE)
        {
            Destroy(this.gameObject); //good enough for now might need to go higher
            return;
        }
        
        _slots = new UIInventorySlot[_INVENTORYSIZE];

        //Determine layout
        if (_INVENTORYSIZE > 4)
            _xMaxRows = (_INVENTORYSIZE / 4) + 1;
        else
            _xMaxRows = _INVENTORYSIZE;

        if (_xMaxRows > 10)
            _xMaxRows = 10;

        //Size matters for the vert/hori scrollbars
        SetSizeOfContentArea();

        //cache a conditions for forloop situations
        bool cond = GameManager.instance._autoSend; //used By eInvType.OUT
        Dictionary<Task, int> _seenTasks = new Dictionary<Task, int>(); //used By eInvType.STATION

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
        for (int i = 0; i < _INVENTORYSIZE; ++i)
        {
            GameObject newButton = Instantiate(_bSlotPREFAB) as GameObject;
            newButton.SetActive(true);
            newButton.transform.SetParent(this.transform, false);
            newButton.transform.localPosition = new Vector3(xLoc, yLoc, 0);
            //Update COL/ROWs
            xLoc += _cellPadding;
            if (i + 1 % _xMaxRows == 0)
            {
                yLoc -= _cellPadding;
                xLoc = _startingX;
            }
            //Rename in scene hierarchy
            newButton.name = "bSlot_" + prefix + " #" + i;
            //Add slot component to our list
            _slots[i] = newButton.GetComponent<UIInventorySlot>();
            //Set the slots manager:
            _slots[i].SetManager(this);
            if (_inventoryType == eInvType.OUT) //Set our out slots to auto send or not
                _slots[i].SetAutomatic(cond);
            else if (_inventoryType == eInvType.STATION) //Set our infinite station items
                SetInfiniteItem(_seenTasks, _slots, i);
        }
    }

    private void SetInfiniteItem(Dictionary<Task, int> _seenTasks, UIInventorySlot[] _slots, int i)
    {
        WorkStation ws = GameManager.instance._workStation;
        var tasks = ws._tasks;
        foreach (Task t in tasks)
        {
            if (_seenTasks.ContainsKey(t))
            {
                int amountSeen = _seenTasks[t];
                if (amountSeen <= t._requiredItemIDs.Count)
                {

                    _seenTasks.Remove(t);
                    _seenTasks.Add(t, amountSeen + 1);
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
        }

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
            _available[Random.Range(0, _available.Count - 1)].AssignItem( itemID, 1);
        }
    }

    public void SetImportant(GameObject button)
    {
        button.transform.SetAsLastSibling();
    }
}

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
    [SerializeField] GridLayoutGroup _layoutGroup;
    [SerializeField] Sprite[] _iconSprites;

    [SerializeField] bool _in;
    private UIInventorySlot[] _slots;

    #region setup
    private void Start()
    {
        if (_bSlotPREFAB == null)
            _bSlotPREFAB = Resources.Load<GameObject>("Prefab/UI/bSlot");
        GetGameManagerData();
        GenInventory();

    }

    private void GetGameManagerData()
    {
        _INVENTORYSIZE = GameManager.instance._inventorySize;
        _STACKABLE = GameManager.instance._isStackable;
        _ADDCHAOTIC = GameManager.instance._addChaotic;
        if (_in)
            GameManager.instance.SetInventoryIn(this);
        else
        {
            GameManager.instance.SetInventoryOut(this);
        }
    }


    private void GenInventory()
    {
        _slots = new UIInventorySlot[_INVENTORYSIZE];
        //Make sure we cant scroll horiz unless we need it 
        if (_INVENTORYSIZE <= 10)
            _layoutGroup.constraintCount = _INVENTORYSIZE;
        else
            _layoutGroup.constraintCount = 10;
        // _layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

        bool cond = GameManager.instance._autoSend;
        string prefix;
        if (_in)
            prefix = "in_";
        else
            prefix = "out_";
        for (int i=0; i<_INVENTORYSIZE; ++i)
        {
            GameObject newButton = Instantiate(_bSlotPREFAB) as GameObject;
            newButton.SetActive(true);
            newButton.transform.SetParent(_layoutGroup.transform, false);
            newButton.name = "bSlot_"+ prefix+" #"+i;
            _slots[i] = newButton.GetComponent<UIInventorySlot>();
            if (!_in) //Set our out slots to auto send or not
                _slots[i].SetAutomatic(cond);
        }
    }



    #endregion


    public bool HasFreeSlot()
    {
        foreach(var slot in _slots)
        {
            if (slot.GetInUse() == false)
                return true;
        }
        return false;
    }
   

    public void AddItemToSlot(int itemLvl)
    {
        if (!_ADDCHAOTIC)
        {
            foreach (UIInventorySlot slot in _slots)
            {
                if (!slot.GetInUse())
                {
                    Sprite sp = BuildableObject.Instance.GetSpriteByLevel(itemLvl);
                    slot.AssignItem(sp, itemLvl);
                    return;
                }
            }
        }
        else
        {
            List<UIInventorySlot> _available = new List<UIInventorySlot>();
            foreach(UIInventorySlot slot in _slots)
            {
                if (!slot.GetInUse())
                    _available.Add(slot);
            }
            Sprite sp = BuildableObject.Instance.GetSpriteByLevel(itemLvl);
            _available[Random.Range(0, _available.Count - 1)].AssignItem(sp, itemLvl);
        }
    }
}

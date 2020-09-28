using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


//https://www.youtube.com/watch?v=Oba1k4wRy-0 //Tutorial
public class UIInventoryManager : MonoBehaviour
{

    public class PlayerItem
    {
        public Sprite _iconSprite;
    }

    private List<PlayerItem> _playerInventory;
    [SerializeField] GameObject _bSlotPREFAB;
    [SerializeField] GridLayoutGroup _layoutGroup;
    [SerializeField] Sprite[] _iconSprites;

    [SerializeField] bool _in;
    private UIInventorySlot[] _slots;

    private void Start()
    {

        _slots = this.transform.GetComponentsInChildren<UIInventorySlot>();

        if (_in)
            GameManager.instance.SetInventoryIn(this);
        else
        {
            GameManager.instance.SetInventoryOut(this);
            SetOutChildren();
        }

        TutorialStart();
    }

    #region setup

    private void TutorialStart()
    {
        _playerInventory = new List<PlayerItem>();
        int size = _iconSprites.Length - 1;
        for (int i=1; i<104; i++)
        {
            PlayerItem newitem = new PlayerItem();

            newitem._iconSprite = _iconSprites[Random.Range(0, size)];

            _playerInventory.Add(newitem);
        }

        GenInventory();
    }

    private void GenInventory()
    {
        Debug.Log("player inventory.count = " + _playerInventory.Count);
        //Make sure we cant scroll horiz unless we need it 
        if (_playerInventory.Count <= 10)
            _layoutGroup.constraintCount = _playerInventory.Count;
        else
            _layoutGroup.constraintCount = 10;
           // _layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

        foreach (var item in _playerInventory)
        {
            GameObject newButton = Instantiate(_bSlotPREFAB) as GameObject;
            newButton.SetActive(true);
            newButton.GetComponent<UIInventorySlot>().AssignItem(item._iconSprite, 1);
            newButton.transform.SetParent(_layoutGroup.transform, false);
        }
    }

    #endregion

    private void SetOutChildren()
    {
        bool cond = GameManager.instance._autoSend;
        foreach (var slot in _slots)
        {
            slot.SetAutomatic(cond);
        }
    }

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
        foreach(UIInventorySlot slots in _slots)
        {
            if(!slots.GetInUse())
            {
                Sprite sp =BuildableObject.Instance.GetSpriteByLevel(itemLvl);
                slots.AssignItem(sp, itemLvl);
            }
        }
    }
}

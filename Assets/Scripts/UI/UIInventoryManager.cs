using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventoryManager : MonoBehaviour
{
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


    }

    private void SetOutChildren()
    {
        bool cond = GameManager.instance._autoSend;
        foreach (var slot in _slots)
        {
            slot.SetAutomatic(cond);
        }
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

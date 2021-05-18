using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderButton : MonoBehaviour
{
    TextMeshProUGUI _text;
    [SerializeField] UIInventorySlot _slot = default;
    [SerializeField] ColorManager _cm = default;

    private int _itemID;

    private float _delieveryTime;
    private float _tLow = 0.35f;
    private float _tBad = 0.15f;



    private bool _orderReceived;

    private void Awake()
    {
        if(!_text)
            _text = this.GetComponentInChildren<TextMeshProUGUI>();
        //if(!_img)
        //    _img = this.GetComponentInChildren<Image>();
    }

    public int ItemID => _itemID;
    public UIInventorySlot Slot => _slot;

    public void SetManager(IInventoryManager manager)
    {
        _slot.SetManager(manager);
    }

    public void SetOrder(int itemID, float timePromised)
    {
        var bo = ObjectManager.Instance;
        //string orderName = bo.GetItemNameByID(itemID);
        Sprite img= bo.GetSpriteByID(itemID);

        //if (_text)
        //    _text.text = orderName;

        _delieveryTime = timePromised;


        //if (_img)
        //    _img.sprite = img;

        _slot.SetAsOutSlot(); ///Will force it to tell manager instead of send as OUT
        _slot.SetRequiredID(itemID);

        _itemID = itemID;

        _orderReceived = true;
    }


    void LateUpdate()
    {
        if(_orderReceived)
        {
            if (_text)
            {
                float newTime = _delieveryTime - Time.time;
                if (newTime < 0)
                    _text.text = "LATE";
                else 
                    _text.text = FormatTime(newTime);

                CheckTimeColor(newTime / _delieveryTime);
            }
        }
    }

    private string FormatTime(float time)
    {
        string min = ((int)time / 60).ToString();
        string sec = (time % 60).ToString("0");

        if (time < 60)
            min = "0";
        if (sec.Length == 1)
            sec = "0" + sec;

        return ($"{min}:{sec}");
    }

    private void CheckTimeColor(float time)
    {

        if ( !_cm)
        {
            return;
        }
        else if (time < _tBad)
        {
            //Debug.Log($"CheckTimeColor=<color=red>{time}</color>");
            _text.color = _cm.Bad;
        }
        else if (time < _tLow)
        {
           // Debug.Log($"CheckTimeColor=<color=yellow>{time}</color>");
            _text.color = _cm.Low;
        }
    }

}

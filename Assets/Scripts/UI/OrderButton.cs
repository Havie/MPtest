using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class OrderButton : MonoBehaviour
{
    [SerializeField] Text _text;
    [SerializeField] Image _img;
    private int _itemID;

    private void Awake()
    {
        if(!_text)
            _text = this.GetComponentInChildren<Text>();
        if(!_img)
            _img = this.GetComponentInChildren<Image>();
    }

    public int ItemID => _itemID;

    public void SetOrder(int itemID)
    {
        var bo = BuildableObject.Instance;
        string orderName = bo.GetItemNameByID(itemID);
        Sprite img= bo.GetSpriteByID(itemID);

        if (_text)
            _text.text = orderName;
        if (_img)
            _img.sprite = img;
        _itemID = itemID;
    }

    

    
}

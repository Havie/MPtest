using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour
{
    Image _img;
    Sprite _default;
    bool _autoSend = false; //Only for OutINV, set by InventoryManager
    int _itemLVL= -1;
    public bool _inUse;

    private void Awake()
    {
        _img = this.GetComponent<Image>();
        if (_img)
            _default = _img.sprite;
    }

    public void SetAutomatic(bool cond)
    {
        _autoSend = cond;
    }
    public bool GetInUse() => _inUse;
    public void PreviewSlot(Sprite img)
    {
        if(!_inUse)
        {
            _img.sprite = img;

        }
        else
        {
            //display something?
        }
    }

    public int GetItemID() => _itemLVL;

    public void RestoreDefault()
    {
        _img.sprite = _default;
        _itemLVL = -1;
        _inUse = false;
    }

    public bool AssignItem(Sprite img, int level)
    {
        if (!_inUse)
        {
            if (_img)
                _img.sprite = img;

            _itemLVL = level;

            if (_autoSend)
                SendData(); // out only
            else
                _inUse = true;
        }

        return false;
    }
    public void SetLarger()
    {
        this.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
    }
    public void SetNormal()
    {
        this.transform.localScale = new Vector3(1, 1, 1);
    }
    public void SendData()
    {
        //Debug.Log("   ....... CALLLED SEND DATA ........        ");
        WorkStation myStation = GameManager.instance._workStation;

        if (WorkStation._stationFlow.ContainsKey((int)myStation._myStation))
        {
            int StationToSend = WorkStation._stationFlow[(int)myStation._myStation];
            // Debug.Log($"Sending ItemLevelID {_itemLVL} to Station: {StationToSend}");
            ClientSend.SendItem(_itemLVL, StationToSend);
            RestoreDefault();
        }
        else
            Debug.LogError($"Error Sending Out INV slot , no StationKey {(int)myStation._myStation}");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour
{
    [SerializeField] Image _myIcon;
    private Sprite _defaultIcon;
    private UIInventoryManager _manager;
    public bool _autoSend = false; //Only for OutINV, set by InventoryManager
    int _itemID= -1;
    public bool _inUse;
    int _numItemsStored = 0;

    private Vector3 _LARGER = new Vector3(1.25f, 1.25f, 1.25f);
    private Vector3 _NORMAL = new Vector3(1, 1, 1);
    private Vector3 _SMALLER = new Vector3(0.5f, 0.5f, 0.5f);


    private void Awake()
    {
        _defaultIcon = _myIcon.sprite;

    }
    public void SetManager(UIInventoryManager manager) { _manager = manager; }

    public void SetAutomatic(bool cond)
    {
        _autoSend = cond;
    }
    public bool GetInUse() => _inUse;
    public void PreviewSlot(Sprite img)
    {


        if (!_inUse)
            _myIcon.sprite = img;
        else
        {
            //display something that shows slot is in use
        }
        _manager.SetImportant(this.gameObject);
        SetLarger();
    }

    public int GetItemID() => _itemID;

    public void RestoreDefault()
    {
        _myIcon.sprite = _defaultIcon;
        _itemID = -1;
        _numItemsStored = 0;
        _inUse = false;
        SetNormal();
        //Debug.Log($"Reset {this.gameObject.name} to default img {_myIcon.sprite} , {_defaultIcon}");
    }

    public void UndoPreview()
    {
       // Debug.Log($"UndoPreview {this.gameObject.name} #items {_numItemsStored} , {_myIcon.sprite}");

        if (_numItemsStored > 0)
        {
            SetNormal();
        }
        else
            RestoreDefault();
    }

    public void RemoveItem()
    {
        --_numItemsStored;
        if (_numItemsStored <= 0)
            RestoreDefault();
    }


    /**Assigns an img to the child sprite of this object, and keeps track of its id */
    public bool AssignItem(int id, int count)
    {
         Debug.Log(this.gameObject.name + " Assign ITEM "  + "id=" +id  + " autosend="+_autoSend);
        if (!_inUse)
        {
            var bo = BuildableObject.Instance;
            Sprite img = bo.GetSpriteByID(id);

            if (_myIcon)
                _myIcon.sprite = img;

            _itemID = id;
            _numItemsStored = count;
            _inUse = true;
            if (_autoSend)
            {
                if (count > 1)
                    Debug.LogWarning("Trying to autosend more than 1 item? shouldnt happen");
                SendData(); // out only
            }
             }

        return false;
    }
    public void SetLarger()
    {
        this.transform.localScale = _LARGER;
    }
    public void SetNormal()
    {
        this.transform.localScale = _NORMAL;
    }

    public void SetSmaller()
    {
        this.transform.localScale = _SMALLER;
    }
    public void SendData()
    {
        Debug.Log("   ....... CALLLED SEND DATA ........        ");
        WorkStation myStation = GameManager.instance._workStation;

        if (_inUse )//&& WorkStation._stationFlow.ContainsKey((int)myStation._myStation))
        {
            //int StationToSend = WorkStation._stationFlow[(int)myStation._myStation];
             Debug.Log($"Sending ItemLevelID {_itemID} to Station: {(int)myStation._sendOutputToStation}");
            ClientSend.SendItem(_itemID, (int)myStation._sendOutputToStation);
            CheckKitting();
            RemoveItem(); // should always call RestoreDefault;
        }
        else
            Debug.LogError($"Error Sending Out INV slot , no StationKey {(int)myStation._myStation}");
    }

    /** Find and remove an order from kittings in orders */
    private void CheckKitting()
    {
        var ws = GameManager.instance._workStation;
        if(ws.isKittingStation())
        {
            GameManager.instance._invKITTING.RemoveOrder(_itemID);
        }
    }
}

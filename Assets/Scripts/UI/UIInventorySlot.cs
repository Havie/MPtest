﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInventorySlot : MonoBehaviour
{
    [SerializeField] Image _myIcon;
    private Sprite _defaultIcon;
    private UIInventoryManager _manager;
    private bool _autoSend = false; //Only for OutINV, set by InventoryManager
    private bool _isOutSlot;
    private int _requiredID=-1;
    int _itemID= -1;
    public bool _inUse;
    int _numItemsStored = 0;

    private Vector3 _LARGER = new Vector3(1.25f, 1.25f, 1.25f);
    private Vector3 _NORMAL = new Vector3(1, 1, 1);
    private Vector3 _SMALLER = new Vector3(0.5f, 0.5f, 0.5f);

    private Color _VISIBLE = new Color(255, 255, 255, 1);
    private Color _TRANSPARENT = new Color(255, 255, 255, 0.5f);
    private Color _INVALID = new Color(255, 155, 155, 0.5f);

    private void Awake()
    {
        _defaultIcon = _myIcon.sprite;

    }
    public void SetManager(UIInventoryManager manager) { _manager = manager; }

    public void SetAutomatic(bool cond)
    {
        _autoSend = cond;
        _isOutSlot = true; // only OUT-INV calls this method so safe to assume
    }
    public int RequiredID => _requiredID;
    public bool RequiresCertainID()
    {
        return _requiredID != -1;
    }
    public void SetRequiredID(int itemID)
    {
        _requiredID = itemID;
        //Set transparent icon 
        AssignSprite(_requiredID,true);
    }
    public bool GetInUse() => _inUse;
    public void PreviewSlot(Sprite img)
    {
        if (!_inUse && _requiredID == -1)
        {
            _myIcon.sprite = img;
        }
        else if (!_inUse && _requiredID != -1)
        {
            //Debug.Log("enter2 result=" + (BuildableObject.Instance.GetSpriteByID(_requiredID) != img));
            if (BuildableObject.Instance.GetSpriteByID(_requiredID) != img)
                _myIcon.color = _INVALID;
            else
                _myIcon.color = _VISIBLE;
        }
        else
        {
            //display something that shows slot is in use
            _myIcon.color = _INVALID;
        }
        _manager.SetImportant(this.gameObject);
        SetLarger();
    }

    public int GetItemID() => _itemID;

    public void RestoreDefault()
    {
        SetNormal();
        _inUse = false;
        _numItemsStored = 0;
        _itemID = -1;
        if (_requiredID != -1)
        {
            AssignSprite(_requiredID, true);
        }
        else
        {
            _myIcon.sprite = _defaultIcon;
        }
    }

    public void UndoPreview()
    {
       // Debug.Log($"UndoPreview {this.gameObject.name} #items {_numItemsStored} , {_myIcon.sprite}");

        if (_numItemsStored > 0)
        {
            SetNormal();
            if (_requiredID != -1)
                AssignSprite(_requiredID, false);
            else
                AssignSprite(_itemID, false);
        }
        else
        {
           RestoreDefault();
        }
           
    }

    public void RemoveItem()
    {
        --_numItemsStored;
        if (_numItemsStored <= 0)
        {
            if (_requiredID != -1)
            {
                AssignSprite(_requiredID, true);
                _itemID = -1;
                _inUse = false;
                SetNormal();
            }
            else
                RestoreDefault();
        }
    }

    private void AssignSprite(int id, bool transparent)
    {

        //Debug.Log($"{this.gameObject.name} AssignSprite {id} , {transparent}");
        var bo = BuildableObject.Instance;
        Sprite img = bo.GetSpriteByID(id);

        if (_myIcon)
            _myIcon.sprite = img;

       
        if(transparent)
            _myIcon.color = _TRANSPARENT;
        else
            _myIcon.color = _VISIBLE;
    }
    /**Assigns an img to the child sprite of this object, and keeps track of its id */
    public bool AssignItem(int id, int count)
    {
        //if (this.gameObject.name.Contains("tation"))
         //   Debug.Log(this.gameObject.name + " Assign ITEM "  + "id=" +id  + "  , autosend="+_autoSend + " , _inUse="+ _inUse + " , _isOutSlot=" + _isOutSlot + " , _requiredID=" + _requiredID) ;
        if (!_inUse)
        {
            if (_isOutSlot && id != _requiredID)
               return false;

            AssignSprite(id, false);

             _itemID = id;
            _numItemsStored = count;
            _inUse = true;
            if (_autoSend)
            {
                if (count > 1)
                    Debug.LogWarning("Trying to autosend more than 1 item? shouldnt happen");
                SendData(); // out only
            }
            else if(_isOutSlot) //non pull send
            {
                TellManager();
            }

            return true;
        }

        return false;
    }
    private void TellManager()
    {
        _manager.CheckIfBatchIsReady();
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

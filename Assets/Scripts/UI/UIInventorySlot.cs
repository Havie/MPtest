using System.Collections;
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
    int _itemID = -1;
    public bool _inUse;
    int _numItemsStored = 0;
    List<ObjectQuality> _qualities = new List<ObjectQuality>();

    public List<ObjectQuality> Qualities => _qualities;
    public int RequiredID { get; private set; } = -1;

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

    public bool RequiresCertainID()
    {
        return RequiredID != -1;
    }
    public void SetRequiredID(int itemID)
    {
        RequiredID = itemID;
        //Set transparent icon 
        AssignSpriteByID(RequiredID, true);
    }
    public bool GetInUse() => _inUse;
    public bool PreviewSlot(Sprite img)
    {
        bool retVal = true;
        if (!_inUse && RequiredID == -1)
        {
            AssignSprite(img);
        }
        else if (!_inUse && RequiredID != -1)
        {
            //Debug.Log("enter2 result=" + (BuildableObject.Instance.GetSpriteByID(_requiredID) != img));
            if (BuildableObject.Instance.GetSpriteByID(RequiredID) != img)
            {
                _myIcon.color = _INVALID;
                retVal = false;
            }
            else
                _myIcon.color = _VISIBLE;
        }
        else
        {
            //display something that shows slot is in use
            _myIcon.color = _INVALID;
            retVal = false;
        }
        _manager.SetImportant(this.gameObject);
        SetLarger();
        return retVal;
    }

    public int GetItemID() => _itemID;

    public void RestoreDefault()
    {
        SetNormal();
        _inUse = false;
        _numItemsStored = 0;
        _itemID = -1;
        if (RequiredID != -1)
        {
            AssignSpriteByID(RequiredID, true);
        }
        else
        {
            AssignSprite(_defaultIcon);
        }
    }

    public void UndoPreview()
    {

        if (_numItemsStored > 0)
        {
            SetNormal();
            if (RequiredID != -1)
                AssignSpriteByID(RequiredID, false);
            else
                AssignSpriteByID(_itemID, false);
        }
        else
        {
            RestoreDefault();
        }

    }

    public void RemoveItem()
    {
        --_numItemsStored;
        //if (gameObject.name.Contains("#1"))
        //    Debug.Log($"Removed Item for {this.gameObject.name} , new total={_numItemsStored}");
        if (_numItemsStored <= 0)
        {
            _qualities.Clear();
            if (RequiredID != -1)
            {
                AssignSpriteByID(RequiredID, true);
                _itemID = -1;
                _inUse = false;
                SetNormal();
            }
            else
                RestoreDefault();
        }
    }

    private void AssignSpriteByID(int id, bool transparent)
    {

        var bo = BuildableObject.Instance;
        Sprite img = bo.GetSpriteByID(id);

        //if (gameObject.name.Contains("#1"))
        //    Debug.Log($"{this.gameObject.name} AssignSprite {id} ={img.name}, {transparent} ");

        AssignSprite(img);


        if (transparent)
            _myIcon.color = _TRANSPARENT;
        else
            _myIcon.color = _VISIBLE;
    }

    private void AssignSprite(Sprite img)
    {
        if (_myIcon)
            _myIcon.sprite = img;

        // Debug.Log($"{this.gameObject.name} AssigndSprite = <color=green>{img.name}</color>");

    }
    /**DEPRECEATED */
    private bool AssignItem(int id, int count) ///Need to make this priv and re-direct old calls
    {
        //if (this.gameObject.name.Contains("tation"))
        //   Debug.Log(this.gameObject.name + " Assign ITEM "  + "id=" +id  + "  , autosend="+_autoSend + " , _inUse="+ _inUse + " , _isOutSlot=" + _isOutSlot + " , _requiredID=" + _requiredID) ;
        if (!_inUse)
        {
            if (_isOutSlot && id != RequiredID)
                return false;

            AssignSpriteByID(id, false);

            _itemID = id;
            _numItemsStored = count;
            _inUse = true;
            if (_autoSend)
            {
                if (count > 1)
                    Debug.LogWarning("Trying to autosend more than 1 item? shouldnt happen");
                SendData(); // out only
            }
            else if (_isOutSlot) //non pull send
            {
                TellManager();
            }

            return true;
        }

        return false;
    }

    public bool AssignItem(ObjectController oc, int count)
    {
        /// get ID from controller
        int id = (int)oc._myID;

        ///Get Object Quality from controller
        OverallQuality overallQuality = oc.GetComponent<OverallQuality>();
        if (overallQuality != null)
        {
            List<ObjectQuality> qualities = overallQuality.Qualities;

            return AssignItem(id, count, qualities);
        }


        return false;
    }

    public bool AssignItem(int id, int count, List<ObjectQuality> qualities)
    {
        if (!_inUse)
        {
            if (_isOutSlot && id != RequiredID)
            {
                Debug.Log($"{id} does not match {RequiredID}");
                return false;
            }
            AssignSpriteByID(id, false);

            ///Might have to clone it, but lets see if we can store it
            if (qualities != null)
                _qualities = qualities;
            else
                _qualities.Clear();

            if (qualities != null)
                DebugQualityIn();
            //else
            //    UIManager.instance.DebugLog("Qualities Read was Null");

            _itemID = id;
            _numItemsStored = count;
            _inUse = true;
            if (_autoSend)
            {
                if (count > 1)
                    Debug.LogWarning("Trying to autosend more than 1 item? shouldnt happen");
                SendData(); // out only
            }
            else if (_isOutSlot) //non pull send
            {
                TellManager();
            }


            return true;
        }
        return false;
    }

    private void DebugQualityIn()
    {
        if (_qualities.Count == 0)
            Debug.Log("..DebuggQualityIn count=0");
        else
        {
            foreach (var q in _qualities)
            {
                UIManager.instance.DebugLog($"{this.gameObject.name} has quality id {q.ID} ,<color=green> {q.CurrentQuality} </color>");
            }
        }
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
        //Debug.Log("   ....... CALLLED SEND DATA ........        ");
        WorkStation myStation = GameManager.instance._workStation;

        if (_inUse)//&& WorkStation._stationFlow.ContainsKey((int)myStation._myStation))
        {
            //int StationToSend = WorkStation._stationFlow[(int)myStation._myStation];
            UIManager.instance.DebugLog($"(UIInventorySlot) sending: <color=green>{_itemID}</color> to Station: <color=blue>{(int)myStation._sendOutputToStation}</color>");
            ClientSend.SendItem(_itemID, _qualities, (int)myStation._sendOutputToStation);
            CheckKitting();
            RemoveItem(); // should always call RestoreDefault;
        }
        //else
        // Debug.LogError($"Error Sending Out INV slot , no StationKey {(int)myStation._myStation}");
    }

    /** Find and remove an order from kittings in orders */
    private void CheckKitting()
    {
        var ws = GameManager.instance._workStation;
        if (ws.isKittingStation())
        {
            Debug.LogWarning($"kitting station removal has been disabled ID={_itemID}");
            //GameManager.instance._invKITTING.RemoveOrder(_itemID);
        }
    }
}

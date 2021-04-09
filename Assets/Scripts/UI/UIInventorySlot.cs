using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIInventorySlot : MonoBehaviour, IAssignable
{
    [SerializeField] Image _myIcon = default;
    [SerializeField] GameObject _greenCheckmark = default;
    [SerializeField] Sprite _defaultIconUsed = default;
    [SerializeField] Sprite _defaultIconEmpty = default;
    private Sprite _currentBGSprite;
    private UIInventoryManager _manager;
    private bool _autoSend = false; //Only for OutINV, set by InventoryManager
    private bool _isOutSlot;
    int _itemID = -1;
    public bool _inUse;
    int _numItemsStored = 0;
    List<QualityObject> _qualities = new List<QualityObject>();

    public List<QualityObject> Qualities => _qualities;
    public int RequiredID { get; private set; } = -1;

    private Vector3 _LARGER = new Vector3(1.15f, 1.15f, 1.15f);
    private Vector3 _NORMAL = new Vector3(1, 1, 1);
    private Vector3 _SMALLER = new Vector3(0.5f, 0.5f, 0.5f);

    private Color _VISIBLE = new Color(255, 255, 255, 1);
    private Color _TRANSPARENT = new Color(255, 255, 255, 0.5f);
    private Color _INVALID = new Color(255, 155, 155, 0.5f);

    /************************************************************************************************************************/
    private void Awake()
    {
        ///DefaultToEmptyIcon
        SwapBackgroundIMGs(false);
    }
    /************************************************************************************************************************/

    public void SetManager(UIInventoryManager manager) { _manager = manager; }
    public void SetAutomatic(bool cond)
    {
        _autoSend = cond;
        _isOutSlot = true; // only OUT-INV calls this method so safe to assume
    }
    public void SetRequiredID(int itemID)
    {
        RequiredID = itemID;
        //Set transparent icon 
        AssignSpriteByID(RequiredID, true);
    }
    public void SwapBackgroundIMGs(bool isInUse)
    {
        if (isInUse)
        {
            _currentBGSprite = _defaultIconUsed;
        }
        else
        {
            _currentBGSprite = _defaultIconEmpty;

        }
        this.GetComponent<Image>().sprite = _currentBGSprite;
    }
    public int GetItemID() => _itemID;


    #region Interface
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
            if (ObjectManager.Instance.GetSpriteByID(RequiredID) != img)
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
        if (_manager)
            _manager.SetImportant(this.gameObject);
        SetLarger();
        return retVal;
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
            SwapBackgroundIMGs(false);
            _qualities.Clear();
            if (RequiredID != -1)
            {
                AssignSpriteByID(RequiredID, true);
                _itemID = -1;
                _inUse = false;
                SetNormal();

                if (_greenCheckmark != null)
                    _greenCheckmark.SetActive(false);

            }
            else
                RestoreDefault();
        }
    }
    public bool AssignItem(ObjectController oc, int count)
    {
        if (oc == null)
            return false;

        /// get ID from controller
        int id = (int)oc._myID;

        ///Get Object Quality from controller
        QualityOverall overallQuality = oc.GetComponent<QualityOverall>();
        if (overallQuality != null)
        {
            List<QualityObject> qualities = overallQuality.Qualities;

            return AssignItem(id, count, qualities);
        }


        return false;
    }
    public bool AssignItem(int id, int count, List<QualityObject> qualities)
    {
        if (!_inUse)
        {
            if (_isOutSlot && id != RequiredID)
            {
                //Debug.Log($"{id} does not match {RequiredID}");
                return false;
            }
            else if (_isOutSlot && id == RequiredID)
            {
                if (_greenCheckmark != null)
                    _greenCheckmark.SetActive(true);
            }
            AssignSpriteByID(id, false);
            SwapBackgroundIMGs(true);
            ///Might have to clone it, but lets see if we can store it
            if (qualities != null)
                _qualities = qualities;
            else
                _qualities.Clear();

            //if (qualities != null)
            //    DebugQualityIn();
            //else
            //    UIManager.DebugLog("Qualities Read was Null");

            _itemID = id;
            _numItemsStored = count;
            _inUse = true;
            if (_autoSend)
            {
                if (count > 1)
                    Debug.LogWarning("Trying to autosend more than 1 item? shouldnt happen");
                SendData(); // out only
            }
            else //non pull send
            {
                TellManager();
            }

            ///Try encapsulating this here:
            SetNormal();
            return true;
        }
        return false;
    }
    public bool RequiresCertainID() => RequiredID != -1;
    #endregion
    public bool SendData()
    {
        //Debug.Log($"   ....... CALLLED SEND DATA ........  _inUse= {_inUse}     ");
        WorkStation myStation = GameManager.instance._workStation;

        if (_inUse)//&& WorkStation._stationFlow.ContainsKey((int)myStation._myStation))
        {
            //int StationToSend = WorkStation._stationFlow[(int)myStation._myStation];
            UIManager.DebugLog($"(UIInventorySlot) sending: <color=green>{_itemID}</color> to Station: <color=blue>{(int)myStation._sendOutputToStation}</color>");
            ClientSend.Instance.SendItem(_itemID, _qualities, (int)myStation._sendOutputToStation);
            CheckKitting();
            RemoveItem(); // should always call RestoreDefault;
            return true;
        }
        //else
        // Debug.LogError($"Error Sending Out INV slot , no StationKey {(int)myStation._myStation}");
        return false;
    }

    public List<QualityObject> RebuildQualities()
    {
        List<QualityObject> newList = new List<QualityObject>();
        if (Qualities != null)
        {
            foreach (var q in Qualities)
                newList.Add(q);
        }

        return newList;
    }


    /************************************************************************************************************************/

    private void AssignSpriteByID(int id, bool transparent)
    {

        var bo = ObjectManager.Instance;
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


    private void DebugQualityIn()
    {
        if (_qualities.Count == 0)
            Debug.Log("..DebuggQualityIn count=0");
        else
        {
            foreach (var q in _qualities)
            {
                UIManager.DebugLog($"{this.gameObject.name} has quality id {q.ID} ,<color=green> {q.CurrentQuality} </color>");
            }
        }
    }

    private void TellManager()
    {
        _manager.ItemAssigned(this);
    }
    private void RestoreDefault()
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
            ///Just show the default ICON ontop instead of disabling
            AssignSprite(_currentBGSprite);
        }
    }
    private void SetLarger()
    {
        this.transform.localScale = _LARGER;
    }
    private void SetNormal() ///Shud get this private and abstracted
    {
        this.transform.localScale = _NORMAL;
    }

    public void SetSmaller()
    {
        this.transform.localScale = _SMALLER;
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

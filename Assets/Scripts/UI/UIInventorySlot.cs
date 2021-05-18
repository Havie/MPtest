using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UIInventorySlot : MonoBehaviour, IAssignable
{
    [SerializeField] Image _myIcon = default;
    [SerializeField] UICheckMark _greenCheckmark = default;
    [SerializeField] Sprite _defaultIconUsed = default;
    [SerializeField] Sprite _defaultIconEmpty = default;
    public int RequiredID { get; private set; } = -1;
    public List<QualityObject> Qualities => _qualities;
    private List<QualityObject> _qualities = new List<QualityObject>();
    
    private bool _inUse;
    private Sprite _currentBGSprite;
    private IInventoryManager _manager;
    private bool _isOutSlot;
    private int _itemID = -1;
    private int _numItemsStored = 0;


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
        if (_greenCheckmark == null)
            _greenCheckmark = this.GetComponentInChildren<UICheckMark>(false);
    }
    /************************************************************************************************************************/

    public void SetManager(IInventoryManager manager) { _manager = manager; }
    public void SetAsOutSlot(){ _isOutSlot = true;   }
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
        //if (_manager)
        //    _manager.SetImportant(this.gameObject);
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
        RemoveItem(false);
    }
    public void SharedKanbanSlotChanged(bool isEmpty, List<QualityObject> qualities)
    {
        if (isEmpty)
        {
            RemoveItem(true);
        }
        else
        {
            AssignItem(RequiredID, 1, qualities, true);
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
        return AssignItem(id, count, qualities, false);
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
            ClientSend.Instance.SendItem(_itemID, _qualities, !_isOutSlot);
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

    public void PlayCheckMarkAnim(bool cond)
    {
        if (_greenCheckmark != null)
            _greenCheckmark.gameObject.SetActive(cond);
    }

    public void FakeSetSpriteAsInUse(int id)
    {
        AssignSpriteByID(id, false);
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
    private bool AssignItem(int id, int count, List<QualityObject> qualities, bool noCallBack)
    {
        if (!_inUse)
        {
            if (_isOutSlot && id != RequiredID)
            {
                //Debug.Log($"{id} does not match {RequiredID}");
                return AskManagerIfSpaceForItem(id, count, qualities);
            }
            else if (id == RequiredID)
            {
                ///removed _isOutSlot check here so kanban flags can play check mark
                PlayCheckMarkAnim(true);
            }
            AssignSpriteByID(id, false);
            SwapBackgroundIMGs(true);
            ///Might have to clone it, but lets see if we can store it
            if (qualities != null)
                _qualities = qualities;
            else
                _qualities.Clear();

            _itemID = id;
            _numItemsStored = count;
            _inUse = true;
            if (!noCallBack) ///Don't tell the manager about our stateChange
            {
                ///We do this so we dont get circular network calls from network changes
                TellManager();
            }

            ///Try encapsulating this here:
            SetNormal();
            return true;
        }

        return AskManagerIfSpaceForItem(id, count, qualities);
    }
    private bool AskManagerIfSpaceForItem(int id, int count, List<QualityObject> qualities)
    {
        return _manager.TryAssignItem(id, count, qualities);
    }
    private void RemoveItem(bool noCallback)
    {
        --_numItemsStored;
        UIManager.DebugLog($"Remove ITEM , new count = {_numItemsStored}");
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
                PlayCheckMarkAnim(false);
            }
            else
                RestoreDefault();
        }
        if (!noCallback)
        {
            TellManager();
        }
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
        _manager.SlotStateChanged(this);
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

        if (_greenCheckmark != null)
            _greenCheckmark.ResetState();
    }
    private void SetLarger()
    {
        this.transform.localScale = _LARGER;
    }
    private void SetNormal() 
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

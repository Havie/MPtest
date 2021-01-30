using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIBucketManagement : MonoBehaviour, IAssignable
{
    [SerializeField] Image _myIcon = default;
    [SerializeField] UIInventoryManager _inventory;
    private Sprite _defaultIcon;

    private bool _autoSend = true;

    int _itemID = -1;
    public bool _inUse;

    List<QualityObject> _qualities = new List<QualityObject>();

    public List<QualityObject> Qualities => _qualities;

    private Vector3 _LARGER = new Vector3(1.25f, 1.25f, 1.25f);
    private Vector3 _NORMAL = new Vector3(1, 1, 1);
    private Vector3 _SMALLER = new Vector3(0.25f, 0.25f, 0.25f);

    private Color _VISIBLE = new Color(255, 255, 255, 1);
    private Color _TRANSPARENT = new Color(255, 255, 255, 0.5f);
    private Color _INVALID = new Color(255, 155, 155, 0.5f);

    /************************************************************************************************************************/

    private void Awake() 
    { 
        _defaultIcon = _myIcon.sprite;
        _NORMAL = transform.localScale;
        _LARGER = _NORMAL * 1.45f;
        _SMALLER = _NORMAL * 0.5f;
    }
    /************************************************************************************************************************/


    #region Interface
    public bool GetInUse() => _inUse;
    public bool PreviewSlot(Sprite img)
    {
        bool retVal = true;
         if (!_inUse)
        {
            AssignSprite(img);
            _myIcon.color = _VISIBLE;
        }
        else
        {
            //display something that shows slot is in use
            _myIcon.color = _INVALID;
            retVal = false;
        }

        //SetLarger();
        return retVal;
    }
    public void UndoPreview()
    {
        RestoreDefault();
    }
    public void RemoveItem()
    {
        RestoreDefault();
    }

    public bool AssignItem(ObjectController oc, int count)
    {
        if(oc==null)
        {
            ///The displacement state will try assigning the bucket cast as an ObjectController to itself
            return false; 
        }

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
        //Debug.Log($"ASsign Item in BUCKET! _autoSend={_autoSend}");

        if (!_inUse)
        {
            AssignSpriteByID(id, false);

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
            _inUse = true;
            if (_autoSend)
            {
                TellManager();
                RestoreDefault();
            }



            return true;
        }

        return false;
    }
    #endregion
    public void SendData()
    {
        //Debug.Log("   ....... CALLLED SEND DATA ........        ");
        WorkStation myStation = GameManager.instance._workStation;

        if (_inUse)//&& WorkStation._stationFlow.ContainsKey((int)myStation._myStation))
        {
            //int StationToSend = WorkStation._stationFlow[(int)myStation._myStation];
            UIManager.DebugLog($"(UIInventorySlot) sending: <color=green>{_itemID}</color> to Station: <color=blue>{(int)myStation._sendOutputToStation}</color>");
            ClientSend.SendItem(_itemID, _qualities, (int)myStation._sendOutputToStation);
            CheckKitting();
            RemoveItem(); // should always call RestoreDefault;
        }
        //else
        // Debug.LogError($"Error Sending Out INV slot , no StationKey {(int)myStation._myStation}");
    }

    /************************************************************************************************************************/

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
        if (_inventory)
            _inventory.AddItemToSlot(_itemID, _qualities, false);
    }
    private void RestoreDefault()
    {
        //SetSmaller();
        _inUse = false;
        _itemID = -1;
        AssignSprite(_defaultIcon);
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

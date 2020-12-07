using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-599)] ///Load a little earlier
public class BuildableObject : MonoBehaviour
{
    public static BuildableObject Instance { get; private set; }
    public Color _colorHand1;
    public Color _colorHand2;


    private ObjectManager _manager;

    //These should be connected to something else like the workstation IDs
    public ObjectManager.eItemID _mID; ///pretty much unused atm?

    private List<GameObject> _objects;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if(Instance!=this)
            Destroy(this);

        _manager = Resources.Load<ObjectManager>("ObjectManager");

        _objects = new List<GameObject>();
    }

    #region localWork
    /** TMP: Used at start to mimick setting starting workstationID Eventually nothing should be in scene */
    public void SetItemID(int id)
    {
        //range check TODO ?

        _mID = (ObjectManager.eItemID)id;

        SpawnObject((int)_mID);
    }
    /**Used to advance construction of workspace objects */
    public void AddComponent()
    {
        ++_mID;
        SpawnObject((int)_mID);
    }


    #endregion
    #region globalWork
    /** Gets it based on current ID */
    public Sprite GetCurrentSprite()
    {
        return _manager.GetSprite((int)_mID);
    }
    /** Gets it from the Manager */
    public Sprite GetSpriteByID(int id)
    {
        return _manager.GetSprite(id);
    }
    /** Gets it from the Manager */
    public string GetItemNameByID(int id)
    {
        return _manager.getItemName(id);
    }

    public bool IsBasicItem(ObjectManager.eItemID itemID)
    {
        return _manager.IsBasicItem(itemID);
    }
    public GameObject SpawnObject(int itemID)
    {
        return SpawnObject( itemID, Vector3.zero);
    }
     public GameObject SpawnObject(int itemID, Vector3 pos)
    {
        //Debug.Log($"The spawn loc heard is {pos} and itemID={itemID}." );
        //GetNextObj
        GameObject _currentObj = GameObject.Instantiate<GameObject>
            (_manager.GetObject(itemID), pos, Quaternion.identity);
        _currentObj.transform.Rotate(Vector3.left, 10);
        _currentObj.transform.SetParent(this.transform);

        _objects.Add(_currentObj);

        return _currentObj;
    }

    public void DestroyObject(GameObject obj)
    {
        Destroy(obj);
    }
    #endregion
}

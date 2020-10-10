﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    public static BuildableObject Instance { get; private set; }
    private ObjectManager _manager;

    //These should be connected to something else like the workstation IDs
    public ObjectManager.eItemID _mID;

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
    public GameObject SpawnObject(int itemID)
    {
        return SpawnObject( itemID, Vector3.zero);
    }
     public GameObject SpawnObject(int itemID, Vector3 pos)
    {
        //Debug.Log($"The spawn loc heard is {pos} and mlevel={(int)_mlvl}." );
        //GetNextObj
        GameObject _currentObj = GameObject.Instantiate<GameObject>
            (_manager.GetObject(itemID), pos, Quaternion.identity);
        _currentObj.transform.SetParent(this.transform);

        _objects.Add(_currentObj);

        return _currentObj;
    }
    #endregion
}

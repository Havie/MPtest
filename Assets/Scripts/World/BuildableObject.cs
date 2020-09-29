using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    public static BuildableObject Instance { get; private set; }
    [SerializeField] ObjectManager _manager;

    //These should be connected to something else like the workstation IDs
    public enum eLevel { Cube0, Cube1, Cube2 };
    public eLevel _mlvl;

    private List<GameObject> _objects;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if(Instance!=this)
            Destroy(this);

        _manager = Resources.Load<ObjectManager>("ObjectManager");
        Debug.Log("OBJMAN= " + _manager);

        _objects = new List<GameObject>();
    }


    public void AddComponent()
    {
        ++_mlvl;
        SpawnObject((int)_mlvl);
    }

    public void SetLevel(int level)
    {
        //range check TODO ?

        _mlvl = (eLevel)level;

        SpawnObject((int)_mlvl);
    }
    /** Gets it based on current ID */
    public Sprite GetCurrentSprite()
    {
        return _manager.GetSprite((int)_mlvl);
    }
    /** Gets it from the Manager */
    public Sprite GetSpriteByLevel(int lvl)
    {
        return _manager.GetSprite(lvl);
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
    
}

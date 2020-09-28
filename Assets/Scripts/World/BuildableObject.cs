using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildableObject : MonoBehaviour
{
    public static BuildableObject Instance { get; private set; }
    [SerializeField] ObjectManager _manager;

    public enum eLevel { Cube0, Cube1, Cube2 };
    public eLevel _mlvl;

    private GameObject _currentObj;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if(Instance!=this)
            Destroy(this);

        _manager = Resources.Load<ObjectManager>("ObjectManager");
        Debug.Log("OBJMAN= " + _manager);
    }

    private void Start()
    {

    }


    public void AddComponent()
    {
        ++_mlvl;
        SpawnCurrentObject();
    }

    public void SetLevel(int level)
    {
        //range check TODO ?

        _mlvl = (eLevel)level;

        SpawnCurrentObject();
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
    private void SpawnCurrentObject()
    {
        if (_currentObj != null)
            Destroy(_currentObj);

        Debug.Log("CURRENT LEVEL = " + (int)_mlvl);

        //GetNextObj
        _currentObj = GameObject.Instantiate<GameObject>
            (_manager.GetObject((int)_mlvl), Vector3.zero, Quaternion.identity);
        _currentObj.transform.SetParent(this.transform);
    }
}

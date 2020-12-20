using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-599)] ///Load a little earlier
public class BuildableObject : MonoBehaviour
{
    public static BuildableObject Instance { get; private set; }
    public Color _colorHand1;
    public Color _colorHand2;

    public QualityStep[] _qualityPresets;

    private ObjectManager _manager;

    //These should be connected to something else like the workstation IDs


    private List<GameObject> _objects;

    public bool DebugItemsOnSpawn;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if(Instance!=this)
            Destroy(this);

        _manager = Resources.Load<ObjectManager>("ObjectManager");

        _objects = new List<GameObject>();
    }

    #region globalWork

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
        //var _objStartPos = new Vector3(pos.x, pos.y, UserInput.Instance._tmpZfix);
        GameObject _currentObj = GameObject.Instantiate<GameObject>
            (_manager.GetObject(itemID), pos, Quaternion.identity);
        _currentObj.transform.Rotate(Vector3.left, 0f); ///was 10f to add tilt toward camera but removed when picking up off table
        _currentObj.transform.SetParent(this.transform);

        _objects.Add(_currentObj);


        if(DebugItemsOnSpawn)
            FPSCounter.Instance.ProfileAnObject(_currentObj);

        return _currentObj;
    }

    public ObjectQuality BuildTempQualities(int id, int currAction)
    {
        var qs = this.transform.gameObject.AddComponent<ObjectQuality>();
        qs.InitalizeAsDummy(_qualityPresets[id + 1] ,currAction);

        return qs;
    }

    public void DestroyObject(GameObject obj)
    {
        Destroy(obj);
    }
    #endregion
}

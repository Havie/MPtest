using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-599)] ///Load a little earlier
public class BuildableObject : MonoBehaviour
{
    public static BuildableObject Instance { get; private set; }
    public Color _colorHand1;
    public Color _colorHand2;

    [SerializeField] Transform _spawnPoint = default;

    public QualityStep[] _qualityPresets;

    private ObjectManager _manager;


    //These should be connected to something else like the workstation IDs


    public bool DebugItemsOnSpawn;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        _manager = Resources.Load<ObjectManager>("ObjectManager");


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
        return SpawnObject(itemID, Vector3.zero, null);
    }

    public GameObject SpawnObject(int itemID, Vector3 pos, List<QualityObject> qualities)
    {
        if (itemID == -1)
            Debug.Break();

        //Debug.Log($"The spawn loc heard is {pos} and itemID={itemID}." );
        //GetNextObj
        //var _objStartPos = new Vector3(pos.x, pos.y, UserInput.Instance._tmpZfix);
        var prefab = _manager.GetObject(itemID);
        if (!prefab)
            return null; ///Prevent any NPEs

        GameObject newObj = GameObject.Instantiate<GameObject>
            (prefab, pos, prefab.transform.rotation);
        newObj.transform.Rotate(Vector3.left, 0f); ///was 10f to add tilt toward camera but removed when picking up off table
        newObj.transform.SetParent(this.transform);
        newObj.GetComponent<ObjectController>().SetStartingRotation(prefab.transform.rotation);

        if (qualities != null && qualities.Count > 0)
        {
            var overallQuality = newObj.GetComponent<QualityOverall>();
            if (overallQuality)
            {
                foreach (var q in qualities)
                {
                    overallQuality.ReadOutQuality(q);
                    Destroy(q);
                    Debug.Log("Copied and removed A quality");
                }
            }
            else
                Debug.LogWarning($"Somehow theres qualities associated w a prefab without an OverallQualityManager {newObj.name}");
        }


        if (DebugItemsOnSpawn)
            FPSCounter.Instance.ProfileAnObject(newObj);

        return newObj;
    }

    public QualityObject BuildTempQualities(int id, int currAction)
    {
        var qs = this.transform.gameObject.AddComponent<QualityObject>();
        Debug.Log("id=" + id);
        qs.InitalizeAsDummy(_qualityPresets[id - 1], currAction);

        return qs;
    }

    public void DestroyObject(GameObject obj){Destroy(obj);}

    public GameObject DropItemInWorld(int itemID)
    {
        // Debug.Log($"Drop item ID: {itemID}");
        var prefab = _manager.GetObject(itemID);
        if (!prefab || !_spawnPoint)
            return null; ///Prevent any NPEs


        GameObject newObj = GameObject.Instantiate<GameObject>
          (prefab, GetRandomPos(_spawnPoint.position), prefab.transform.rotation);

        newObj.transform.Rotate(GetRandomPos(newObj.transform.position), 0f); ///was 10f to add tilt toward camera but removed when picking up off table
        newObj.transform.SetParent(this.transform);
       
        var controller = newObj.GetComponent<ObjectController>();
        if (controller)
        {
            controller.SetStartingRotation(prefab.transform.rotation);
            controller.PutDown(); ///turn on physics 
        }

        if (DebugItemsOnSpawn)
            FPSCounter.Instance.ProfileAnObject(newObj);

        return newObj;
    }
    public GameObject DropItemInWorld(int itemID, List<QualityObject> qualities)
    {
        var prefab = _manager.GetObject(itemID);
        if (!prefab || !_spawnPoint)
            return null; ///Prevent any NPEs

        GameObject newObj = GameObject.Instantiate<GameObject>
            (prefab, GetRandomPos(_spawnPoint.position), prefab.transform.rotation);

        newObj.transform.Rotate(GetRandomPos(newObj.transform.position), 0f); ///was 10f to add tilt toward camera but removed when picking up off table
        newObj.transform.SetParent(this.transform);

        var controller = newObj.GetComponent<ObjectController>();
        if (controller)
            controller.PutDown(); ///turn on physics 

        LoadQualitiesOntoObject(qualities, newObj);

        return newObj;
    }

    private Vector3 GetRandomPos(Vector3 near)
    {
        float x = Random.Range(near.x - 0.75f, near.x + 0.75f);
        float y = Random.Range(near.y - 1, near.y + 1);
        float z = Random.Range(near.z - 0.25f, near.z);

        return new Vector3(x, y, z);
    }

    private void LoadQualitiesOntoObject(List<QualityObject> qualities, GameObject newObj)
    {
        if (qualities != null && qualities.Count > 0)
        {
            var overallQuality = newObj.GetComponent<QualityOverall>();
            if (overallQuality)
            {
                List<QualityObject> childrenQualities = newObj.GetComponentsInChildren<QualityObject>().ToList();
                foreach (QualityObject clonedQuality in qualities)
                {
                    foreach (QualityObject currQuality in childrenQualities)
                    {
                        if (clonedQuality.QualityStep == currQuality.QualityStep)
                        {
                            currQuality.AssignCurrentActions(clonedQuality.CurrentActions);
                        }
                        else
                        {
                            overallQuality.ReadOutQuality(clonedQuality);
                        }
                        //Debug.Log("Copied and removed A quality");
                        Destroy(clonedQuality);///This is the dummy component being stored
                    }
                }
            }
            else
                Debug.LogWarning($"<color=yellow>!!</color>..Somehow theres qualities associated w a prefab without an OverallQualityManager {newObj.name}");
        }
    }

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(10000)] // load late after game manager
public class InjectorWorkStation : MonoBehaviour
{
     WorkStationManager _workStationManager;
    [SerializeField] bool _testSceneInjection = false;
    [SerializeField] int _stationToInject = 1; //Change this to a enum drop down
    [SerializeField] ObjectManager.eItemID _itemToInject = default;


    private List<WorkStation> _workStations;

    void Start()
    {
        _workStationManager = GameManager.instance.CurrentWorkStationManager;
        _workStations = _workStationManager.GetStationList();
        if (_testSceneInjection)
        {
            AssignWorkStation(_stationToInject);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            InjectItem((int)_itemToInject);
    }

    /** TMP for testing */
    private void AssignWorkStation(int index)
    {
        if (_workStations.Count > index)
        {
            GameManager.instance.AssignWorkStation(_workStations[index]);
            UIManager.instance.BeginLevel(_stationToInject); // random#?
        }
    }


    private void InjectItem(int ItemID)
    {
        UserInputManager.Instance.InjectItem(ItemID);
    }
}

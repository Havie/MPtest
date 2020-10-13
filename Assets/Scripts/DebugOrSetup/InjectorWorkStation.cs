using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InjectorWorkStation : MonoBehaviour
{
    [SerializeField] WorkStationManager _workStationManager;
    [SerializeField] bool _testSceneInjection = false;
    [SerializeField] int _stationToInject = 1; //Change this to a enum drop down


   private List<WorkStation> _workStations;

    void Start()
    {
        _workStations = _workStationManager.GetStationList();
        if (_testSceneInjection)
        {
            AssignWorkStation(_stationToInject);
        }
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
}

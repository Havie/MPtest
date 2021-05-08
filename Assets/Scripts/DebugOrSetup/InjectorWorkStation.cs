using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class began as a developer tool to demo different stations without coming in from the networking menu
/// but as the networking menu was re-written, it started using this pathway too.
/// However it still holds some development hacks to test various things  (_developerInjections)
/// </summary>
[DefaultExecutionOrder(10000)] // load late after game manager
public class InjectorWorkStation : MonoBehaviour
{

    WorkStationManager _workStationManager;

    [SerializeField] int _stationToInject = 1; ///TODO Change this to a enum drop down


    [Header("Dev Hacks")]
    [SerializeField] bool _developerInjections = true;
    [SerializeField] ObjectRecord.eItemID _itemToInject = default;
    [SerializeField] BatchEvent _batchSentEvent;

    private List<WorkStation> _workStations;

    /************************************************************************************************************************/

    void Start()
    {
        if (UIManager.LoadedFromMenu)
        {
            _stationToInject = UIManager.StationToInject;
        }

       //Debug.Log($"The scene injector thinks: <color=green>{_stationToInject}</color> vs  <color=red>{UIManager.LoadedFromMenu}</color> "); 
        _workStationManager = GameManager.Instance.CurrentWorkStationManager;
        _workStations = _workStationManager.GetStationList();
        AssignWorkStation(_stationToInject);
    }


    private void AssignWorkStation(int index)
    {
        if (_workStations.Count > index)
        {
            GameManager.Instance.AssignWorkStation(_workStations[index]);
            UIManager.BeginLevel(); // _stationToInject#?
        }
    }


    /************************************************************************************************************************/
    /// Development Hacks:
    /************************************************************************************************************************/

    private void Update()
    {
        if (!_developerInjections)
            return;
        if (Input.GetKeyDown(KeyCode.I))
            InjectItem((int)_itemToInject);
        if (Input.GetKeyDown(KeyCode.S))
            FakeOrderShipped();

    }
    private void InjectItem(int ItemID)
    {
        UserInput.UserInputManager.Instance.InjectItem(ItemID);
    }

    /// <summary>
    /// This wont work if not networked. aka Cant load from Inventory Scene, must come in thru Networking
    /// </summary>
    private void FakeOrderShipped()
    {
        if (_batchSentEvent)
        {
            WorkStation ws = GameManager.Instance._workStation;
            bool isShippingStation = true;
            _batchSentEvent.Raise(new BatchWrapper((int)ws._myStation, 1, isShippingStation));
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "WorkStationManager", menuName = "WorkStationManager")]
public class WorkStationManager : ScriptableObject
{
    [SerializeField] List<WorkStation> _workStations = default;

    public WorkStation GetStationByEnumIndex(int index)
    {
        foreach (var item in GetStationList())
        {
            if ((int)item._myStation == index)
                return item;
        }
        return null;
    }

    public WorkStation GetStationByEnum(WorkStation.eStation station)
    {
        foreach (var item in GetStationList())
        {
            if (item._myStation == station)
                return item;
        }
        return null;
    }

    //for testing injections
    public List<WorkStation> GetStationList() => _workStations;

    public int GetStationCount() => _workStations.Count ;

    public void SetupDropDown(Dropdown dropdown)
    {
        //Debug.Log($"Setting up DropDown for {this.name}");
        if(dropdown)
        {
            List<string> dropOptions = new List<string>();
            foreach (WorkStation ws in _workStations)
            {
                dropOptions.Add(ws.StationName);
                //Debug.Log($"...adding {ws._stationName}");
            }

            dropdown.ClearOptions();
            dropdown.AddOptions(dropOptions);
        }
       
    }

    /** Called From UILobby, Updates the GameManager and Network */
    public void UpdateStation(WorkStation ws)
    {
        if (ws != null)
        {
            GameManager.instance.AssignWorkStation(ws);
            ClientSend.Instance.SendWorkStationID((int)ws._myStation, ws.StationLocation);
        }
        else
        {
            ClientSend.Instance.SendWorkStationID(0, Vector3.zero);
        }    
    }
    public KeyValuePair<WorkStation, string> GetStationPair(Dropdown dropdown)
    {
        int wsID = dropdown.value;
        WorkStation ws = _workStations[wsID];
        Debug.Log($"<color=purple>[WSM] DropDownVal= </color> {wsID} => {ws.StationName}");
        var outStation = ws._sendOutputToStation;
        string outName = "None";
        foreach (var station in _workStations)
        {
            if (station._myStation == outStation)
            {
                outName= station.StationName;
                break;
            }
        }

        return new KeyValuePair<WorkStation, string>(ws, outName);
    }
    public string GetStationOutput(Dropdown dropdown)
    {
        int wsID = dropdown.value;
        WorkStation ws = _workStations[wsID];
        var outStation = ws._sendOutputToStation;

        foreach (var station in _workStations)
        {
            if (station._myStation == outStation)
                return station.StationName;
        }

        return "None";
    }
}

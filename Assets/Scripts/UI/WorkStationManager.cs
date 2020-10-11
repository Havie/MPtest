using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(fileName = "WorkStationManager", menuName = "WorkStationManager")]
public class WorkStationManager : ScriptableObject
{
    [SerializeField] List<WorkStation> _workStations;



    //for testing injections
    public List<WorkStation> GetStationList() => _workStations;

    public int GetStationCount() => _workStations.Count ;

    public void SetupDropDown(Dropdown dropdown)
    {
        if(dropdown)
        {
            List<string> dropOptions = new List<string>();
            foreach (WorkStation ws in _workStations)
                dropOptions.Add(ws._stationName);

            dropdown.ClearOptions();
            dropdown.AddOptions(dropOptions);
        }

    }


    /**Called from UI asset confirm workstation button  */
    public int ConfirmStation(Dropdown dropdown)
    {
        int wsID = dropdown.value;
        WorkStation ws = _workStations[wsID];
        GameManager.instance.AssignWorkStation(ws);
        ClientSend.SendWorkStationID((int)ws._myStation);

        //UIManager.instance.BeginLevel(dropdown.value); //This # might need to change and be based on the workstation, index #s probably wont perfectly match steps
        return wsID; //eventually might need to get an itemID FROM the workstation as we split tasks
    }



}

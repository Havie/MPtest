﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWorkStationManager : MonoBehaviour
{
    [SerializeField] List<WorkStation> _workStations;
    [SerializeField] Dropdown _dropdown;

    [SerializeField] bool test = false;

    private void Start()
    {
        if(_dropdown)
        {
            List<string> dropOptions = new List<string>();
            foreach (WorkStation ws in _workStations)
                dropOptions.Add(ws._stationName);

            _dropdown.ClearOptions();
            _dropdown.AddOptions(dropOptions);
        }

        if (test)
            AssignWorkStation(2);
    }

    /**Called from UI asset confirm workstation button  */
    public void AssignWorkStation(int index)
    {
        if(_workStations.Count > index)
        {
            GameManager.instance.AssignWorkStation(_workStations[index]);
        }
    }
    public void ConfirmStation()
    {
        WorkStation ws = _workStations[_dropdown.value];
        GameManager.instance.AssignWorkStation(ws);
        UIManager.instance.BeginLevel(_dropdown.value); //This # might need to change and be based on the workstation
        ClientSend.SendWorkStationID((int)ws._myStation);
    }

    public int GetStationCount() => _workStations.Count-1;
}

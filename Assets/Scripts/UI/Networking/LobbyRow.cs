﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LobbyRow : MonoBehaviour
{
    [SerializeField] Text _playerNumber = default;
    [SerializeField] Text _playerName = default;
    [SerializeField] Dropdown _stationDropDown = default;
    [SerializeField] Button _taskInfo = default;
    [SerializeField] Text _outputLabel = default;

    private string _outputDefault = "None";

    private WorkStationManager _wsManager;

    public void initialize(int num, string name, WorkStationManager dropDownManager, bool isInteractable, int stationID)
    {
        _playerNumber.text = num.ToString();
        _wsManager = dropDownManager;
        _wsManager.SetupDropDown(_stationDropDown);
        ManuallyChangeStation(stationID);
        UpdateData(name, isInteractable, stationID);
        _outputLabel.text = _outputDefault;
    }

    public void UpdateData(string name, bool isInteractable, int stationID)
    {
        _playerName.text = name;
        ManuallyChangeStation(stationID);
        SetInteractable(isInteractable);
    }

    private void SetInteractable(bool cond)
    {
        _stationDropDown.interactable = cond;
        _taskInfo.interactable = cond;
    }

    private void ManuallyChangeStation(int index)
    {
        _stationDropDown.value = index; ///MAYBE?
    }

    /// <summary> From Button /// </summary>
    public void OnStationChanged()
    {
        //Update Output Label
        if (_wsManager)
        {
            _outputLabel.text = _wsManager.GetStationOutput(_stationDropDown);
        }
    }  
    
    public void ConfirmStation()
    {
        int stationID = _wsManager.ConfirmStation(_stationDropDown);
        UIManagerNetwork.instance.ConfirmWorkStation(stationID);
    }


}

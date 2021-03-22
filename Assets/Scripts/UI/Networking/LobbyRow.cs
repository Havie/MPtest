using System.Collections;
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

    public void initialize(int num, string name, WorkStationManager dropDownManager, bool isInteractable)
    {
        _playerNumber.text = num.ToString();
        _playerName.text = name;
        _wsManager = dropDownManager;
        _wsManager.SetupDropDown(_stationDropDown);
        _outputLabel.text = _outputDefault;
        SetInteractable(isInteractable);
    }

    private void SetInteractable(bool cond)
    {
        _stationDropDown.interactable = cond;
        _taskInfo.interactable = cond;
    }


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

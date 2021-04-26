using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LobbyRow : MonoBehaviour
{
    public System.Action<WorkStation> OnSelectionChanged;
    public int WorkStationID { get; private set; } = -1;

    [SerializeField] Text _playerNumber = default;
    [SerializeField] Text _playerName = default;
    [SerializeField] LobbyDropdown _stationDropDown = default;
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

    public void SetLockedDropDownIndicies(List<int> invalidIndicies)
    {
        _stationDropDown.SetLockedDropDownIndicies(invalidIndicies);
    }

    /// <summary> Called From Button </summary>
    public void OnStationChanged()
    {
        //Update Output Label
        if (_wsManager)
        {
            var stationPair = _wsManager.GetStationPair(_stationDropDown);
            _outputLabel.text = stationPair.Value;
            WorkStationID = (int)stationPair.Key._myStation;
            OnSelectionChanged?.Invoke(stationPair.Key);
        }
    }
    
    /// <summary> Called From Button </summary>
    public void OnToggleInstructions()
    {
        var lobbyInstructions = LobbyInstructions.Instance;
        if(lobbyInstructions)
        {
            lobbyInstructions.ToggleInstructions();
        }
    }
    //**************PRIVATE******************************************************************//

    private void SetInteractable(bool cond)
    {
        _stationDropDown.interactable = cond;
        _taskInfo.interactable = cond;
    }

    private void ManuallyChangeStation(int index)
    {
        _stationDropDown.value = index; ///MAYBE?
    }



}

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

    private static string _outputDefault = "None";

    private WorkStationManager _wsManager;
    private WorkStation _lastKnownStation;
    private bool _isActiveRow = false;
    //***************************************************************************************//
    public void initialize(int num, string name, WorkStationManager dropDownManager, bool isInteractable, int stationID)
    {
        this.gameObject.name = "LobbyRow #" + num.ToString();
        _playerNumber.text = num.ToString();
        _wsManager = dropDownManager;
        _wsManager.SetupDropDown(_stationDropDown);
        ManuallyChangeStation(stationID);
        UpdateData(name, isInteractable, stationID);
        OnSelectionChanged += MonitorTaskInfoButton;
        MonitorTaskInfoButton(null);
        _outputLabel.text = _outputDefault;
    }

    public void UpdateData(string name, bool isInteractable, int stationID)
    {
        _playerName.text = name;
        SetInteractable(isInteractable);
        ManuallyChangeStation(stationID);
    }

    /// <summary>Sets which values in the dropdown are interactable </summary>
    public void SetLockedDropDownIndicies(List<int> invalidIndicies)
    {
        _stationDropDown.SetLockedDropDownIndicies(invalidIndicies);
    }

    /// <summary> Called anytime DropDown component changes at all, by UserInput or ManuallyChangeStation() </summary>
    public void OnStationChanged()
    {
        //Update Output Label
        if (_wsManager)
        {
            var stationPair = _wsManager.GetStationPair(_stationDropDown);
            var newKey = (int)stationPair.Key._myStation;
            if (_isActiveRow && newKey != WorkStationID)
            {
                HideInstructions(); ///If changed we cant update as ezily, and if change to NONE cant show at all, so best to hide instead of update
                OnSelectionChanged?.Invoke(stationPair.Key);
            }
            WorkStationID = newKey;
            _outputLabel.text = stationPair.Value;
        }
    }

    /// <summary> Called From Button </summary>
    public void OnToggleInstructions()
    {
        var lobbyInstructions = LobbyInstructions.Instance;
        if (lobbyInstructions)
        {
            Sprite img = _lastKnownStation == null ? null : _lastKnownStation.StationInstructions;
            lobbyInstructions.ToggleInstructions(img);
        }
    }
    //**************PRIVATE******************************************************************//

    /// <summary> Used to enable/disable this row, useful so players cant edit other players settings </summary>
    private void SetInteractable(bool cond)
    {
        _isActiveRow = cond;
        _stationDropDown.interactable = cond;
        if (_isActiveRow)
        {
            var stationPair = _wsManager.GetStationPair(_stationDropDown);
            MonitorTaskInfoButton(stationPair.Key);
        }
        else
        {
            _taskInfo.interactable = cond;
        }
    }

    /// <summary> StationIDs match dropdown Indicies, so its an easy correlation</summary>
    private void ManuallyChangeStation(int index)
    {
        _stationDropDown.value = index;
    }

    /// <summary> Whether or not to show the task button based on valid task </summary>
    private void MonitorTaskInfoButton(WorkStation ws)
    {
        var invalid = ws == null || ws._myStation == WorkStation.eStation.SELF;
        _taskInfo.interactable = !invalid;
        _lastKnownStation = ws;
    }
    /// <summary>Force Hides station Instructions </summary>
    private void HideInstructions()
    {
        var lobbyInstructions = LobbyInstructions.Instance;
        if (lobbyInstructions)
        {
            lobbyInstructions.ShowInstructions(false);
        }
    }
}

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
    public void Initialize(int num, string name, WorkStationManager dropDownManager, bool isInteractable, int stationID)
    {
        this.gameObject.name = "LobbyRow #" + num.ToString();
        _playerNumber.text = num.ToString();
        //ManuallyChangeStation(stationID);
        UpdateWorkStationManager(dropDownManager);
        UpdateData(name, isInteractable, stationID);
        MonitorTaskInfoButton(null);
        _outputLabel.text = _outputDefault;
    }
    
    /// <summary> Updates the current row stored & displayed data </summary>
    public void UpdateData(string name, bool isInteractable, int stationID)
    {
        _playerName.text = name;
        SetInteractable(isInteractable);
        /// Must assign the dropdown index first, or un-interactable rows are stuck at index 0
        ManuallyChangeStation(stationID);
        ///Update station without invoking a network change
        UpdateStation();
    }
    
    /// <summary> Update the current WorkStationManager so proper instructions/station names are displayed and selected on start</summary>
    public void UpdateWorkStationManager(WorkStationManager wsm)
    {
        _wsManager = wsm;
        /// Hold onto our last index to reset after
        int lastIndex = _stationDropDown.value; 
        /// Repopulates the dropdown list with new station names (pointless now that we renamed all stations to 1,2,3,4 etc)
        _wsManager.SetupDropDown(_stationDropDown); 
        /// Put our selected index back to what we had chosen before
        ManuallyChangeStation(lastIndex);
        ///Update station without invoking a network change
        UpdateStation(); 
    }

    /// <summary>Sets which values in the dropdown are interactable </summary>
    public void SetLockedDropDownIndicies(List<int> invalidIndicies)
    {
        _stationDropDown.SetLockedDropDownIndicies(invalidIndicies);
    }

    #region Buttons
    /// <summary> Called from LobbyDropDown Component anytime theres a change by UserInput </summary>
    public void OnStationChanged()
    {
        ///An importantChange Occurred or not :
        if (UpdateStation())
        {
            ///Will update the server / everyone else:
            KeyValuePair<WorkStation, string> stationPair = _wsManager.GetStationPair(_stationDropDown);
            OnSelectionChanged?.Invoke(stationPair.Key);
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
    #endregion
    //**************PRIVATE******************************************************************//
    /// <summary>Updates the workStation and hides the Instructions </summary>
    private bool UpdateStation()
    {
        bool importantChange = false;
        //Update Output Label
        if (_wsManager)
        {
            ///**Technically null safe becuz the wsm always retusn the default SELF station for NONE
            KeyValuePair<WorkStation, string> stationPair = _wsManager.GetStationPair(_stationDropDown);
            int newKey = (int)stationPair.Key._myStation;
            importantChange = _isActiveRow && newKey != WorkStationID;
            WorkStationID = newKey;
            ///Update the task info buttons workstation so it can get new instructions
            MonitorTaskInfoButton(stationPair.Key);
            ///Update this outputLabel to show who this station sends to
            _outputLabel.text = stationPair.Value;
            if (importantChange)
            {
                HideInstructions(); ///If changed simply easier to hide instead of update
            }
        }
        else
            Debug.Log($"<color=red>{gameObject.name}::NO WSM</color>");

        Debug.Log($"<color=yellow> UpdateStation() </color> = {importantChange} .._isActiveRow={_isActiveRow}");
        return importantChange;
    }
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
        Debug.Log($"<color=orange>ManuallyChangeStation-></color> {index}");
        _stationDropDown.value = index;
    }

    /// <summary> Whether or not to show the task button based on valid task </summary>
    private void MonitorTaskInfoButton(WorkStation ws)
    {
        if (!_isActiveRow)
            return; ///Dont let ppl click other stations instructions
        var invalid = ws == null || ws._myStation == WorkStation.eStation.SELF;
        _taskInfo.interactable = !invalid;
        _lastKnownStation = ws;///Cache the passed in WS so the instructions can query it
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

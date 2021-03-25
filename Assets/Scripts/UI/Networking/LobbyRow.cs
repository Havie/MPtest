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
    [SerializeField] Dropdown _stationDropDown = default;
    [SerializeField] Button _taskInfo = default;
    [SerializeField] Text _outputLabel = default;

    private string _outputDefault = "None";

    private WorkStationManager _wsManager;
    private List<int> _invalidDropDownIndicies;

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

    public void LockDropDownItems(List<int> invalidIndicies)
    {
        _invalidDropDownIndicies = invalidIndicies;
        Debug.Log($"LockDropDownItems: {_stationDropDown} c= {_invalidDropDownIndicies.Count} ");

        string inde = "";
        foreach (var index in invalidIndicies)
        {
            inde += $"{index}, ";
        }
        Debug.Log($" The invalid indicies are : {inde}");


        int c = 0;
        ///The true here included inactive buttons!
        foreach (var button in _stationDropDown.GetComponentsInChildren<Toggle>(true))
        {
            button.interactable = !invalidIndicies.Contains(c);
            Debug.Log($"{c} .. set {button.gameObject.name} to interactble = {!invalidIndicies.Contains(c)}");
            ++c;
        }
    }


    /// <summary> From Button /// </summary>
    public void OnDropDownEnabled()
    {
        Debug.Log("LobbyRow is enabled");
    }

    /// <summary> From Button /// </summary>
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

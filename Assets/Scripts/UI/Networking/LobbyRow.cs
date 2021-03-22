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


    public void initialize(int num, string name, string outputStation)
    {
        _playerNumber.text = num.ToString();
        _playerName.text = name;
        _outputLabel.text = outputStation;
    }

}

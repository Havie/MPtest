using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{

    [SerializeField] Transform _lobbyDiv = default;
    [SerializeField] Button _startGameButton = default;
    [SerializeField] Text _numPlayerText = default;

    GameObject _lobbyRowPrefab;
    WorkStationManager _workstationManager;
    int _rowNumber = 0;
    LobbyRow _interactableRow;

    List<LobbyRow> _rows;

    List<int> _lockedDropdownIds;



    private void Awake()
    {
        _lobbyRowPrefab = Resources.Load<GameObject>("Prefab/UI/LobbyRow");
        _rows = new List<LobbyRow>();
        _lockedDropdownIds = new List<int>();
    }

    private void Start()
    {
        ///Get some info from GameManager regarding settings
        _workstationManager = GameManager.Instance.CurrentWorkStationManager;
        _startGameButton.interactable= UIManagerNetwork.Instance.RegisterLobbyMenu(this); //Slightly circular >.<
        Refresh();
        WorkStationChanged(null); ///Will notify otherplayers were connected with No ws selected
    }
    public void ReceieveRefreshData(List<LobbyPlayer> incommingData)
    {
        _lockedDropdownIds.Clear();

        foreach (LobbyRow row in _rows)
        {
            var entry = incommingData[0];
            incommingData.Remove(entry);
            row.UpdateData(entry.Username, entry.IsInteractable, entry.StationID);
            _lockedDropdownIds.Add(entry.StationID); ///Dupes?
        }

        foreach (var newPlayer in incommingData)
        {
            PlayerConnected(newPlayer.Username, newPlayer.IsInteractable, newPlayer.StationID);
            _lockedDropdownIds.Add(newPlayer.StationID); ///Dupes?
        }

        if(_interactableRow)
        {
            _interactableRow.SetLockedDropDownIndicies(_lockedDropdownIds);
        }
    }

    public void PlayerConnected(string name, bool isInteractable, int stationID)
    {
        var row = CreateRow(name, isInteractable, stationID);
        _rows.Add(row);
        if (isInteractable)
        {
            if (_interactableRow != null)
            {
                Debug.Log($"<color=yellow> more than 1 interactble Row?</color> {_interactableRow}");
                _interactableRow.OnSelectionChanged -= WorkStationChanged;
            }
            _interactableRow = row;
            _interactableRow.OnSelectionChanged += WorkStationChanged;
        }
    }

    /// <summary> This will send a message across network for other players to see changes 
    /// and update this players gameManager </summary>
    public void WorkStationChanged(WorkStation ws)
    {
        if(_workstationManager)
            _workstationManager.UpdateStation(ws);
    }

   
    /// <summary> Called From Button only available to host /// </summary>
    public void HostWantsToStartRound()
    {
        UIManagerNetwork.Instance.HostStartsRound();
    }
   
    /// <summary> Called From Network /// </summary>
    public int GetStationSelectionID()
    {
        if (_interactableRow)
        {
            return _interactableRow.WorkStationID;
        }

        ///Should never happen
        Debug.LogError("This should never happen");
        return 0;
    }

    /// <summary> Called From Button /// </summary>
    public void Refresh()
    {
        UIManagerNetwork.Instance.RequestRefresh();
    }
    
   
    //**************PRIVATE******************************************************************//
    private LobbyRow CreateRow(string name, bool isInteractable, int stationID)
    {
        if (_lobbyRowPrefab)
        {
            LobbyRow row = Instantiate(_lobbyRowPrefab, _lobbyDiv).GetComponent<LobbyRow>();
            row.initialize(++_rowNumber, name, _workstationManager, isInteractable, stationID);
            _numPlayerText.text = $" {_rowNumber}/{sNetworkManager.Instance.maxPlayers}";
            return row;
        }
        return null;
    }
}

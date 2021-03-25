using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{

    [SerializeField] Transform _lobbyDiv = default;
    [SerializeField] Button _startGame = default;

    GameObject _lobbyRowPrefab;
    WorkStationManager _workstationManager;
    int _rowNumber = 0;
    LobbyRow _interactableRow;

    List<LobbyRow> _rows;

    List<int> _lockedDropdownIds;

    /// <summary>
    /// NEED TO RECEIEVE GAMEDATA SeTTINGs
    /// </summary>

    private void Awake()
    {
        _lobbyRowPrefab = Resources.Load<GameObject>("UI/LobbyRow");
        _rows = new List<LobbyRow>();
        _lockedDropdownIds = new List<int>();
    }

    private void Start()
    {
        ///Get some info from GameManager regarding settings
        _workstationManager = GameManager.Instance.CurrentWorkStationManager;
        Debug.Log($"..lobby WSMAN= {_workstationManager}");
        UIManagerNetwork.Instance.RegisterLobbyMenu(this);//Slightly circular >.<
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
            _interactableRow.LockDropDownItems(_lockedDropdownIds);
        }
    }

    private void Update()
    {
        ///TMP
        if (Input.GetKeyDown(KeyCode.P))
            PlayerConnected("host", true, -1);
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

    /// <summary> Called From Button /// </summary>
    public void StartRound()
    {
        if (_interactableRow)
        {
            var stationID =_interactableRow.WorkStationID;
            UIManagerNetwork.instance.ConfirmWorkStation(stationID);
        }
    }

    /// <summary> Called From Button /// </summary>
    public void Refresh()
    {
        UIManagerNetwork.Instance.RequestRefresh();
    }
    private LobbyRow CreateRow(string name, bool isInteractable, int stationID)
    {
        LobbyRow row = Instantiate(_lobbyRowPrefab, _lobbyDiv).GetComponent<LobbyRow>();
        row.initialize(++_rowNumber, name, _workstationManager, isInteractable, stationID);
        return row;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{

    [SerializeField] Transform _lobbyDiv = default;

    GameObject _lobbyRowPrefab;
    WorkStationManager _workstationManager;
    int _rowNumber = 0;
    LobbyRow _interactableRow;

    List<LobbyRow> _rows;

    private void Awake()
    {
        _lobbyRowPrefab = Resources.Load<GameObject>("UI/LobbyRow");
        _rows = new List<LobbyRow>();
    }

    private void Start()
    {
        ///Get some info from GameManager regarding settings
        _workstationManager = GameManager.Instance.CurrentWorkStationManager;
        UIManagerNetwork.Instance.RegisterLobbyMenu(this);//Slightly circular >.<
    }
    public void ReceieveRefreshData(List<LobbyPlayer> playerData)
    {
        foreach (LobbyRow row in _rows)
        {
            var entry = playerData[0];
            playerData.Remove(entry);
            row.UpdateData(entry.Username, entry.IsInteractable, entry.StationID);
        }

        foreach (var newPlayer in playerData)
        {
            PlayerConnected(newPlayer.Username, newPlayer.IsInteractable, newPlayer.StationID);
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
            }
            _interactableRow = row;
        }
    }

    /// <summary> Called From Button /// </summary>
    public void StartRound()
    {
        if (_interactableRow)
            _interactableRow.ConfirmStation();
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

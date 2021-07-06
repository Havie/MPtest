using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMenu : MonoBehaviour
{

    [SerializeField] Transform _lobbyDiv = default;
    [SerializeField] Button _startGameButton = default;
    [SerializeField] Button _hostTabButton = default;
    [SerializeField] Text _numPlayerText = default;

    GameObject _lobbyRowPrefab;
    WorkStationManager _workstationManager;
    int _rowNumber = 0;
    LobbyRow _interactableRow;

    List<LobbyRow> _rows;

    List<int> _lockedDropdownIds;

    //****************************************************************************************//

    private void Awake()
    {
        _lobbyRowPrefab = Resources.Load<GameObject>("Prefab/UI/LobbyRow");
        _rows = new List<LobbyRow>();
        _lockedDropdownIds = new List<int>();
    }

    private void Start()
    {
        bool iAmHost = UIManagerNetwork.Instance.RegisterLobbyMenu(this); //Slightly circular >.<
        _startGameButton.interactable = iAmHost;
        _hostTabButton.interactable = iAmHost;
        ///TODO find someotherway to abstract this class from knowing where to get the current WSM
        WorkStationManagerChanged(GameManager.Instance.CurrentWorkStationManager);
        Refresh();
        WorkStationChanged(null); ///Will notify otherplayers were connected with No ws selected

    }
    //****************************************************************************************//
    public void ReceieveRefreshData(List<LobbyPlayer> incommingData)
    {

        _lockedDropdownIds.Clear();

        ///Update our existing rows by essentially removing _rows.Count from incommingData (will become an issue if players leave)
        foreach (LobbyRow row in _rows)
        {
            if (incommingData.Count < 1)
            {
                row.gameObject.SetActive(false);
                break;
            }
            row.gameObject.SetActive(true);
            LobbyPlayer entry = incommingData[0]; ///Always 0 since we remove right below this line
            incommingData.Remove(entry); ///Get rid of this entry we just saw
            row.UpdateData( entry.Username, entry.IsInteractable, entry.StationID);
            _lockedDropdownIds.Add(entry.StationID); ///Dupes?
        }
        ///Any left over entries in incommingData means theres new players, so make a new row:
        foreach (LobbyPlayer newPlayer in incommingData)
        {
            PlayerConnected(newPlayer.Username, newPlayer.IsInteractable, newPlayer.StationID);
            _lockedDropdownIds.Add(newPlayer.StationID); ///Dupes?
        }

        if (_interactableRow)
        {
            _interactableRow.SetLockedDropDownIndicies(_lockedDropdownIds);
        }
    }

    /// <summary> This will send a message across network for other players to see changes 
    /// and update this players gameManager </summary>
    public void WorkStationChanged(WorkStation ws)
    {
        if (_workstationManager)
            _workstationManager.UpdateStation(ws);
    }

    /// <summary> Swap out manager and update the lobby rows:</summary>
    /// TODO- should get rid of the rows needing to hold this reference somehow?
    public void WorkStationManagerChanged(WorkStationManager wsm)
    {
        _workstationManager = wsm;
        foreach (LobbyRow row in _rows)
        {
            row.UpdateWorkStationManager(_workstationManager);
        }
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
            row.Initialize(++_rowNumber, name, _workstationManager, isInteractable, stationID);
            _numPlayerText.text = $" {_rowNumber}/{sNetworkManager.Instance.maxPlayers}";
            return row;
        }
        return null;
    }


    private void PlayerConnected(string name, bool isInteractable, int stationID)
    {
        var row = CreateRow(name, isInteractable, stationID);
        _rows.Add(row);
        if (isInteractable)  ///Only set up a listener if this is the current players active row
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
}

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


    private void Awake()
    {
        _lobbyRowPrefab = Resources.Load<GameObject>("UI/LobbyRow");
    }

    private void Start()
    {
        ///Get some info from GameManager regarding settings
        _workstationManager = GameManager.Instance.CurrentWorkStationManager; 
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            PlayerConnected("host", true);
    }

    public void PlayerConnected(string name, bool isInteractable)
    {
        
        var row =CreateRow(name, isInteractable);
        if (isInteractable)
            _interactableRow = row;
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

    }

    private LobbyRow CreateRow(string name, bool isInteractable)
    {
        LobbyRow row = Instantiate(_lobbyRowPrefab, _lobbyDiv).GetComponent<LobbyRow>();
        row.initialize(++_rowNumber, name, _workstationManager, isInteractable);
        return row;
    }
}

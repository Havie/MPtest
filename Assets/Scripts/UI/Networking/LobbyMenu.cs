using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyMenu : MonoBehaviour
{

    [SerializeField] Transform _lobbyDiv = default;

    private GameObject _lobbyRowPrefab;


    private void Awake()
    {
        _lobbyRowPrefab = Resources.Load<GameObject>("UI/LobbyRow");
    }

    public void PlayerConnected()
    {
        
    }


    private void CreateRow()
    {
        var row = Instantiate(_lobbyRowPrefab, _lobbyDiv);
    }
}

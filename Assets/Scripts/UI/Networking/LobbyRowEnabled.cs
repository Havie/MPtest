using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyRowEnabled : MonoBehaviour
{

    [SerializeField] LobbyRow _lobbyRow = default;
    private void OnEnable()
    {
        if (_lobbyRow)
            _lobbyRow.OnDropDownEnabled();
    }
}

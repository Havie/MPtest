using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayer
{

    public int ID { get; private set; }
    public string Username { get; private set; }
    public int StationID { get; private set; }
    public bool IsInteractable { get; private set; } ///this is the active players data for this session

    public LobbyPlayer(int id, string username, int stationID, bool isInteractable)
    {
        ID = id;
        Username = username;
        StationID = stationID;
        IsInteractable = isInteractable;
    }

}

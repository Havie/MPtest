using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class sPlayerData 
{

  static List<sPlayer> _players = new List<sPlayer>();


    public static void AddPlayerInfo(string userName, int clientID)
    {
        sPlayer pl = new sPlayer(clientID, userName);
        _players.Add(pl);
    }
    public static void RemovePlayerInfo(int clientID)
    {
        foreach (var player in _players)
        {
            if(player.ID == clientID)
            {
                _players.Remove(player);
                return;
            }
        }
    }

    public static void SetStationDataForPlayer(int stationID, int fromClient)
    {
        foreach (var player in _players)
        {
            if(player.ID == fromClient)
            {
                player.SetStationID(stationID);
                return;
            }
        }

        Debug.Log($"<color=red> Didnt find player {fromClient} to assign Station to? {stationID} </color>");
    }

    public static List<sPlayer> GetPlayerData()
    {
        return _players;
    }
}

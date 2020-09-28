using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sServerHandle
{

    public static void WelcomeReceived(int fromClient, sPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($"{sServer._clients[fromClient]._tcp._socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}");
        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }


    }
    public static void PlayerMovement(int fromClient, sPacket packet)
    {
        bool[] inputs = new bool[packet.ReadInt()];
        for (int i = 0; i < inputs.Length; ++i)
            inputs[i] = packet.ReadBool();

        Quaternion rotation = packet.ReadQuaternion();


        sClient client = sServer._clients[fromClient];
        if(client!=null)
        {
            if(client._player!=null)
                client._player.SetInput(inputs, rotation);
        }
    }

    public static void StationIDReceived(int fromClient, sPacket packet)
    {
        int stationID = packet.ReadInt();
        Debug.Log("The stationID Read was : " + stationID);
        sClient client = sServer._clients[fromClient];
        if (client != null)
            client._workStation = stationID;
        else
            Debug.Log("Found an error w StationIDReceived");
    }

    public static void ItemReceived(int fromClient, sPacket packet)
    {

        int itemLvl = packet.ReadInt();
        Debug.Log("The itemLvl Read was : " + itemLvl);
        int stationID = packet.ReadInt();
        Debug.Log("The stationID Read was : " + stationID);

        foreach (sClient c in sServer._clients.Values)
        {
            //if client workstation ID matches stationID 
            if(c._workStation == stationID)
            {
                //Send the item to their inventory:
                c.SendItem(itemLvl);
            }


        }

    }

}

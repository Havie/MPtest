using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sServerHandle
{

    ///This is just a callback to verify the client connected properly
    public static void WelcomeReceived(int fromClient, sPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($"<color=blue>{sServer._clients[fromClient]._tcp._socket.Client.RemoteEndPoint} </color> <color=green>{username}</color> connected successfully and is now player {fromClient}");
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

        List<int> qualities = new List<int>();

        var count = packet.ReadInt();
        Debug.Log($"ServerHandle QualityCount={count}");
        string info = "";
        ///Reconstruct the Object Quality data
        for (int i = 0; i < count; ++i)
        {
            var id = packet.ReadInt();
            var curAction = packet.ReadInt();
            qualities.Add(id); ///quality ID
            qualities.Add(curAction); ///quality Count

            info += $" server pack :({id},{curAction}) ";
        }

        foreach (sClient c in sServer._clients.Values) ///This isnt great, its circular, i shud remove this if i wasnt so afraid to break the networking code
        {
            //if client workstation ID matches stationID 
            if(c._workStation == stationID)
            {
                //Send the item to their inventory:
                c.SendItem(itemLvl, qualities);
            }

            Debug.Log(info);

        }

    }

}

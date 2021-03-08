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

        Debug.Log($"[ServerHandle] <color=blue>{sServer._clients[fromClient]._tcp._socket.Client.RemoteEndPoint} </color> <color=green>{username}</color> connected successfully and is now player {fromClient}");
        if (fromClient != clientIdCheck)
        {
            Debug.Log($"[ServerHandle] Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }


    }
    public static void PlayerMovement(int fromClient, sPacket packet)
    {
        bool[] inputs = new bool[packet.ReadInt()];
        for (int i = 0; i < inputs.Length; ++i)
            inputs[i] = packet.ReadBool();

        Quaternion rotation = packet.ReadQuaternion();


        sClient client = sServer._clients[fromClient];
        if (client != null)
        {
            if (client._player != null)
                client._player.SetInput(inputs, rotation);
        }
    }

    public static void StationIDReceived(int fromClient, sPacket packet)
    {
        int stationID = packet.ReadInt();
        Debug.Log("[ServerHandle] stationID Read was : " + stationID);
        sClient client = sServer._clients[fromClient];
        if (client != null)
            client._workStation = stationID;
        else
            Debug.Log("Found an error w StationIDReceived");
    }

    public static void ItemReceived(int fromClient, sPacket packet)
    {

        int itemLvl = packet.ReadInt();
        Debug.Log("[ServerHandle] itemLvl Read was : " + itemLvl);
        int stationID = packet.ReadInt();
        Debug.Log("[ServerHandle] stationID Read was : " + stationID);

        List<int> qualities = new List<int>();

        var count = packet.ReadInt();
        Debug.Log($"[ServerHandle] QualityCount={count}");
        // string info = "";
        ///Reconstruct the Object Quality data
        for (int i = 0; i < count; ++i)
        {
            var id = packet.ReadInt();
            var curAction = packet.ReadInt();
            qualities.Add(id); ///quality ID
            qualities.Add(curAction); ///quality Count

            //  info += $" server pack :({id},{curAction}) ";
        }

        foreach (sClient c in sServer._clients.Values) ///This isnt great, its circular, i shud remove this if i wasnt so afraid to break the networking code
        {
            //if client workstation ID matches stationID 
            if (c._workStation == stationID)
            {
                //Send the item to their inventory:
                c.SendItem(itemLvl, qualities);
            }

            //if (!info.Equals(""))
            //    Debug.Log(info);

        }

    }

    public static void BatchReceived(int fromClient, sPacket packet)
    {

        int stationID = packet.ReadInt();
        int batchSize = packet.ReadInt();
        bool wasShipped = packet.ReadBool();

        Debug.Log("[sServerHandle] stationID Read was : " + stationID);
        Debug.Log("[sServerHandle] batchSize Read was : " + batchSize);

        if (fromClient != stationID)
            Debug.Log($"[ServerHandle]!!..<color=yellow> why do IDs not match , game end vs Server end?</color>  {fromClient} vs {stationID}");

        sServer._gameStatistics.StationSentBatch(stationID, batchSize, wasShipped, Time.time);

        var cycleTime = sServer._gameStatistics.GetCycleTimeForStation(stationID, Time.time);

        Debug.Log($"The CycleTime for Station#{stationID} is currently: <color=purple> {cycleTime} </color>");
    }

    public static void OrderCreated(int fromClient, sPacket packet)
    {
        int itemID = packet.ReadInt();
        float createdTime = packet.ReadFloat();
        float dueTime = packet.ReadFloat();

        //Debug.Log("<color=green>[sServerHandle]</color> itemID Read was : " + itemID);
        //Debug.Log("<color=green>[sServerHandle]</color> createdTime Read was : " + createdTime);
        //Debug.Log("<color=green>[sServerHandle]</color> dueTime Read was : " + dueTime);

        sServer._gameStatistics.CreatedAnOrder(itemID, createdTime, dueTime);

        Debug.Log("<color=purple>[sServerHandle]</color> WIP= : " + sServer._gameStatistics.GetWIP());

    }

    public static void DefectAdded(int fromClient, sPacket packet)
    {
        int stationID = packet.ReadInt();
        int itemID = packet.ReadInt();

        if (fromClient != stationID)
            Debug.Log($"[ServerHandle]!!..<color=yellow> why do IDs not match , game end vs Server end?</color>  {fromClient} vs {stationID}");


        Debug.Log("<color=orange>[sServerHandle]</color> itemID Read was : " + stationID);
        Debug.Log("<color=orange>[sServerHandle]</color> createdTime Read was : " + itemID);

        sServer._gameStatistics.AddedADefect(stationID, itemID);


        Debug.Log($"Current Defects#={sServer._gameStatistics.Defects}");
    }


    public static void RoundBegin(int fromClient, sPacket packet)
    {
        float roundStart = packet.ReadFloat();
        int roundDuration = packet.ReadInt();

        Debug.Log("<color=white>[sServerHandle]</color> RoundBegin @ : " + roundStart);

        ///I wish something on the server was ticking so we could keep track of time on it,
        ///but instead we will let the hosts Timer call an end event to trigger RoundEnd
        ///We could Tick on the sNetworkManager but feels wrong

        sServer._gameStatistics.RoundBegin(roundStart);
        foreach (sClient c in sServer._clients.Values) ///This isnt great, its circular, i shud remove this if i wasnt so afraid to break the networking code
        {
            ///Tell all clients to start: (this sets the timer)
            c.StartRound(roundDuration);
        }

    }

    public static void RoundEnded(int fromClient, sPacket packet)
    {
        float endTime = packet.ReadFloat();

        Debug.Log("<color=white>[sServerHandle]</color> RoundEnded @ : " + endTime);

        var gameStats = sServer._gameStatistics;

        gameStats.RoundEnded(endTime);

        foreach (sClient c in sServer._clients.Values) ///This isnt great, its circular, i shud remove this if i wasnt so afraid to break the networking code
        {
            int workStationId = c._workStation;
            float cycleTime = gameStats.GetCycleTimeForStation(workStationId, endTime);
            float thruPut= gameStats.GetThroughput();
            int shippedOnTime = gameStats.GetShippedOnTime();
            int shippedLate = gameStats.GetShippedLate();
            int wip = gameStats.GetWIP();

            c.EndRound(cycleTime, thruPut, shippedOnTime, shippedLate, wip);



        }
    }
}

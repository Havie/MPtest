using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using dataTracking;

public class sServerHandle
{

    ///This is just a callback to verify the client connected properly
    public static void WelcomeReceived(int fromClient, sPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString();

        Debug.Log($"[ServerHandle] <color=blue>{sServer._clients[fromClient].Tcp._socket.Client.RemoteEndPoint} </color> <color=green>{username}</color> connected successfully and is now player {fromClient}");
        if (fromClient != clientIdCheck)
        {
            Debug.Log($"[ServerHandle] Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
            return;
        }

        ///Store this data in a way that is useable for the LobbyMenu
        sPlayerData.AddPlayerInfo(username, clientIdCheck);

    }

   
    public static void StationInfoReceived(int fromClient, sPacket packet)
    {
        int stationID = packet.ReadInt();
        //Debug.Log($"[ServerHandle] stationID Read was :  {stationID} ");
        ///This is somewhat unsafe
        sClient client = sServer._clients[fromClient];
        if (client != null)
        {
            client.SetWorkStationInfo(stationID);
            sPlayerData.SetStationDataForPlayer(stationID, fromClient);

        }
        else
            Debug.Log("Found an error w StationIDReceived");

        ///Refresh the other clients on the network with this change
        foreach (var clientEntry in sServer._clients)
        {
            var otherClient = clientEntry.Value;
            if(otherClient != client)
            {
                sServerSend.SendMultiPlayerData(clientEntry.Key);
            }
        }
    }

    public static void RequestMultiPlayerData(int fromClient, sPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        //Debug.Log("[ServerHandle] RequestMultiPlayerData from : " + clientIdCheck);
        ///This is silly... considering these already match in WelcomeReceived, and whatever calls us
        if (clientIdCheck < 0 || clientIdCheck >= sServer._clients.Count)
        {
            Debug.Log("Found an error w clientIdCheck");
            return;
        }

        sServerSend.SendMultiPlayerData(fromClient);

    }

    public static void ItemReceived(int fromClient, sPacket packet)
    {

        int itemLvl = packet.ReadInt();
        int stationID = packet.ReadInt();
        List<int> qualities = ReconstructQualityData(packet);

        foreach (sClient c in sServer.GetClients()) 
        {
            //if client workstation ID matches stationID 
            if (c.WorkStationID == stationID)
            {
                Debug.Log($"...SENT item to client # {c.ID}");
                //Send the item to their inventory:
                c.SendItem(itemLvl, qualities);
            }

            //if (!info.Equals(""))
            //    Debug.Log(info);

        }

    }

    private static List<int> ReconstructQualityData(sPacket packet)
    {
        List<int> qualities = new List<int>();
        var count = packet.ReadInt();
        ///Reconstruct the Object Quality data
        for (int i = 0; i < count; ++i)
        {
            var id = packet.ReadInt();
            var curAction = packet.ReadInt();
            qualities.Add(id); ///quality ID
            qualities.Add(curAction); ///quality Count
        }
        return qualities;
    }
    ///TODO test this
    private static int[] ReconstructQualityDataArr(sPacket packet)
    {
        int count = packet.ReadInt();
        int[] qualities = new int[count * 2];
        ///Reconstruct the Object Quality data
        for (int i = 0; i < count; ++i)
        {
            var id = packet.ReadInt();
            var curAction = packet.ReadInt();
            qualities[i] = (id); ///quality ID
            ++i;
            qualities[i] = (curAction); ///quality Count
        }
        return qualities;
    }

    public static void BatchReceived(int fromClient, sPacket packet)
    {

        int stationID = packet.ReadInt();
        int batchSize = packet.ReadInt();
        bool wasShipped = packet.ReadBool();

        // Debug.Log("[sServerHandle] BatchReceived: stationID Read was : " + stationID);
        // Debug.Log("[sServerHandle] BatchReceived: batchSize Read was : " + batchSize);

        //if (fromClient != stationID)
        //    Debug.Log($"[ServerHandle]!!..<color=yellow> why do IDs not match , game end vs Server end?</color>  {fromClient} vs {stationID}");

        sServer._gameStatistics.StationSentBatch(stationID, batchSize, wasShipped, Time.time);

        var cycleTime = sServer._gameStatistics.GetCycleTimeForStation(stationID);
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
       // Debug.Log("<color=orange>[sServerHandle]</color>DEFECT itemID Read was : " + stationID);
       // Debug.Log("<color=orange>[sServerHandle]</color>DEFECT createdTime Read was : " + itemID);
        sServer._gameStatistics.AddedADefect(stationID, itemID);

        Debug.Log($"Current DEFECTS#={sServer._gameStatistics.Defects}");
    }

    public static void RoundBegin(int fromClient, sPacket packet)
    {
        float roundStart = packet.ReadFloat();
        int roundDuration = packet.ReadInt();

        //Debug.Log("<color=white>[sServerHandle]</color> RoundBegin @ : " + roundStart);

        ///I wish something on the server was ticking so we could keep track of time on it,
        ///but instead we will let the hosts Timer call an end event to trigger RoundEnd
        ///We could Tick on the sNetworkManager but feels wrong

        sServer._gameStatistics.RoundBegin(roundStart);
        var batchSize = GameManager.Instance._batchSize;
        foreach (sClient c in sServer.GetClients())
        {
            if (batchSize == 1)
            {
                ///Set up the KanBan flags for pull
                c.RequestTransportInfo();
            }
            ///Tell all clients to start: (this sets the timer, and loads the scene)
            ///This will call all 6 since they are init, but calls wont go anywhere for those not connected
           c.StartRound(roundDuration);
        }

    }

    public static void RoundEnded(int fromClient, sPacket packet)
    {
        float endTime = packet.ReadFloat();

        Debug.Log("<color=white>[sServerHandle]</color> RoundEnded @ : " + endTime);

        var gameStats = sServer._gameStatistics;

        gameStats.RoundEnded(endTime);

        float thruPut = gameStats.GetThroughput();
        int shippedOnTime = gameStats.GetShippedOnTime();
        int shippedLate = gameStats.GetShippedLate();
        int wip = gameStats.GetWIP();
        RoundResults rs = new RoundResults(thruPut, shippedOnTime, shippedLate, wip);

        foreach (sClient c in sServer._clients.Values) ///This isnt great, its circular, i shud remove this if i wasnt so afraid to break the networking code
        {
           // Debug.Log($"[ServerHandle] sees Client: {c} , {c._id} vs {c._workStation} ");
            int workStationId = c.WorkStationID;
            if (workStationId == 0)
            {
                ///Zero means one of the clients was never assigned a stationID (could be host?)
                continue;
            }
            ///Cycle time is the only one unique to a station:
            float cycleTime = gameStats.GetCycleTimeForStation(workStationId);
            /// I am worried the workStationID frin Client doesnt correlate to the ingame WS
           //Debug.Log("[ServerHandle] stationID RoundEnd was : " + workStationId);
            rs.SetCycleTime(workStationId, cycleTime);
            c.EndRound(cycleTime, thruPut, shippedOnTime, shippedLate, wip);

        }
        ///Print out and store our round results
        FileSaver.WriteToFile(rs);
        sServer.ResetStatistics();
        sServer.ResetSharedInventories();
    }

    /// <summary>
    /// The reason why this is slightly different from using sClient.WorkStationID is that
    /// we dont necessarily have access to the info of who the workstations output is
    /// and if we have this info we might as well store it in  a map for quicker access
    /// I would like the entire client send system to move towards a map approach in time,
    /// Behind on sprints right now, so no time to redesign
    /// </summary>
    public static void ReceivedTransportData(int fromClient, sPacket packet)
    {

        var ownersStationID = packet.ReadInt();
        var receiversStationID = packet.ReadInt();
        var sharedInvs = sServer._sharedInventories;
        ///On a refactor we could do something like this, then w a bool flag, re-build the key properly
        //KeyValuePair<int, int> _stationPair = new KeyValuePair<int, int>(receiversStationID, ownersStationID);
        //Debug.Log($"<color=white>!..!..! </color>{fromClient} sent us : stationID:{ownersStationID} to out:{receiversStationID}");
        sharedInvs.RegisterClientToStationId(fromClient, ownersStationID);
        sharedInvs.BuildInventory(ownersStationID, receiversStationID, KanbanFlagChanged);
        ///I would like to store this in the "sharedinventories" class but thats only batch==1
        Vector3 stationLoc = packet.ReadVector3();
        foreach (var client in sServer.GetClients())
        {
            if(client.WorkStationID == ownersStationID)
            {
                client.SetWorldLocation(stationLoc);
            }
        }
    }

    public static void InventoryChanged(int fromClient, sPacket packet)
    {
        bool isInInventory = packet.ReadBool();
        bool isRemovedItem = packet.ReadBool();
        int itemID = packet.ReadInt();
        List<int> qualities = ReconstructQualityData(packet);
        var sharedInvs = sServer._sharedInventories;
        Debug.Log($"<color=white>[ServerHandle]</color> heard InvChanged for : clientID{fromClient} , to isIN={isInInventory} isEmpty={isRemovedItem} ");

        sharedInvs.UpdateInventories(isRemovedItem, isInInventory, fromClient, itemID, qualities);
    }

    public delegate void KanbanChangedEvent(int caller, int needsToKnow, bool wasInInventory, bool isEmpty, int itemID, List<int> qualityData);

    private static void KanbanFlagChanged(int callerStationID, int needsToKnowStationID, bool invType, bool isEmpty, int itemID, List<int> qualityData)
    {
        Debug.Log($"Heard the kanban flag for callerStation:{callerStationID} , NeedstoKnowStation:{needsToKnowStationID} cond:{isEmpty}");
        var sharedInvs = sServer._sharedInventories;
        ///Should we instead do sServer.GetClients() way ?
        int clientID = sharedInvs.GetClientIDForStation(needsToKnowStationID);
        if (clientID != -1)
        {
            Debug.Log($"Tell Serverclient#:{clientID} to update inventory to = {isEmpty}");
            Debug.Log($"Changer was In = {invType}");
            sServerSend.SharedInventoryChanged(clientID, invType, isEmpty, itemID, qualityData);
        }
        else
            Debug.Log($"whoops no client found");
    }
}

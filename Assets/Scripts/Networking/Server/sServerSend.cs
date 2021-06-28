using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sServerSend
{
    private static void SendTCPData(int toClient, sPacket packet)
    {
        packet.WriteLength();
        sServer._clients[toClient].Tcp.SendData(packet);
    }

    private static void SendUDPData(int toClient, sPacket packet)
    {
        packet.WriteLength();
        sServer._clients[toClient].Udp.SendData(packet);
    }
    #region Packets
    private static void SendTCPDataToAll(sPacket packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= sServer._maxPlayers; ++i)
            sServer._clients[i].Tcp.SendData(packet);
    }

    private static void SendTCPDataToAll(int exceptClient, sPacket packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= sServer._maxPlayers; ++i)
        {
            if (i != exceptClient)
                sServer._clients[i].Tcp.SendData(packet);
        }
    }
    private static void SendUDPDataToAll(sPacket packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= sServer._maxPlayers; ++i)
            sServer._clients[i].Udp.SendData(packet);
    }

    private static void SendUDPDataToAll(int exceptClient, sPacket packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= sServer._maxPlayers; ++i)
        {
            if (i != exceptClient)
                sServer._clients[i].Udp.SendData(packet);
        }
    }

    public static void Welcome(int toClient, string msg)
    {
        /// Write all the GameManager DATA:
        using (sPacket packet = new sPacket((int)ServerPackets.welcome)) //Auto call packet.Dispose when done "UsingBlock"
        {
            packet.Write(msg);
            packet.Write(toClient);

            var instance = GameManager.Instance;
            packet.Write(instance._orderFrequency);
            packet.Write(instance._isStackable); ///needs to be before batchChanged
            packet.Write(instance._batchSize);
            packet.Write(instance._autoSend);
            packet.Write(instance._addChaotic);
            packet.Write(instance._workStationArrangement);
            packet.Write(instance._workStationTaskChanging);
            packet.Write(instance._HUDManagement);
            packet.Write(instance._HostDefectPausing);
            packet.Write(instance._roundDuration);


            SendTCPData(toClient, packet);
        }
    }

    public static void SendMultiPlayerData(int toClient)
    {
        var players = sPlayerData.GetPlayerData();
        using (sPacket packet = new sPacket((int)ServerPackets.sendMpData))
        {
            packet.Write(players.Count);

            foreach (var player in players)
            {
                packet.Write(player.ID);
                packet.Write(player.Username);
                packet.Write(player.StationID);
            }

            SendTCPData(toClient, packet);
        }

    }

    public static void RequestTransportInfo(int toClient)
    {
        using (sPacket packet = new sPacket((int)ServerPackets.requestTransportData))
        {
            SendTCPData(toClient, packet);
        }
    }
    public static void StartRound(int toClient, int roundDuration)
    {
        using (sPacket packet = new sPacket((int)ServerPackets.roundStart))
        {
            packet.Write(roundDuration);

            SendTCPData(toClient, packet);

        }
    }

    public static void EndRound(int toClient, float cycleTime, float thruPut, int shippedOnTime, int shippedLate, int wip)
    {
        using (sPacket packet = new sPacket((int)ServerPackets.roundEnd))
        {
            packet.Write(cycleTime);
            packet.Write(thruPut);
            packet.Write(shippedOnTime);
            packet.Write(shippedLate);
            packet.Write(wip);

            SendTCPData(toClient, packet);

        }
    }

    public static void SendItem(int toClient, int itemID, List<int> qualityData)
    {
        using (sPacket packet = new sPacket((int)ServerPackets.item))
        {
            WriteQualityData(itemID, qualityData, packet);

            SendTCPData(toClient, packet);

        }
    }

    ///TODO make sure this is thread safe?
    private static void WriteQualityData(int itemID, List<int> qualityData, sPacket packet)
    {
        packet.Write(itemID);
        if (qualityData != null)
        {
            packet.Write(qualityData.Count);
            ///pass along the quality Data
            for (int i = 0; i < qualityData.Count; ++i)
            {
                packet.Write(qualityData[i]);
            }
        }
        else
            packet.Write(0);
    }

    public static void OrderShipped(int itemID)
    {
        using (sPacket packet = new sPacket((int)ServerPackets.orderShipped))
        {
            packet.Write(itemID);
            SendTCPDataToAll(packet);
        }
    }

    public static void SharedInventoryChanged(int toClient, bool isInInventory, bool isEmpty, int itemID, List<int> qualityData)
    {

        using (sPacket packet = new sPacket((int)ServerPackets.sharedInventoryChanged))
        {
            packet.Write(isInInventory);
            packet.Write(isEmpty);
            WriteQualityData(itemID, qualityData, packet);
            SendTCPData(toClient, packet);
        }

    }

    public static void NewOrderCreated(int toClient, int itemID, float createTime, float expectedTime)
    {
        using (sPacket packet = new sPacket((int)ServerPackets.newOrderCreated))
        {
            packet.Write(itemID);
            packet.Write(createTime);
            packet.Write(expectedTime);
            SendTCPData(toClient, packet);
        }
    }
    #endregion
}


#region OLD-ForServerSidePLayerMovement
//public static void SpawnPlayer(int toClient, sPlayer player)
//{
//    //Debug.Log("ServerSend.SpawnPlayer:: " + player._id);
//    using (sPacket packet = new sPacket((int)ServerPackets.spawnPlayer))
//    {
//        packet.Write(player.ID);
//        packet.Write(player.Username);
//        packet.Write(player.transform.position);
//        packet.Write(player.transform.rotation);

//        //Important info cant risk losing so use TCP over UDP
//        SendTCPData(toClient, packet);
//    }
//}

//public static void PlayerPosition(sPlayer player)
//{
//    using (sPacket packet = new sPacket((int)ServerPackets.playerPosition))
//    {
//        packet.Write(player.ID);
//        packet.Write(player.transform.position);

//        SendUDPDataToAll(packet);
//    }

//}

//public static void PlayerRotation(sPlayer player)
//{
//    using (sPacket packet = new sPacket((int)ServerPackets.playerRotation))
//    {
//        packet.Write(player.ID);
//        packet.Write(player.transform.rotation);
//        //exclude self
//        SendUDPDataToAll(player.ID, packet);
//    }

//}
#endregion
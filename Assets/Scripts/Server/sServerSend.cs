﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sServerSend
{
    private static void SendTCPData(int toClient, sPacket packet)
    {
        packet.WriteLength();
        sServer._clients[toClient]._tcp.SendData(packet);
    }

    private static void SendUDPData(int toClient, sPacket packet)
    {
        packet.WriteLength();
        sServer._clients[toClient]._udp.SendData(packet);
    }
    #region Packets
    private static void SendTCPDataToAll(sPacket packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= sServer._maxPlayers; ++i)
            sServer._clients[i]._tcp.SendData(packet);
    }

    private static void SendTCPDataToAll(int exceptClient, sPacket packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= sServer._maxPlayers; ++i)
        {
            if (i != exceptClient)
                sServer._clients[i]._tcp.SendData(packet);
        }
    }
    private static void SendUDPDataToAll(sPacket packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= sServer._maxPlayers; ++i)
            sServer._clients[i]._udp.SendData(packet);
    }

    private static void SendUDPDataToAll(int exceptClient, sPacket packet)
    {
        packet.WriteLength();
        for (int i = 1; i <= sServer._maxPlayers; ++i)
        {
            if (i != exceptClient)
                sServer._clients[i]._udp.SendData(packet);
        }
    }

    public static void Welcome(int toClient, string msg)
    {
        using (sPacket packet = new sPacket((int)ServerPackets.welcome)) //Auto call packet.Dispose when done "UsingBlock"
        {
            packet.Write(msg);
            packet.Write(toClient);
            SendTCPData(toClient, packet);
        }
    }


    public static void SpawnPlayer(int toClient, sPlayer player)
    {
        //Debug.Log("ServerSend.SpawnPlayer:: " + player._id);
        using (sPacket packet = new sPacket((int)ServerPackets.spawnPlayer))
        {
            packet.Write(player._id);
            packet.Write(player._username);
            packet.Write(player.transform.position);
            packet.Write(player.transform.rotation);

            //Important info cant risk losing so use TCP over UDP
            SendTCPData(toClient, packet);
        }

    }

    public static void PlayerPosition(sPlayer player)
    {
        using (sPacket packet = new sPacket((int)ServerPackets.playerPosition))
        {
            packet.Write(player._id);
            packet.Write(player.transform.position);

            SendUDPDataToAll(packet);
        }

    }

    public static void PlayerRotation(sPlayer player)
    {
        using (sPacket packet = new sPacket((int)ServerPackets.playerRotation))
        {
            packet.Write(player._id);
            packet.Write(player.transform.rotation);
            //exclude self
            SendUDPDataToAll(player._id, packet);
        }

    }

    public static void SendItem(int toClient, int itemID)
    {
        using (sPacket packet = new sPacket((int)ServerPackets.item))
        {
            packet.Write(itemID);

            SendTCPData(toClient, packet);

        }
    }

    #endregion
}

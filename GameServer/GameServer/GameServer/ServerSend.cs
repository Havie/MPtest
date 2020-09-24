using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer
{
    class ServerSend
    {
        private static void SendTCPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server._clients[toClient]._tcp.SendData(packet);
        }

        private static void SendUDPData(int toClient, Packet packet)
        {
            packet.WriteLength();
            Server._clients[toClient]._udp.SendData(packet);
        }
        #region Packets
        private static void SendTCPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server._maxPlayers; ++i)
                Server._clients[i]._tcp.SendData(packet);
        }

        private static void SendTCPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server._maxPlayers; ++i)
            {
                if (i != exceptClient)
                    Server._clients[i]._tcp.SendData(packet);
            }
        }
        private static void SendUDPDataToAll(Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server._maxPlayers; ++i)
                Server._clients[i]._udp.SendData(packet);
        }

        private static void SendUDPDataToAll(int exceptClient, Packet packet)
        {
            packet.WriteLength();
            for (int i = 1; i <= Server._maxPlayers; ++i)
            {
                if (i != exceptClient)
                    Server._clients[i]._udp.SendData(packet);
            }
        }

        public static void Welcome(int toClient, string msg)
        {
            using (Packet packet = new Packet((int)ServerPackets.welcome)) //Auto call packet.Dispose when done "UsingBlock"
            {
                packet.Write(msg);
                packet.Write(toClient);
                SendTCPData(toClient, packet);
            }
        }


        public static void SpawnPlayer(int toClient, Player player)
        {
            //Console.WriteLine("ServerSend.SpawnPlayer:: " + player._id);
            using (Packet packet = new Packet((int)ServerPackets.spawnPlayer))
            {
                packet.Write(player._id);
                packet.Write(player._username);
                packet.Write(player._position);
                packet.Write(player._rotation);

                //Important info cant risk losing so use TCP over UDP
                SendTCPData(toClient, packet);
            }

        }

        public static void PlayerPosition(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerPosition))
            {
                packet.Write(player._id);
                packet.Write(player._position);

                SendUDPDataToAll(packet);
            }

        }

        public static void PlayerRotation(Player player)
        {
            using (Packet packet = new Packet((int)ServerPackets.playerRotation))
            {
                packet.Write(player._id);
                packet.Write(player._rotation);
                //exclude self
                SendUDPDataToAll(player._id, packet);
            }

        }

        #endregion

    }
}

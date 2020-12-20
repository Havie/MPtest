using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(sPacket packet)
    {
        packet.WriteLength();
        Client.instance._tcp.SendData(packet);
    }
    private static void SendUDPData(sPacket packet)
    {
        packet.WriteLength();
        Client.instance._udp.SendData(packet);
    }

    #region packets
    public static void WelcomeReceived()
    {
        using (sPacket packet = new sPacket((int)ClientPackets.welcomeReceived))
        {
           // Debug.Log("THE ID Sent= " + Client.instance._myId  + " the string= " + UIManager.instance._usernameField);
            packet.Write(Client.instance._myId);
            packet.Write(UIManager.instance._usernameField.text);
            //TODO fix this and send it AFTER we've chosen a work station (hardcoded atm)
            packet.Write((int)GameManager.instance._workStation._myStation);

            SendTCPData(packet);
        }
    }


    public static void SendWorkStationID(int stationID)
    {
        using (sPacket packet = new sPacket((int)ClientPackets.stationID))
        {
            packet.Write(stationID);

            SendTCPData(packet);

        }
    }

    public static void SendItem(int itemLVL, List<ObjectQuality> qualities, int toStationID)
    {
        UIManager.instance.DebugLog("(ClientSend): Sending Item on channel : " + (int)ClientPackets.item);
        using (sPacket packet = new sPacket((int)ClientPackets.item))
        {
            packet.Write(itemLVL);
            ///Who am I sending it to? (station/ClientID?)
            packet.Write(toStationID);
            ///How many qualities to parse in a for loop
            packet.Write(qualities.Count);
            Debug.Log($"ClientSend QualityCount={qualities.Count}");
            string info = "";
            for (int i = 0; i < qualities.Count; ++i)
            {
                ObjectQuality q = qualities[i];
                packet.Write(q.ID);
                packet.Write(q.CurrentQuality);

                info += $" send:({q.ID},{q.CurrentQuality}) ";
            }

            UIManager.instance.DebugLog(info);

            SendTCPData(packet);

        }
    }

    #endregion


    #region OldFromTutorial
    public static void PlayerMovement(bool[] inputs)
    {
        using (sPacket packet = new sPacket((int)ClientPackets.playerMovement))
        {
            packet.Write(inputs.Length);
            foreach (bool input in inputs)
                packet.Write(input);

            //use UDP cause we can afford to lose data (faster)
            // packet.Write(GameManager._players[Client.instance._myId].transform.rotation);

            SendUDPData(packet);
        }
    }
    #endregion
}


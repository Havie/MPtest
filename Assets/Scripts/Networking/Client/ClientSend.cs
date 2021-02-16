using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend :MonoSingleton<ClientSend>
{
    private  void SendTCPData(sPacket packet)
    {
        packet.WriteLength();
        Client.instance._tcp.SendData(packet);
    }
    private  void SendUDPData(sPacket packet)
    {
        packet.WriteLength();
        Client.instance._udp.SendData(packet);
    }

    #region packets
    public  void WelcomeReceived()
    {
        using (sPacket packet = new sPacket((int)ClientPackets.welcomeReceived))
        {
           // Debug.Log("THE ID Sent= " + Client.instance._myId  + " the string= " + UIManager.instance._usernameField);
            packet.Write(Client.instance._myId);
            packet.Write(UIManagerNetwork.instance._usernameField.text);
            //TODO fix this and send it AFTER we've chosen a work station (hardcoded atm)
            //packet.Write((int)GameManager.Instance._workStation._myStation);

            SendTCPData(packet);
        }
    }


    public  void SendWorkStationID(int stationID)
    {
        using (sPacket packet = new sPacket((int)ClientPackets.stationID))
        {
            packet.Write(stationID);

            SendTCPData(packet);

        }
    }

    public void SendItem(int itemLVL, List<QualityObject> qualities, int toStationID)
    {
        UIManager.DebugLog("(ClientSend): Sending Item on channel : " + (int)ClientPackets.item);
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
                QualityObject q = qualities[i];
                packet.Write(q.ID);
                packet.Write(q.CurrentQuality);

                info += $" send:({q.ID},{q.CurrentQuality}) ";
            }

            UIManager.DebugLog(info);

            SendTCPData(packet);

        }
    }

    /// <summary>
    /// For now, this is only for tracking statistics across the network...
    /// TODO: It would be really nice to encapsulate sending Items from this Batch, not the individual slots calling SendItem()
    /// however the way this was built with AutoSend and UIInventoryManager it would take some re-designing, plan to return to fix this
    /// </summary>
    /// <param name="batch"></param>
    public void BatchSent(BatchWrapper batch)
    {
        Debug.Log($"<color=yellow>Client Handling BatchSent </color>{batch.StationId} , {batch.ItemCount}");

        using (sPacket packet = new sPacket((int)ClientPackets.batch))
        {
            packet.Write(batch.StationId);
            packet.Write(batch.ItemCount);
            SendTCPData(packet);
        }
    }


    #endregion


    #region OldFromTutorial
    public void PlayerMovement(bool[] inputs)
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


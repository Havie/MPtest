using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientSend : MonoSingleton<ClientSend>
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
    /// <summary> We heard back from the server, so send along our Info/// </summary>
    public void WelcomeReceived()
    {
        using (sPacket packet = new sPacket((int)ClientPackets.welcomeReceived))
        {
           // Debug.Log("THE ID Sent= " + Client.instance._myId  + " the string= " + UIManager.instance._usernameField);
            packet.Write(Client.instance._myId);
            if (sServer._iAmHost)
            {
                packet.Write("Host");
            }
            else
            {
                packet.Write(UIManagerNetwork.instance._usernameField.text);
            }
            SendTCPData(packet);
        }
    }

    public void SendWorkStationID(int stationID)
    {
        using (sPacket packet = new sPacket((int)ClientPackets.stationID))
        {
            packet.Write(stationID);

            SendTCPData(packet);

        }
    }

    public void RequestMPData()
    {
        using (sPacket packet = new sPacket((int)ClientPackets.requestMpData))
        {
            packet.Write(Client.instance._myId);

            SendTCPData(packet);

        }
    }
    public void HostWantsToBeginRound()
    {
        RoundBegin();
    }
    public void RoundBegin()
    {
        ///Weird stuff could happen if a client clicks the host button before its grayed out
        ///Since its "Host-->Create ROOM" that actually starts server/hosting
        ///but "Host" that sets this flag. Will investigate later when we refine networking menus
        if (!sServer._iAmHost)
            return;

        Debug.Log($"<color=white>(ClientSend) Round Begin </color>");
        using (sPacket packet = new sPacket((int)ClientPackets.roundBegin))
        {

            packet.Write(Time.unscaledTime);
            ///other clients shud already have this, might be un-needed, if removed, remove on receieve too
            packet.Write(GameManager.Instance._roundDuration); 
            SendTCPData(packet);
        }
    }

    public void RoundEnded()
    {
        ///TODO see RoundBegin Comment about this:
        if (!sServer._iAmHost)
            return;

        Debug.Log($"<color=white>(ClientSend) Round Ended </color>");
        using (sPacket packet = new sPacket((int)ClientPackets.roundEnd))
        {
            ///Need to do time.unScaledTime - Time.(when we came in from the networking menu)
            packet.Write(Time.unscaledTime);
            SendTCPData(packet);
        }
    }



    /***Gameplay***/
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
    /// For now, this is only for tracking statistics across the network...Called from an InspectorEvent
    /// TODO: It would be really nice to encapsulate sending Items from this Batch, not the individual slots calling SendItem()
    /// however the way this was built with AutoSend and UIInventoryManager it would take some re-designing, plan to return to fix this
    /// </summary>
    /// <param name="batch"></param>
    public void BatchSent(BatchWrapper batch)
    {
        Debug.Log($"<color=yellow>Client Send BatchSent </color>{batch.StationId} , {batch.ItemCount}");

        using (sPacket packet = new sPacket((int)ClientPackets.batch))
        {
            packet.Write(batch.StationId);
            packet.Write(batch.ItemCount);
            packet.Write(batch.IsShipped);
            SendTCPData(packet);
        }
    }


    public void OrderCreated(OrderWrapper order)
    {
        Debug.Log($"<color=white>(ClientSend) Order Created</color>");
        using (sPacket packet = new sPacket((int)ClientPackets.orderCreated))
        {
            packet.Write(order.ItemID);
            packet.Write(order.CreatedTime);
            packet.Write(order.DueTime);
            SendTCPData(packet);
        }
    }

    public void DefectAdded(DefectWrapper defect)
    {
        Debug.Log($"<color=orange>(ClientSend) DefectAdded</color>");
        using (sPacket packet = new sPacket((int)ClientPackets.defectAdded))
        {
            packet.Write(defect.StationId);
            packet.Write(defect.ItemId);
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


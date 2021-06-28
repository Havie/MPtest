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
    /// <summary> We heard back from the server, so send along our Info
    /// </summary>
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
    public void SendWorkStationID(int stationID, Vector3 stationLocation)
    {
        using (sPacket packet = new sPacket((int)ClientPackets.stationID))
        {
            packet.Write(stationID);
            packet.Write(stationLocation);
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

        //Debug.Log($"<color=white>(ClientSend) Round Begin </color>");
        using (sPacket packet = new sPacket((int)ClientPackets.roundBegin))
        {
            ///Send an empty Packet and let the sServer set the time/duration there
            //packet.Write(Time.unscaledTime);
            //packet.Write(GameManager.Instance._roundDuration); 
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

    public void SendTransportData(int myStationID, int outputStationID, float distance)
    {
        using (sPacket packet = new sPacket((int)ClientPackets.receiveTransportData))
        {
            packet.Write(myStationID);
            packet.Write(outputStationID);
            packet.Write(distance); // distance between stations on client end
            SendTCPData(packet);
        }

    }
    /***Gameplay***/
    public void SendItem(int itemID, List<QualityData> qualities, bool isInInventory)
    {
        UIManager.DebugLog("(ClientSend): Sending Item on channel : " + (int)ClientPackets.item);
        using (sPacket packet = new sPacket((int)ClientPackets.item))
        {
            packet.Write(itemID);
            ///Who am I sending it to? (station/ClientID?)
            //packet.Write(toStationID);
            ///Am I the In out OUT inventory
            packet.Write(isInInventory);
            ///How many qualities to parse in a for loop
            packet.Write(qualities.Count);
            //Debug.Log($"ClientSend QualityCount={qualities.Count}");
            //string info = "";
            PackQualities(qualities, packet);

            //UIManager.DebugLog(info);

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
        //Debug.Log($"<color=yellow>Client Send BatchSent </color>{batch.StationId} , {batch.ItemCount}");
        var itemCount = batch.ItemCount;
        if (itemCount == -1)
            return; ///Fail safe so we can queue dummy events for kanban bin animations

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
        //Debug.Log($"<color=orange>(ClientSend) DefectAdded</color>");
        using (sPacket packet = new sPacket((int)ClientPackets.defectAdded))
        {
            packet.Write(defect.StationId);
            packet.Write(defect.ItemId);
            SendTCPData(packet);
        }
    }
    public void KanbanChanged(bool isInInventory, bool isRemoved, int itemID, List<QualityData> qualities)
    {
        //Debug.Log($"!!..<color=orange>(ClientSend) KanbanChanged</color>");
        using (sPacket packet = new sPacket((int)ClientPackets.inventoryChanged))
        {
            packet.Write(isInInventory);
            packet.Write(isRemoved);
            packet.Write(itemID);
            packet.Write(qualities.Count);
            PackQualities(qualities, packet);
            SendTCPData(packet);
        }
    }

    private static void PackQualities(List<QualityData> qualities, sPacket packet)
    {
        for (int i = 0; i < qualities.Count; ++i)
        {
            QualityData q = qualities[i];
            packet.Write(q.ID);
            packet.Write(q.Actions);
        }
    }
    #endregion

}


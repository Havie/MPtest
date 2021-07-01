
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoSingleton<ClientHandle>
{
    [Header("Events")]
    [SerializeField] OrderReceivedEvent _orderCreated;

    public void Welcome(sPacket packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        /// Read In Game Manager Vars
        Debug.Log($"!!...Message from server: { msg}");
        Client.instance._myId = myId;
        ClientSend.Instance.WelcomeReceived();

        var instance = GameManager.Instance;

        instance._orderFrequency = packet.ReadInt();
        instance.IsStackableChanged(packet.ReadBool()); ///needs to be before batchChanged
        instance.BatchChanged(packet.ReadInt());
        instance.AutoSendChanged(packet.ReadBool());
        instance._addChaotic = packet.ReadBool();
        instance._workStationArrangement = packet.ReadBool();
        instance._workStationTaskChanging = packet.ReadBool();
        instance._HUDManagement = packet.ReadBool();
        instance._HostDefectPausing = packet.ReadBool();
        instance.RoundDurationChanged(packet.ReadInt());

        UIManager.DebugLog("WE read GameManager VARS:");

        //give UDP the same port our tcp connection is using 
        Client.instance._udp.Connect(((IPEndPoint)Client.instance._tcp._socket.Client.LocalEndPoint).Port);
    }
    public void ReceivedMpData(sPacket packet)
    {
        //Debug.Log($"[ClientHandle]<color=green> Received refreshData From server</color>");
        List<LobbyPlayer> _players = new List<LobbyPlayer>();
        var count = packet.ReadInt();

        for (int i = 0; i < count; ++i)
        {
            int id = packet.ReadInt();
            string userName = packet.ReadString();
            int stationID = packet.ReadInt();
            bool isSelf = id == Client.instance._myId;
            _players.Add(new LobbyPlayer(id, userName, stationID, isSelf));
        }

        UIManagerNetwork.Instance.ReceieveMPData(_players);
    }
    public void RequestTransportData(sPacket packet)
    {
        var gm = GameManager.Instance;
        WorkStation ws = gm._workStation;
        if(ws == null)
        {
            ///Client didnt select a WorkStation
            ///send -1 so Server can figure it out,
            ///however server cant figure it out , because right now its "trusting" the client.
            ///Need to redo logic so its more server authorative , but too big of a task right now for V1.
            return;
        }
        //UIManager.DebugLog($"The ws at time of request is : {ws._myStation}");
        int myStation = (int)ws._myStation;
        int output = (int)ws._sendOutputToStation;
        Vector3 loc = ws.StationLocation;
        Vector3 endLoc = Vector3.zero;

        if(ws._sendOutputToStation != WorkStation.eStation.NONE)
            endLoc = gm.CurrentWorkStationManager.GetStationByEnumIndex(output).StationLocation;

        float distance = Mathf.Abs(Vector3.Distance(loc, endLoc));

        ClientSend.Instance.SendTransportData(myStation, output, distance);

    }

    public void RoundStarted(sPacket packet)
    {
        int roundDuration = packet.ReadInt(); ///I think the gameManager will already have this?
        UIManagerNetwork.Instance.ConfirmWorkStation();
    }

    public void RoundEnded(sPacket packet)
    {
        float cycleTime = packet.ReadFloat();
        float thruPut = packet.ReadFloat();
        int shippedOnTime = packet.ReadInt();
        int shippedLate = packet.ReadInt();
        int wip = packet.ReadInt();

        UIManagerGame.Instance.RoundOutOfTime(cycleTime, thruPut, shippedOnTime, shippedLate, wip);
    }

    public void ItemReceived(sPacket packet)
    {
        int itemID = packet.ReadInt();
        List<QualityData> qualities = ReadQualityData(packet);

        ///UNSURE IF I CAN DO UIMANAGER print logs in here, might be on wrong thread 
       // UIManager.DebugLog($"(ClientHandle):Item Received , item=<color=green>{itemLvl}</color>");

        //Tell the leftSide UI 
        UIManagerGame.Instance.ItemReceived(itemID, qualities);

    }

    private List<QualityData> ReadQualityData(sPacket packet)
    {
        List<QualityData> qualities = new List<QualityData>();
        var count = packet.ReadInt() / 2;  ///Divide by 2 because its (ID,CurrAction) per thing encoded
                                           ///Reconstruct the Object Quality data
        for (int i = 0; i < count; ++i)
        {
            var id = packet.ReadInt();
            var currQ = packet.ReadInt();
            //qualities.Add(ObjectManager.Instance.BuildTempQualities(id, currQ));
            qualities.Add(new QualityData(id, currQ));
            UIManager.DebugLog($"..Reconstructed {qualities[qualities.Count - 1]} with ({id} , {currQ})");
        }
        return qualities;
    }

    public void NewOrderCreated(sPacket packet)
    {
        Debug.Log($"..ClientHandle : NewOrderCreated");
        if (_orderCreated)
        {
            int itemID = -1; //TODO MOVE TO SERVER WHEN ADDING MORE THAN 1 ITEM
            float createTime = packet.ReadFloat();
            float dueTime = packet.ReadFloat();
            _orderCreated.Raise(new OrderWrapper(itemID, createTime, dueTime));
            Debug.Log($"invoked");
        }
    }
    public void OrderShipped(sPacket packet)
    {
        int itemID = packet.ReadInt();
        //Debug.Log($"[ClientHandle] itemIDShipped= {itemID}");
        ///Tell kitting menu or whoever else to remove order
        UIManagerGame.Instance.OrderShipped(itemID);
    }

    public void KanbanInventoryChanged(sPacket packet)
    {
        bool isInInventory = packet.ReadBool();
        bool isEmpty = packet.ReadBool();
        int itemID = packet.ReadInt();
        List<QualityData> qualities = ReadQualityData(packet);
        string inv = isInInventory ? "In" : "Out";
        UIManager.DebugLog($"My Kanban {inv}::Inventory changed !  {isInInventory}  , {isEmpty}");
        ///Tell someone to add the slot but not recall the server
        UIManagerGame.instance.KanbanUpdateInventory(isInInventory, isEmpty, itemID, qualities);
    }
}


using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoSingleton<ClientHandle>
{
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

        for(int i=0; i<count; ++i)
        {
            int id = packet.ReadInt();
            string userName = packet.ReadString();
            int stationID = packet.ReadInt();
            bool isSelf = id == Client.instance._myId;
            _players.Add(new LobbyPlayer(id, userName, stationID, isSelf));
        }

        UIManagerNetwork.Instance.ReceieveMPData(_players);
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
        int itemLvl = packet.ReadInt(); //get rid of the first btye data?

        List<QualityObject> qualities = new List<QualityObject>();

        var count = packet.ReadInt() / 2;  ///Divide by 2 because its (ID,CurrAction) per thing encoded
        UIManager.DebugLog($"ClientHandle Count={count}");

        ///Reconstruct the Object Quality data
        for (int i = 0; i < count; ++i)
        {
            var id = packet.ReadInt();
            var currQ = packet.ReadInt();
            qualities.Add(ObjectManager.Instance.BuildTempQualities(id, currQ));
            Debug.Log($"..Reconstructed {qualities[qualities.Count - 1]} with ({id} , {currQ})");
        }


        ///UNSURE IF I CAN DO UIMANAGER print logs in here, might be on wrong thread 
        UIManager.DebugLog($"(ClientHandle):Item Received , item=<color=green>{itemLvl}</color>");

        //Tell the leftSide UI 
        UIManagerGame.Instance._invIN.AddItemToSlot(itemLvl, qualities, false);

    }

    public void OrderShipped(sPacket packet)
    {

        int itemID = packet.ReadInt();
        Debug.Log($"[ClientHandle] itemIDShipped= {itemID}");
        ///Tell kitting menu or whoever else to remove order
        UIManagerGame.Instance.OrderShipped(itemID);
    }

    #region OldTutorial
    //public  void SpawnPlayer(sPacket packet)
    //{
    //    int id = packet.ReadInt();
    //    string username = packet.ReadString();
    //    Vector3 pos = packet.ReadVector3();
    //    Quaternion rot = packet.ReadQuaternion();

    //    //GameManager.instance.SpawnPlayer(id, username, pos, rot);
    //}

    //public  void PlayerPosition(sPacket packet)
    //{
    //    int id = packet.ReadInt();
    //    Vector3 position = packet.ReadVector3();

    //    //if (GameManager._players.TryGetValue(id, out PlayerManager pm))
    //    {
    //        //   pm.transform.position = position;
    //    }
    //}

    //public  void PlayerRotation(sPacket packet)
    //{
    //    int id = packet.ReadInt();
    //    Quaternion rotation = packet.ReadQuaternion();


    //    // if(GameManager._players.TryGetValue(id, out PlayerManager pm ))
    //    {
    //        //  pm.transform.rotation = rotation;
    //    }

    //}


    #endregion

}

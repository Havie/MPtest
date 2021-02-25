﻿
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoSingleton<ClientHandle>
{
    public  void Welcome(sPacket packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        ///TODO Read In Game Manager Vars
        Debug.Log($"Message from server: { msg}");
        Client.instance._myId = myId;
        ClientSend.Instance.WelcomeReceived();

        var instance = GameManager.Instance;

       instance._orderFrequency = packet.ReadInt();
       instance.BatchChanged(packet.ReadInt());
       instance.AutoSendChanged(packet.ReadBool());
       instance._addChaotic = packet.ReadBool();
       instance.IsStackableChanged(packet.ReadBool());
       instance._workStationArrangement = packet.ReadBool();
       instance._workStationTaskChanging = packet.ReadBool();
       instance._HUDManagement = packet.ReadBool();
       instance._HostDefectPausing = packet.ReadBool();

        UIManager.DebugLog("WE read GameManager VARS:");

        //give UDP the same port our tcp connection is using 
        Client.instance._udp.Connect(((IPEndPoint)Client.instance._tcp._socket.Client.LocalEndPoint).Port);
    }



    public void ItemReceived(sPacket packet)
    {
        int itemLvl = packet.ReadInt(); //get rid of the first btye data?

        List<QualityObject> qualities = new List<QualityObject>();

        var count = packet.ReadInt()/2;  ///Divide by 2 because its (ID,CurrAction) per thing encoded
        UIManager.DebugLog($"ClientHandle Count={count}");

        ///Reconstruct the Object Quality data
        for (int i = 0; i < count; ++i)
        {
            var id = packet.ReadInt();
            var currQ = packet.ReadInt();
            qualities.Add(BuildableObject.Instance.BuildTempQualities(id, currQ));
            Debug.Log($"..Reconstructed {qualities[qualities.Count - 1]} with ({id} , {currQ})");
        }


        ///UNSURE IF I CAN DO UIMANAGER print logs in here, might be on wrong thread 
        UIManager.DebugLog($"(ClientHandle):Item Received , item=<color=green>{itemLvl}</color>");

        //Tell the leftSide UI 
        GameManager.Instance._invIN.AddItemToSlot(itemLvl, qualities,  false);

    }


    #region OldTutorial
    public  void SpawnPlayer(sPacket packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 pos = packet.ReadVector3();
        Quaternion rot = packet.ReadQuaternion();

        //GameManager.instance.SpawnPlayer(id, username, pos, rot);
    }

    public  void PlayerPosition(sPacket packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        //if (GameManager._players.TryGetValue(id, out PlayerManager pm))
        {
            //   pm.transform.position = position;
        }
    }

    public  void PlayerRotation(sPacket packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();


        // if(GameManager._players.TryGetValue(id, out PlayerManager pm ))
        {
            //  pm.transform.rotation = rotation;
        }

    }


    #endregion

}

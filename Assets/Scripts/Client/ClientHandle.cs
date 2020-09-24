
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(sPacket packet)
    {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log($"Message from server: { msg}");
        Client.instance._myId = myId;
        ClientSend.WelcomeReceived();
        //give UDP the same port our tcp connection is using 
        Client.instance._udp.Connect(((IPEndPoint)Client.instance._tcp._socket.Client.LocalEndPoint).Port);
    }

    public static void SpawnPlayer(sPacket packet)
    {
        int id = packet.ReadInt();
        string username = packet.ReadString();
        Vector3 pos = packet.ReadVector3();
        Quaternion rot = packet.ReadQuaternion();

        GameManager.instance.SpawnPlayer(id, username, pos, rot);
    }

    public static void PlayerPosition(sPacket packet)
    {
        int id = packet.ReadInt();
        Vector3 position = packet.ReadVector3();

        if (GameManager._players.TryGetValue(id, out PlayerManager pm))
        {
            pm.transform.position = position;
        }
    }

    public static void PlayerRotation(sPacket packet)
    {
        int id = packet.ReadInt();
        Quaternion rotation = packet.ReadQuaternion();


        if(GameManager._players.TryGetValue(id, out PlayerManager pm ))
        {
            pm.transform.rotation = rotation;
        }

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sServerHandle
{

    public static void WelcomeReceived(int fromClient, sPacket packet)
    {
        int clientIdCheck = packet.ReadInt();
        string username = packet.ReadString(); //error here


        Debug.Log($"{sServer._clients[fromClient]._tcp._socket.Client.RemoteEndPoint} connected successfully and is now player {fromClient}");
        if (fromClient != clientIdCheck)
        {
            Debug.Log($"Player \"{username}\" (ID: {fromClient}) has assumed the wrong client ID ({clientIdCheck})!");
        }


        // send player into game 

        if (sServer._clients[fromClient] != null)
            sServer._clients[fromClient].SendIntoGame(username);
        else
            Debug.Log("Found an error");

    }
    public static void PlayerMovement(int fromClient, sPacket packet)
    {
        bool[] inputs = new bool[packet.ReadInt()];
        for (int i = 0; i < inputs.Length; ++i)
            inputs[i] = packet.ReadBool();

        Quaternion rotation = packet.ReadQuaternion();


        sClient client = sServer._clients[fromClient];
        if(client!=null)
        {
            if(client._player!=null)
                client._player.SetInput(inputs, rotation);
        }
       // sServer._clients[fromClient]._player.SetInput(inputs, rotation);
    }

}

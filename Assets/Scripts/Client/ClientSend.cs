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

            SendTCPData(packet);
        }
    }

    public static void PlayerMovement(bool[] inputs)
    {
        using (sPacket packet = new sPacket((int)ClientPackets.playerMovement))
        {
            packet.Write(inputs.Length);
            foreach (bool input in inputs)
                packet.Write(input);

            //use UDP cause we can afford to lose data (faster)
            packet.Write(GameManager._players[Client.instance._myId].transform.rotation);

            SendUDPData(packet);
        }
    }
    #endregion
}


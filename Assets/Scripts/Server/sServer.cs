using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Net.NetworkInformation;
using System.Text;

public static class sServer
{
    public static int _maxPlayers { get; private set; }
    public static int _port { get; private set; }

    private static TcpListener _tcpListener;
    private static UdpClient _udpListener;

    public static Dictionary<int, sClient> _clients = new Dictionary<int, sClient>();


    public delegate void PacketHandler(int fromClient, sPacket packet);
    public static Dictionary<int, PacketHandler> _packetHandlers;


    public static IPEndPoint _broadcastAddress;
    public static bool _iAmHost;
    public static event Action<string> OnHostIpFound = delegate { };

    ///Doesnt work for some reason, says sockets not connected...
    #region BroadCastListener  
    /*
    public static event Action<string> OnHostIpFound = delegate { };

    public static void ListenForHostBroadCasts()
    {
        _udpListener = new UdpClient(_port);
        _udpListener.BeginReceive(UDPReceiveCallBack, null);

        UIManager.instance.DebugLog("Server..listening for hostIP");
    }

    public static void BroadCastIP()
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            var data = Encoding.UTF8.GetBytes(sServer.GetLocalIPAddress());
            _udpListener.Send(data, data.Length, "255.255.255.255", _port);
            
        });

        _iAmHost = true;
    }
    */
    #endregion





    #region Live Host
    public static void Start(int maxPlayers, int port)
    {
        _maxPlayers = maxPlayers;
        _port = port;

        UIManager.instance.DebugLog("Starting Server..");
        InitServerData();

        _tcpListener = new TcpListener(IPAddress.Any, _port);
        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);


        BroadcastListener.Instance.BroadCastIP();
        BroadcastListener.Instance.SetAsHost(); ///will start to forward all info to us now
        UIManager.instance.DisableHostButton("");
        ///Use the _udp from the broadcaster now 
        //_udpListener = new UdpClient(_port);
        //_udpListener.BeginReceive(UDPReceiveCallBack, null);


        UIManager.instance.DebugLog($"Server started on <color=blue>{_port}</color> , GetLocalIPAddress: <color=green>{GetLocalIPAddress()}</color>");

    }

    private static void Two()
    {
        // Get server related information.
        IPHostEntry heserver = Dns.Resolve(GetLocalIPAddress());

        foreach (IPAddress curAdd in heserver.AddressList)
        {

            UIManager.instance.DebugLog($"Server started on <color=blue>{_port}</color>. IPAddress={curAdd} vs GetLocalIPAddress: <color=green>{GetLocalIPAddress()}</color>");
        }
    }

    private static void One()
    {
        foreach (var netInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            //if (netInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 ||
            // netInterface.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            {
                foreach (var addrInfo in netInterface.GetIPProperties().UnicastAddresses)
                {
                    if (addrInfo.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        var ipAddress = addrInfo.Address;

                        UIManager.instance.DebugLog($"Server started on <color=blue>{_port}</color>. IPAddress={ipAddress} vs GetLocalIPAddress: <color=green>{GetLocalIPAddress()}</color>");

                    }
                }
            }
        }
    }


    private static void TCPConnectCallback(IAsyncResult result)
    {
        TcpClient client = _tcpListener.EndAcceptTcpClient(result);
        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        UIManager.instance.DebugLog($"Incoming connection from {client.Client.RemoteEndPoint}...");

        for (int i = 1; i <= _maxPlayers; ++i)
        {
            if (_clients[i]._tcp._socket == null)
            {
                _clients[i]._tcp.Connect(client);
                return;
            }
        }
        UIManager.instance.DebugLog($"{client.Client.RemoteEndPoint} failed to connect: Server Full!");
    }

    public static void PrepareForHorribleForwardingStrategy(UdpClient udp)
    {
        _udpListener = udp;
    }


    public static void UDPReceiveCallBack(IAsyncResult result)
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            try
            {

                //UIManager.instance.DebugLog($"<color=purple>UDPReceiveCallBack</color> from Server:");


                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpListener.EndReceive(result, ref clientEndPoint);//set our endpoint to where the data came from 
                _udpListener.BeginReceive(UDPReceiveCallBack, null);

                if (data.Length < 4)
                {
                    return;
                }

                string receiveString = System.Text.Encoding.ASCII.GetString(data);
                if (LookLikeIpAddress(receiveString))
                {

                    UIManager.instance.DebugLogWarning($"Server Received HostIP: {receiveString}");
                   OnHostIpFound(receiveString);



                    return;
                }

                using (sPacket packet = new sPacket(data))
                {
                    int clientId = packet.ReadInt();
                    //should never happen but cud crash server becuz dictonary starts at 1
                    if (clientId == 0)
                        return;

                    if (_clients.ContainsKey(clientId))
                    {
                        if (_clients[clientId]._udp._endPoint == null)
                        {
                            _clients[clientId]._udp.Connect(clientEndPoint); //first time through?
                            return;
                        }

                        //Prevent hacker from impersonating someone by sending a different ID
                        //Convert to string because even when they match returns false?
                        // Debug.Log("Test val1: " + _clients[clientId]._udp._endPoint + " , val2: " + clientEndPoint + " comparison= " + (_clients[clientId]._udp._endPoint == clientEndPoint));
                        if (_clients[clientId]._udp._endPoint.ToString().Equals(clientEndPoint.ToString()))
                        {
                            _clients[clientId]._udp.HandleData(packet);
                        }
                    }
                    else
                    {
                        Debug.LogWarning(clientId + " Does not exist in _clients , might be okay if sending broadvcast");
                        //byte[] receiveBytes = _udpListener.EndReceive(result, ref _broadcastAddress);
                        // string receiveString = System.Text.Encoding.ASCII.GetString(receiveBytes);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log($"Error receiving UDP data : {e}");
            }

        });
    }

    private static bool LookLikeIpAddress(string s)
    {
        int count = 0;
        for (int i = 0; i < s.Length - 1; i++)
        {
            var c = s[i];
            if (c.Equals('.'))
                ++count;
        }
        return count > 2;
    }

    public static void SendUDPData(IPEndPoint clientEndPoint, sPacket packet)
    {
        try
        {
            if (clientEndPoint != null)
            {
                _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
            }
        }
        catch (Exception e)
        {
            Debug.Log($"Error Sending UDP data to :{clientEndPoint}  exception: {e}");
        }

    }

    private static void InitServerData()
    {

        for (int i = 1; i <= _maxPlayers; ++i)
        {
            _clients.Add(i, new sClient(i));
        }

        _packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived , sServerHandle.WelcomeReceived},
                { (int)ClientPackets.playerMovement , sServerHandle.PlayerMovement},
                { (int)ClientPackets.stationID , sServerHandle.StationIDReceived},
                { (int)ClientPackets.item , sServerHandle.ItemReceived}

            };

        UIManager.instance.DebugLog("Initilalized Packets.");
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("No network adapters with an IPv4 address in the system!");
    }

    #endregion
}


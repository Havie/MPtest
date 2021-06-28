using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using System.Linq;

public static class sServer
{
    public static int _maxPlayers { get; private set; }
    public static int _port { get; private set; }

    private static TcpListener _tcpListener;
    private static UdpClient _udpListener;

    public static Dictionary<int, sClient> _clients = new Dictionary<int, sClient>();

    public delegate void PacketHandler(int fromClient, sPacket packet);
    public static Dictionary<int, PacketHandler> _packetHandlers;

    public static event Action<string> OnHostIpFound = delegate { };
    public static bool _iAmHost;
    ///Not loving all the public stuff , but the network code is written in this messy way everywhere so
    public static sGameStatistics _gameStatistics;
    public static sSharedInventories _sharedInventories;
    public static sOrderManager _orderManager;
    public static int BatchSize { get; private set; }


    /************************************************************************************************************************/
    public static void Tick()
    {
        if (_orderManager != null)
        {
            _orderManager.Tick();
        }
    }

    /************************************************************************************************************************/

    public static void ListenForHostBroadCasts()
    {
        _udpListener = new UdpClient(sNetworkManager._defaultPort);
        _udpListener.EnableBroadcast = true;
        _udpListener.BeginReceive(UDPReceiveCallBack, null);
        UIManager.DebugLog("Server..listening for hostIP");
    }
    public static void BroadCastIP()
    {
        var data = Encoding.UTF8.GetBytes(GetLocalIPAddress());
        _udpListener.Send(data, data.Length, "255.255.255.255", sNetworkManager._defaultPort);

    }

    public static void Start(int maxPlayers, int port)
    {
        _maxPlayers = maxPlayers;
        _port = port;

        //Debug.Log("Starting Server..");
        InitServerData();

        _tcpListener = new TcpListener(IPAddress.Any, _port);
        _tcpListener.Start();
        _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
        /// send out a message so other tablets listening can pick up our IP
        BroadCastIP();
        _iAmHost = true;

        ///Init our internal game classes
        ResetStatistics();
        ResetSharedInventories();
        ResetOrderManager();

        //_udpListener = new UdpClient(_port);
        // _udpListener.BeginReceive(UDPReceiveCallBack, null);

        //UIManager.DebugLog($"Server started on IP:<color=green>{GetLocalIPAddress()} </color> Port:<color=blue> {_port}. </color>");
    }

    public static void ResetStatistics()
    {
        _gameStatistics = new sGameStatistics();
    }
    public static void ResetSharedInventories()
    {
        _sharedInventories = new sSharedInventories();
    }
    public static void ResetOrderManager()
    {
        var gm = GameManager.Instance;
        _orderManager = new sOrderManager(gm._orderFrequency, gm.ExpectedDeliveryDelay);
    }

    ///Note: I think this method is Asynchronous which means it will be run on a different thread, so 
    ///      game logic like UIManager.DebugLog is not safe (and will sometimes crash without error and mess other things up)
    private static void TCPConnectCallback(IAsyncResult result)
    {
        try
        {
            TcpClient client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Debug.Log($"Incoming connection from <color=green>{client.Client.RemoteEndPoint}</color>");
            ///WARNING TRYING TO PRINT THIS TO THE WINDOW CRASHES WITH NO WARNINGS 
            //UIManager.DebugLog($"Incoming connection from <color=green>{client.Client.RemoteEndPoint}</color> .");

            for (int i = 1; i <= _maxPlayers; ++i)
            {
                if (_clients[i].Tcp._socket == null)
                {
                    _clients[i].Tcp.Connect(client);
                    return;
                }
            }
            Debug.Log($"{client.Client.RemoteEndPoint} failed to connect: Server Full!");
        }
        catch (Exception e)
        {
            Debug.Log($"Error TCPConnectCallback : {e}");
        }
    }

    private static void UDPReceiveCallBack(IAsyncResult result)
    {
        try
        {
            IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = _udpListener.EndReceive(result, ref clientEndPoint);//set our endpoint to where the data came from 
            _udpListener.BeginReceive(UDPReceiveCallBack, null);

            if (data.Length < 4)
            {
                return;
            }

            string receiveString = Encoding.ASCII.GetString(data);
            if (LookLikeIpAddress(receiveString))
            {
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    UIManager.DebugLogWarning($"Server Received a string IP: {receiveString}");
                    OnHostIpFound(receiveString);
                });

                return;
            }
            if (!_iAmHost) /// we dont want to process data , we might even want to close our UDP here but idk how without receiving errors
            {
                //_udpListener.Close(); ///Results in Error:CanNot access disposed of object
                Debug.Log("<color=yellow>This connection is open</color>");
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
                    if (_clients[clientId].Udp._endPoint == null)
                    {
                        _clients[clientId].Udp.Connect(clientEndPoint); //first time through?
                        return;
                    }

                    //Prevent hacker from impersonating someone by sending a different ID
                    //Convert to string because even when they match returns false?
                    // Debug.Log("Test val1: " + _clients[clientId]._udp._endPoint + " , val2: " + clientEndPoint + " comparison= " + (_clients[clientId]._udp._endPoint == clientEndPoint));
                    if (_clients[clientId].Udp._endPoint.ToString().Equals(clientEndPoint.ToString()))
                    {
                        _clients[clientId].Udp.HandleData(packet);
                    }
                }
                else
                    Debug.LogError(clientId + " Does not exist in _clients");
            }
        }
        catch (Exception e)
        {
            ///Cant debug to UI on this thread
            Debug.LogError($"Error receiving UDP data : {e}");
        }
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
            ///Cant debug to UI on this thread
            Debug.LogError($"Error Sending UDP data to :{clientEndPoint}  exception: {e}");
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
                //{ (int)ClientPackets.playerMovement , sServerHandle.PlayerMovement},
                { (int)ClientPackets.stationID , sServerHandle.StationInfoReceived},
                { (int)ClientPackets.requestMpData , sServerHandle.RequestMultiPlayerData},
                { (int)ClientPackets.item , sServerHandle.ItemReceived},
                { (int)ClientPackets.batch , sServerHandle.BatchReceived},
                { (int)ClientPackets.orderCreated , sServerHandle.OrderCreated},
                { (int)ClientPackets.defectAdded , sServerHandle.DefectAdded},
                { (int)ClientPackets.roundBegin , sServerHandle.RoundBegin},
                { (int)ClientPackets.roundEnd , sServerHandle.RoundEnded},
                { (int)ClientPackets.receiveTransportData , sServerHandle.ReceivedTransportData},
                { (int)ClientPackets.inventoryChanged , sServerHandle.InventoryChanged}

            };

        Debug.Log("Initilalized Packets.");
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

    /************************************************************************************************************/
    //   My attempt at starting to rewrite this to following better programming principles
    //   Theres far too many public accessors to what should be private data
    /************************************************************************************************************/

    public static List<sClient> GetClients()
    {
        return _clients.Values.ToList();
    }
}


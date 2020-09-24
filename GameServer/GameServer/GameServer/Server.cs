using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    class Server
    {
        public static int _maxPlayers { get; private set; }
        public static int _port { get; private set; }

        private static TcpListener _tcpListener;
        private static UdpClient _udpListener;

        public static Dictionary<int, Client> _clients = new Dictionary<int, Client>();

        public delegate void PacketHandler(int fromClient, Packet packet);
        public static Dictionary<int, PacketHandler> _packetHandlers; 

        public static void Start(int maxPlayers, int port)
        {
            _maxPlayers = maxPlayers;
            _port = port;

            Console.WriteLine("Starting Server..");
            InitServerData();

            _tcpListener = new TcpListener(IPAddress.Any, _port);
            _tcpListener.Start();
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            _udpListener = new UdpClient(_port);
            _udpListener.BeginReceive(UDPReceiveCallBack, null);

            Console.WriteLine($"Server started on {_port}.");
        }

        private static void TCPConnectCallback(IAsyncResult result)
        {
            TcpClient client = _tcpListener.EndAcceptTcpClient(result);
            _tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);
            Console.WriteLine($"Incoming connection from {client.Client.RemoteEndPoint}...");

             for (int i = 1; i <= _maxPlayers; ++i)
            {
                if(_clients[i]._tcp._socket==null)
                {
                    _clients[i]._tcp.Connect(client);
                    return;
                }
            }
            Console.WriteLine($"{client.Client.RemoteEndPoint} failed to connect: Server Full!");
        }

        private static void UDPReceiveCallBack(IAsyncResult result)
        {
            try
            {
                IPEndPoint clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = _udpListener.EndReceive(result, ref clientEndPoint);//set our endpoint to where the data came from 
                _udpListener.BeginReceive(UDPReceiveCallBack, null);

                if(data.Length <4)
                {
                    return;
                }
                using (Packet packet = new Packet(data))
                {
                    int clientId = packet.ReadInt();
                    //should never happen but cud crash server becuz dictonary starts at 1
                    if (clientId == 0)
                       return;
                    if(_clients[clientId]._udp._endPoint ==null )
                    {
                        _clients[clientId]._udp.Connect(clientEndPoint); //first time through?
                        return;
                    }

                    //Prevent hacker from impersonating someone by sending a different ID
                    //Convert to string because even when they match returns false?
                   // Console.WriteLine("Test val1: " + _clients[clientId]._udp._endPoint + " , val2: " + clientEndPoint + " comparison= " + (_clients[clientId]._udp._endPoint == clientEndPoint));
                    if (_clients[clientId]._udp._endPoint.ToString().Equals(clientEndPoint.ToString()))
                    {
                        _clients[clientId]._udp.HandleData(packet);
                    }
                    
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error receiving UDP data : {e}");
            }
        }

        public static void SendUDPData(IPEndPoint clientEndPoint, Packet packet)
        {
            try
            {
                if(clientEndPoint != null)
                {
                    _udpListener.BeginSend(packet.ToArray(), packet.Length(), clientEndPoint, null, null);
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"Error Sending UDP data to :{clientEndPoint}  exception: {e}");
            }

        }

        private static void InitServerData()
        {

            for (int i = 1; i <= _maxPlayers; ++i)
            {
                _clients.Add(i, new Client(i));
            }

            _packetHandlers = new Dictionary<int, PacketHandler>()
            {
                { (int)ClientPackets.welcomeReceived , ServerHandle.WelcomeReceived},
            
                { (int)ClientPackets.playerMovement , ServerHandle.PlayerMovement}

            };
            Console.WriteLine("Initliazed Packets.");
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;

public class Client : MonoSingleton<Client>
{

    public static int _dataBufferSize = 4096;

    private string _ip =  "127.0.0.1"; // local host  //"192.168.1.19"
    private int _port = 0000; //Match server
    public int _myId = 0;
    public TCP _tcp;
    public UDP _udp;

    private bool _isConnected;

    private delegate void PacketHandler(sPacket packet);
    private static Dictionary<int, PacketHandler> _packetHandlers;

    public bool IWillBeHost =false;

    private void OnApplicationQuit()
    {
        Disconnect();
    }


    private void Start()
    {
        _ip = sServer.GetLocalIPAddress(); ///cache this here because we might be the host and the UDP callback takes too long on tablets and 127 local host doesnt work for them? idk why
        sServer.ListenForHostBroadCasts();
        sServer.OnHostIpFound += UpdateHostIP;
        _port = sNetworkManager._defaultPort;
        _tcp = new TCP();
        _udp = new UDP();
    }


    private void UpdateHostIP(string address)
    {
        _ip = address;
        UIManager.DebugLog("<color=purple>Client received broadcast </color> for new host address" + address);
        ///As Soon as we hear about the first host, Stop caring. (Might have to change later if we swap things, or host DC's)
        sServer.OnHostIpFound -= UpdateHostIP;
    }


    public void ConnectToServer()
    {
        if (IWillBeHost)
            ConnectionDelay();
        else
            TryToConnect();
    }

    private void TryToConnect()
    {
        InitClientData();
        _tcp.Connect(this);
        //Figure out if the connection succeeded or not 
        ThreadManager.ExecuteOnMainThread(() =>
        {
            StartCoroutine(ConnectionCheck(1));
        });
    }

    private void ConnectionDelay()
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            StartCoroutine(WaitForServerToStart(1000)); ///like ~5 seconds
        });
    }

    IEnumerator WaitForServerToStart(int timeOutCount)
    {
        bool keepTrying = true;

        while (keepTrying)
        {
            if (sServer._iAmHost)
                keepTrying = false;
            else if (timeOutCount <= 0)
                keepTrying = false;
            else
                --timeOutCount;
            
            yield return new WaitForEndOfFrame();

        }
        TryToConnect();
    }
    IEnumerator ConnectionCheck(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        //Debug.LogWarning($"Connection comparison <color=green>{_isConnected}</color>  <color=blue>{_tcp._socket.Connected}</color>");
        _isConnected = _tcp._socket.Connected;
        UIManagerNetwork.Instance.Connected(_isConnected);
    }

    public class TCP
    {
        public TcpClient _socket;
        private NetworkStream _stream;
        private sPacket _receivedData; 
        private byte[] _receivedBuffer;
        private Client _client;

        public void Connect(Client client)
        {
            _client = client;
            _socket = new TcpClient
            {
                ReceiveBufferSize = _dataBufferSize,
                SendBufferSize = _dataBufferSize
            };

            //= how this constructor works , it just does this somehow
            /*  _socket.ReceiveBufferSize = _dataBufferSize;
            _socket.SendBufferSize = _dataBufferSize;*/

            _receivedBuffer = new byte[_dataBufferSize];
            _socket.BeginConnect(instance._ip, instance._port, ConnectCallback, _socket);
            UIManager.DebugLog($"trying to connect to..<color=orange>{instance._ip} </color> port: <color=orange>{instance._port}</color>");

        }

      
        private void ConnectCallback(IAsyncResult result)
        {

            _socket.EndConnect(result);
            _client._isConnected = _socket.Connected;
            if(!_socket.Connected)
            {
                return;
            }

            _stream = _socket.GetStream();

            _receivedData = new sPacket();

            _stream.BeginRead(_receivedBuffer, 0, _dataBufferSize, ReceiveCallBack, null) ;
        }

        public void SendData(sPacket packet)
        {
            try
            {
                if(_socket!=null)
                {
                    _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch(Exception e)
            {
                Debug.LogError("Error in SendData" + e);
            }
        }

        private void ReceiveCallBack(IAsyncResult result)
        {
            try
            {
                int byteLength = _stream.EndRead(result);
                if(byteLength <=0)
                {
                    instance.Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(_receivedBuffer, data, byteLength);

                _receivedData.Reset(HandleData(data));
                _stream.BeginRead(_receivedBuffer, 0, _dataBufferSize, ReceiveCallBack, null);
            }
            catch
            {
                Disconnect();
            }
        }

        private bool HandleData(byte[] data)
        {
            int packetLength = 0;

            _receivedData.SetBytes(data);

            //If true we have the start of one of our packets 
            if(_receivedData.UnreadLength() >= 4) //int
            {
                packetLength = _receivedData.ReadInt();
                if (packetLength <= 0)
                    return true;
            }

            while(packetLength >0 && packetLength <= _receivedData.UnreadLength())
            {
                byte[] packetBytes = _receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (sPacket packet = new sPacket(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        _packetHandlers[packetId](packet); // Invoke delegate 
                    }
                });

                packetLength = 0;
                if (_receivedData.UnreadLength() >= 4) //int
                {
                    packetLength = _receivedData.ReadInt();
                    if (packetLength <= 0)
                        return true;
                }

                if (packetLength <= 1)
                    return true;
            }

        return false;
        }

        private void Disconnect()
        {
            instance.Disconnect();
            _stream = null;
            _receivedBuffer = null;
            _receivedData = null;
            _socket = null;
        }
    }
    

    public class UDP
    {
        public UdpClient _socket;
        public IPEndPoint _endPoint;


        public UDP()
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(instance._ip), instance._port);
        }

        public void Connect(int localPoint)
        {
            _socket = new UdpClient(localPoint);
            _socket.Connect(_endPoint);
            _socket.BeginReceive(ReceiveCallBack, null);

            //Create new packet to open port and begin connection so client cant received messages
            using (sPacket packet = new sPacket())
            {
                SendData(packet); //this method writes the ID to the packet 
            };
        }

        public void SendData(sPacket packet)
        {
            try
            {
                packet.InsertInt(instance._myId); // insert id to packet , use val on server to determine who sent it due to not being able to give clients unique ids on server side due to ports being closed? v3 4:00min
                if(_socket!=null )
                {
                    _socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch(Exception e)
            {
                Debug.Log("Error SendData in UDP : " + e);
            }
        }

        private void ReceiveCallBack(IAsyncResult result)
        {
            try
            {
                byte[] data = _socket.EndReceive(result, ref _endPoint);
                _socket.BeginReceive(ReceiveCallBack, null);

                //Could happen if partial packet is lost 
                if(data.Length<4)
                {
                    instance.Disconnect();
                    return;
                }

                HandleData(data);
            }
            catch (Exception e)
            {
                Debug.Log("UDP error: " + e); //Why does this keep running despite being DC'd . reenable debug log to see (close game server first)
                Disconnect();
            }
        }

        private void HandleData(byte[] data)
        {
            using (sPacket packet = new sPacket(data))
            {
                int packetLength = packet.ReadInt();
                data = packet.ReadBytes(packetLength); // removes initial 4 byes that tell length , not sure why 
            }
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (sPacket packet = new sPacket(data))
                {
                    int packetId = packet.ReadInt();
                    Client.PacketHandler _delegate;
                    if (_packetHandlers.TryGetValue(packetId, out _delegate))
                    {
                        _delegate(packet); //Invoke
                    }
                }
            }
            );
        }

        private void Disconnect()
        {
            instance.Disconnect();
            _endPoint = null;
            _socket = null;
        }
    }

    private void InitClientData()
    {
        //This is for data coming IN
        _packetHandlers = new Dictionary<int, PacketHandler>()
        {
            //Setup the <index, Delegate> 
            { (int)ServerPackets.welcome, ClientHandle.Instance.Welcome } ,
            { (int)ServerPackets.changedGMValues, ClientHandle.Instance.UpdateGameManagerVars } ,
            { (int)ServerPackets.sendMpData, ClientHandle.Instance.ReceivedMpData } ,
            //{ (int)ServerPackets.spawnPlayer, ClientHandle.Instance.SpawnPlayer }, //oldTutorial
            //{ (int)ServerPackets.playerPosition, ClientHandle.Instance.PlayerPosition }, //oldTutorial
            //{ (int)ServerPackets.playerRotation, ClientHandle.Instance.PlayerRotation }, //oldTutorial
            { (int)ServerPackets.item, ClientHandle.Instance.ItemReceived },
            { (int)ServerPackets.roundStart, ClientHandle.Instance.RoundStarted },
            { (int)ServerPackets.roundEnd, ClientHandle.Instance.RoundEnded },
            { (int)ServerPackets.orderShipped, ClientHandle.Instance.OrderShipped },
            { (int)ServerPackets.requestTransportData, ClientHandle.Instance.RequestTransportData },
            { (int)ServerPackets.sharedInventoryChanged, ClientHandle.Instance.KanbanInventoryChanged },
            { (int)ServerPackets.newOrderCreated, ClientHandle.Instance.NewOrderCreated }
        };

       // Debug.Log("InitClientData packets ");
    }

    public void Disconnect()
    {
        if (_isConnected)
        {
            _isConnected = false;
            _tcp._socket.Close();
            _udp._socket.Close();

            Debug.Log("Disconnected from server");
        }
    }


}

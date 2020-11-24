using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;


public class Client : MonoBehaviour
{
    public static Client instance;
    public static int _dataBufferSize = 4096;

    private string _ip = "192.168.1.19";  //"127.0.0.1"; // local host
    private int _port = 26951; //Match server  ///shud just take this from the serverClass on Awake
    public int _myId = 0;
    public TCP _tcp;
    public UDP _udp;

    public UdpClient _listener;
    public IPEndPoint _broadcastAddress;

    private bool _isConnected;

    private delegate void PacketHandler(sPacket packet);
    private static Dictionary<int, PacketHandler> _packetHandlers;

    private void OnApplicationQuit()
    {
        Disconnect();
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
        {
            Debug.LogWarning("Duplicate Clients, destroying");
            Destroy(this);
        }
    }



    private void Start()
    {
        _tcp = new TCP();
        _udp = new UDP();

       BroadcastListener.Instance.OnHostIpFound += UpdateHostIP;
       sServer.OnHostIpFound += UpdateHostIP;

        UIManager.instance.DebugLog("Client..listening for a hostIP");
    }

    private void UpdateHostIP(string address)
    {
        _ip = address;
        UIManager.instance.DebugLog("<color=purple>Client received broadcast </color> for new host address" + address);
        ///As Soon as we hear about the first host, Stop caring. (Might have to change later if we swap things, or host DC's)
        BroadcastListener.Instance.OnHostIpFound -= UpdateHostIP;
        sServer.OnHostIpFound -= UpdateHostIP;
    }

    public void ConnectToServer()
    {
        InitClientData();
        UIManager.instance.DebugLog("Tellng the tcp to connect:");
        _tcp.Connect();
        //Figure out if the connection succeeded or not 
        ThreadManager.ExecuteOnMainThread(() =>
        {
            StartCoroutine(ConnectionCheck(1));
        });
    }

    IEnumerator ConnectionCheck(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        _isConnected = _tcp._socket.Connected;
        UIManager.instance.Connected(_isConnected);
    }

    public class TCP
    {
        public TcpClient _socket;
        private NetworkStream _stream;
        private sPacket _receivedData;
        private byte[] _receivedBuffer;
        public bool _connectionRan;

        public void Connect()
        {
            UIManager.instance.DebugLog("trying to connect....");
            _socket = new TcpClient     //= how this constructor works , it just somehow does this:
            {
                ReceiveBufferSize = _dataBufferSize,  //  _socket.ReceiveBufferSize = _dataBufferSize;
                SendBufferSize = _dataBufferSize    //  _socket.SendBufferSize = _dataBufferSize
            };


            _receivedBuffer = new byte[_dataBufferSize];
       
            _socket.BeginConnect(instance._ip, instance._port, ConnectCallback, _socket);
             UIManager.instance.DebugLog($"Trying to connect ip:{instance._ip} port:{instance._port}");
        }

        IEnumerator TryConnect()
        {
            _connectionRan = false;
            FailedStuff();
            UIManager.instance.DebugLog($"(GetLocalIPAddress)= <color=green> {sServer.GetLocalIPAddress()} </color>");

            string ipBase = GetIPBase();
            bool valid = true;
            int index = 0;
            while (valid)
            {
                instance._ip = ipBase + index;
                if (_socket.Connected || index == 255)
                    valid = false;

                _socket.BeginConnect(instance._ip, instance._port, ConnectCallback, _socket);
                UIManager.instance.DebugLog($"Trying to connect ip:{instance._ip} port:{instance._port}");
                if (_socket.Connected || index == 255)
                    valid = false;

                yield return new WaitForSeconds(0.5f);
                ++index;

            }

            UIManager.instance.DebugLog($"Somehow we connected on {instance._ip}");
            _connectionRan = true;
        }


        private void FailedStuff()
        {
            IPAddress[] ipArray = Dns.GetHostAddresses(sServer.GetLocalIPAddress());

            foreach (var item in ipArray)
            {
                UIManager.instance.DebugLog($"HostAddresses on network=:{item}");
            }

            //ThreadManager.ExecuteOnMainThread(() =>
            //{
            //    client.StartCoroutine(TryConnect());
            //});
        }

        private string GetIPBase()
        {
            string retVal = sServer.GetLocalIPAddress();
            retVal = retVal.Substring(0, retVal.LastIndexOf(".") + 1);

            return retVal;
        }

        private void ConnectCallback(IAsyncResult result)
        {
            _socket.EndConnect(result);


            if (!_socket.Connected)
            {
                UIManager.instance.DebugLogError("<color=black>Stream DC'd:</color>");
                return;
            }

            _stream = _socket.GetStream();

            _receivedData = new sPacket();
            UIManager.instance.DebugLog("<color=blue>Stream is connected </color>:");

            _stream.BeginRead(_receivedBuffer, 0, _dataBufferSize, ReceiveCallBack, null);
        }

        public void SendData(sPacket packet)
        {
            try
            {
                if (_socket != null)
                {
                    _stream.BeginWrite(packet.ToArray(), 0, packet.Length(), null, null);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in SendData" + e);
            }
        }

        private void ReceiveCallBack(IAsyncResult result)
        {
            UIManager.instance.DebugLog("ReceivedCallBack!..");

            try
            {
                int byteLength = _stream.EndRead(result);
                if (byteLength <= 0)
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
            if (_receivedData.UnreadLength() >= 4) //int
            {
                packetLength = _receivedData.ReadInt();
                if (packetLength <= 0)
                    return true;
            }

            while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
            {
                byte[] packetBytes = _receivedData.ReadBytes(packetLength);
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (sPacket packet = new sPacket(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        UIManager.instance.DebugLog("<color=green>Client heard</color> packetId: " + packetId);
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

        public UDP(string customIP)
        {
            _endPoint = new IPEndPoint(IPAddress.Parse(customIP), instance._port);
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
                if (_socket != null)
                {
                    _socket.BeginSend(packet.ToArray(), packet.Length(), null, null);
                }
            }
            catch (Exception e)
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
                if (data.Length < 4)
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
            { (int)ServerPackets.welcome, ClientHandle.Welcome } ,
            { (int)ServerPackets.spawnPlayer, ClientHandle.SpawnPlayer }, //oldTutorial
            { (int)ServerPackets.playerPosition, ClientHandle.PlayerPosition }, //oldTutorial
            { (int)ServerPackets.playerRotation, ClientHandle.PlayerRotation }, //oldTutorial
            { (int)ServerPackets.item, ClientHandle.ItemReceived }


        };

        Debug.Log("InitClientData packets ");
    }

    private void Disconnect()
    {
        if (_isConnected)
        {
            _isConnected = false;
            _tcp._socket.Close();
            _udp._socket.Close();

            Debug.Log("Disconnected from server");
        }
    }

    private void OnDisable()
    {
        BroadcastListener.Instance.OnHostIpFound -= UpdateHostIP;
        sServer.OnHostIpFound -= UpdateHostIP;
    }


}

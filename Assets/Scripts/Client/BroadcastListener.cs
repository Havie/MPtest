using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

//[DefaultExecutionOrder(-1000000)] ///Don't load before the ThreadManager
public class BroadcastListener : MonoBehaviour
{
    public static BroadcastListener Instance { get; private set; }
    ///Route1
    public int _defaultPort = 26951; ///ToDoMake this set in one place, ScriptableObj (networkManager,SServer,Client, this)
    public UdpClient _udpListener;
    public IPEndPoint _broadcastAddress;
    public bool _enabled = true;
    public bool _iAmHost = false;
    public event Action<string> OnHostIpFound = delegate { };


    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        if (Instance != this)
            Destroy(this);

    }




    public void BroadCastIP()
    {
        ThreadManager.ExecuteOnMainThread(() =>
        {
            var data = Encoding.UTF8.GetBytes(sServer.GetLocalIPAddress());
        _udpListener.Send(data, data.Length, "255.255.255.255", _defaultPort);
        //IPManager.Ping_all();
        });

  
    }

    public void SetAsHost()
    {
        sServer.PrepareForHorribleForwardingStrategy(_udpListener);
        _iAmHost = true;
    }

    private void Start()
    {
        //Application.runInBackground = true;
        //startServer();

        //_broadcastAddress = new IPEndPoint(IPAddress.Any, 26951);
        // _listener = new UdpClient();
        //_listener.Client.Bind(_broadcastAddress);
        //_listener.BeginReceive(UDPReceiveCallBack, null);

        _udpListener = new UdpClient(_defaultPort);
        _udpListener.BeginReceive(UDPReceiveCallBack, null);



        UIManager.instance.DebugLog("BroadcastListener..listening for hostIP");
    }

  

    private void UDPReceiveCallBack(IAsyncResult result)
    {
        if (_iAmHost)
            sServer.UDPReceiveCallBack(result);
        else
        {
            ThreadManager.ExecuteOnMainThread(() =>
            {
                try
                {
                   // UIManager.instance.DebugLog($"<color=purple>UDPReceiveCallBack</color> fromBroadcast:");

                    if (!_enabled)
                    {
                        Debug.Log("not enabled so returning");
                        return;
                    }


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
                        ThreadManager.ExecuteOnMainThread(() =>
                    {
                                //UIManager.instance.DebugLogWarning($"Received: {receiveString}");
                                OnHostIpFound(receiveString);
                            });

                        return;
                    }

                }
                catch (Exception e)
                {
                    Debug.Log($"Error receiving UDP data : {e}");
                }
            });
        }
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

    public void Disable()
    {
        _enabled = false;
        _udpListener.Close();

        keepReading = false;

        //stop thread
        if (SocketThread != null)
        {
            SocketThread.Abort();
        }

        if (_tcpHandler != null && _tcpHandler.Connected)
        {
            _tcpHandler.Disconnect(false);
            Debug.Log("Disconnected!");
        }


        Destroy(this);

    }





    #region StuffThatDoesntWork
    ///Route2
    Socket _tcpListener;
    Socket _tcpHandler;

    System.Threading.Thread SocketThread;
    volatile bool keepReading = false;

    void startServer()
    {
        SocketThread = new System.Threading.Thread(NetworkCode);
        SocketThread.IsBackground = true;
        SocketThread.Start();
    }

    void NetworkCode()
    {
        IPAddress[] ipArray = Dns.GetHostAddresses(sServer.GetLocalIPAddress());
        IPEndPoint localEndPoint = new IPEndPoint(ipArray[0], 1755);
        // Create a TCP/IP socket.
        _tcpListener = new Socket(ipArray[0].AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        string data;

        //ThreadManager.ExecuteOnMainThread(() =>
        //{
        // Data buffer for incoming data.
        byte[] bytes = new Byte[1024];
        try
        {
            _tcpListener.Bind(localEndPoint);
            _tcpListener.Listen(10);

            // Start listening for connections.
            while (true)
            {
                keepReading = true;

                // Program is suspended while waiting for an incoming connection.
                UIManager.instance.DebugLog("Waiting for Connection");     //It works

                _tcpHandler = _tcpListener.Accept();
                UIManager.instance.DebugLog("Client Connected");     //It doesn't work
                data = null;

                // An incoming connection needs to be processed.
                while (keepReading)
                {
                    bytes = new byte[1024];
                    int bytesRec = _tcpHandler.Receive(bytes);
                    UIManager.instance.DebugLog("Received from Server");

                    if (bytesRec <= 0)
                    {
                        keepReading = false;
                        _tcpHandler.Disconnect(true);
                        break;
                    }

                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    UIManager.instance.DebugLog($"<color=yellow>Reading data</color> ={data}");
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }

                    System.Threading.Thread.Sleep(1);
                }

                System.Threading.Thread.Sleep(1);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        //});

    }
    #endregion
}

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;


public class sClient
{
    public static int _dataBufferSize = 4096;
    public int ID { get; private set; }
    public TCP Tcp { get; private set; }
    public UDP Udp { get; private set; }
    public int WorkStationID { get; private set; }
    public Vector3 WorldLocation { get; private set; }


    public sClient(int clientId)
    {
        ID = clientId;
        Tcp = new TCP(ID);
        Udp = new UDP(ID);
    }

    public class TCP
    {
        public TcpClient _socket;
        private readonly int _id;
        private sPacket _receivedData;
        private NetworkStream _stream;
        private byte[] _receiveBuffer;

        public TCP(int id)
        {
            _id = id;
        }

        public void Connect(TcpClient socket)
        {
            this._socket = socket;
            socket.ReceiveBufferSize = _dataBufferSize;
            socket.SendBufferSize = _dataBufferSize;

            _stream = socket.GetStream();
            _receiveBuffer = new byte[_dataBufferSize];
            _receivedData = new sPacket();

            _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallBack, null);

            // send welcome packet
            sServerSend.Welcome(_id, "Welcome to the server!"); //This isnt being called
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
                Debug.Log($"Error sending data to player {_id} Via TCP: {e}");
            }
        }

        private void ReceiveCallBack(IAsyncResult result)
        {
            try
            {
                int byteLength = _stream.EndRead(result);
                if (byteLength <= 0) ///Error checking
                {
                    sServer.DisconnectClient(_id);
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);  //Copy into our cache array

                _receivedData.Reset(HandleData(data));
                _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallBack, null);
            }
            catch (Exception e)
            {
                Debug.Log($"<color=red>Error receiving TCP data </color>: {e}");
                sServer.DisconnectClient(_id);
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
                        int packetOperation = packet.ReadInt();
                        ///To remove this being awkwardish, we could have sServer assign this dictonary here when an sClient connects??
                        sServer._packetHandlers.TryGetValue(packetOperation, out sServer.PacketHandler _delegate);
                        _delegate?.Invoke(_id, packet);

                       // Debug.Log($"HandleData TCP id={_id}  packetOp={packetOperation}");
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

        public void Disconnect()
        {
            _socket.Close();
            _stream = null;
            _receivedData = null;
            _receiveBuffer = null;
            _socket = null;
        }
    }

    public class UDP
    {
        public IPEndPoint _endPoint;
        private int _id;

        public UDP(int id)
        {
            _id = id;
        }

        public void Connect(IPEndPoint endpoint)
        {
            _endPoint = endpoint;

        }

        public void SendData(sPacket packet)
        {
            sServer.SendUDPData(_endPoint, packet);
        }

        public void HandleData(sPacket packetData)
        {
            int packetLength = packetData.ReadInt();
            byte[] packetBytes = packetData.ReadBytes(packetLength);
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (sPacket packet = new sPacket(packetBytes))
                {
                    int packetOperation = packet.ReadInt();
                    if(sServer._packetHandlers.TryGetValue(packetOperation, out sServer.PacketHandler _delegate))
                      _delegate?.Invoke(packetOperation, packet);

                    Debug.Log("HandleData 2 id=" + _id);
                }
            });
        }
        public void Disconnect()
        {
            _endPoint = null;
        }
    }

    /** can use WorkStation static dic to see where we send info to*/
    public void SetWorkStationInfo(int workStation)
    {
        WorkStationID = workStation;
    }
    public void SetWorldLocation(Vector3 location)
    {
        WorldLocation = location;
    }
    public void RequestTransportInfo()
    {
        ///Tells the clients to send up and pair IN/OUT connection details
        sServerSend.RequestTransportInfo(ID);
    }
    public void StartRound(int roundDuration)
    {
        ///This is the SERVER client
        sServerSend.StartRound(ID, roundDuration);
    }

    public void EndRound(float cycleTime, float thruPut, int shippedOnTime, int shippedLate, int wip)
    {
       sServerSend.EndRound(ID,  cycleTime,  thruPut,  shippedOnTime,  shippedLate,  wip);
    }

    public void SendItem(int itemId, List<int> qualityData)
    {
        sServerSend.SendItem(ID, itemId, qualityData);
    }


    public void Disconnect()
    {
        Debug.Log($"{Tcp._socket.Client.RemoteEndPoint} has disconnected");
        Tcp.Disconnect();
        Udp.Disconnect();
        ///Remove them from the game data
        sPlayerData.RemovePlayerInfo(ID);
        sServerHandle.RefreshAllButOneClientsMPData(this);
    }


}

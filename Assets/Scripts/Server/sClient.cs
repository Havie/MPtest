﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System;


public class sClient
{
    public static int _dataBufferSize = 4096;
    public int _id;
    public TCP _tcp;
    public UDP _udp;
    public sPlayer _player;

    public sClient(int clientId)
    {
        _id = clientId;
        _tcp = new TCP(_id);
        _udp = new UDP(_id);
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
                if (byteLength <= 0)
                {
                    sServer._clients[_id].Disconnect();
                    return;
                }

                byte[] data = new byte[byteLength];
                Array.Copy(_receiveBuffer, data, byteLength);  //Copy into our cache array

                _receivedData.Reset(HandleData(data));
                _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallBack, null);
            }
            catch (Exception e)
            {
                Debug.Log($"Error receiving TCP data: {e}");
                sServer._clients[_id].Disconnect();
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

                Debug.Log("sClient(3) calls ThreadManager");
                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (sPacket packet = new sPacket(packetBytes))
                    {
                        int packetId = packet.ReadInt();
                        Debug.LogWarning("(1)PACKETID= " + packetId);
                        sServer.PacketHandler _delegate;
                        sServer._packetHandlers.TryGetValue(packetId, out _delegate);
                        _delegate?.Invoke(packetId, packet);
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
           /* Debug.Log("sClient(4) calls ThreadManager");
            ThreadManager.ExecuteOnMainThread(() =>
            {
                using (sPacket packet = new sPacket(packetBytes))
                {
                    int packetID = packet.ReadInt();
                    Debug.LogWarning("(2)PACKETID= " + packetID);
                    Debug.Log("Dictonary contains = " + sServer._packetHandlers.ContainsKey(packetID));
                    if (sServer._packetHandlers.ContainsKey(packetID))
                    {
                        sServer.PacketHandler _delegate;
                        sServer._packetHandlers.TryGetValue(packetID, out _delegate);
                        _delegate?.Invoke(packetID, packet);
                    }
                }
            });*/
        }
        public void Disconnect()
        {
            Debug.Log("CALLED DISCONNECT ON UDP");
            _endPoint = null;
        }
    }

    public void SendIntoGame(string playerName)
    {
        _player = sNetworkManager.instance.InstantiatePlayer();
        _player.Init(_id, playerName);
        //Tell the other players about new player
        foreach (sClient client in sServer._clients.Values)
        {
            if (client._player != null)
            {
                if (client._id != _id)
                {
                    sServerSend.SpawnPlayer(_id, client._player);
                }
                //including urself
                sServerSend.SpawnPlayer(client._id, _player);
            }
        }


    }

    public void Disconnect()
    {
        Debug.Log($"{_tcp._socket.Client.RemoteEndPoint} has disconnected");
        UnityEngine.Object.Destroy(_player.gameObject);
        _player = null;
        _tcp.Disconnect();
        _udp.Disconnect();
    }

}

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace GameServer
{
    class Client
    {
        public static int _dataBufferSize = 4096;
        public int _id;
        public TCP _tcp;
        public UDP _udp;
        public Player _player;

        public Client(int clientId)
        {
            _id = clientId;
            _tcp = new TCP(_id);
            _udp = new UDP(_id);
        }

        public class TCP
        {
            public TcpClient _socket;
            private readonly int _id;
            private Packet _receivedData;
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
                _receivedData = new Packet();

                _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallBack, null);

                // send welcome packet
                ServerSend.Welcome(_id, "Welcome to the server!"); //This isnt being called
            }

            public void SendData(Packet packet)
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
                    Console.WriteLine($"Error sending data to player {_id} Via TCP: {e}");
                }
            }

            private void ReceiveCallBack(IAsyncResult result)
            {
                try
                {
                    int byteLength = _stream.EndRead(result);
                    if(byteLength <=0)
                    {
                        Server._clients[_id].Disconnect();
                        return;
                    }

                    byte[] data = new byte[byteLength];
                    Array.Copy(_receiveBuffer, data, byteLength);  //Copy into our cache array

                    _receivedData.Reset(HandleData(data));
                    _stream.BeginRead(_receiveBuffer, 0, _dataBufferSize, ReceiveCallBack, null);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error receiving TCP data: {e}");
                    Server._clients[_id].Disconnect();
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
                Console.WriteLine("handle Data sending to thread manager:");
                while (packetLength > 0 && packetLength <= _receivedData.UnreadLength())
                {
                    byte[] packetBytes = _receivedData.ReadBytes(packetLength);

                    ThreadManager.ExecuteOnMainThread(() =>
                    {
                        using (Packet packet = new Packet(packetBytes))
                        {
                            int packetId = packet.ReadInt();
                            Server._packetHandlers[packetId](_id, packet); // Invoke delegate 
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

            public void SendData(Packet packet)
            {
                Server.SendUDPData(_endPoint, packet);
            }

            public void HandleData(Packet packetData)
            {
                int packetLength = packetData.ReadInt();
                byte[] packetBytes = packetData.ReadBytes(packetLength);

                ThreadManager.ExecuteOnMainThread(() =>
                {
                    using (Packet packet = new Packet(packetBytes))
                    {
                        int packetID = packet.ReadInt();
                        Server._packetHandlers[packetID](_id, packet); //Invoke
                    }
                });
            }
            public void Disconnect()
            {
                _endPoint = null;
            }
        }

        public void SendIntoGame(string playerName)
        {
             _player = new Player(_id, playerName, new Vector3(0, 0, 0));
            //Tell the other players about new player
            foreach (Client client in Server._clients.Values)
            {
                if(client._player!=null)
                {
                    if (client._id != _id)
                    {
                        ServerSend.SpawnPlayer(_id, client._player);
                    }
                    //including urself
                    ServerSend.SpawnPlayer(client._id, _player); 
                }
            }


        }

        public void Disconnect()
        {
            Console.WriteLine($"{_tcp._socket.Client.RemoteEndPoint} has disconnected");
            _player = null;
            _tcp.Disconnect();
            _udp.Disconnect();
        }
    }
}

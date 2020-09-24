using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace GameServer
{
    class Player
    {

        public int _id;
        public string _username;

        public Vector3 _position;
        public Quaternion _rotation;

        public float _moveSpeed = 5f / Constants.TICKS_PER_SECOND;
        private bool[] _inputs;

        public Player(int id, string username, Vector3 spawnPos)
        {
            _id = id;
            _username = username;
            _position = spawnPos;
            _rotation = Quaternion.Identity;
            _inputs = new bool[4];
        }

        public void Update()
        {
            Vector3 inputDir = new Vector3(0, 0, 0);
            if (_inputs[0])
                inputDir.Y += 1;
            if (_inputs[1])
                inputDir.Y -= 1;
            if (_inputs[2])
                inputDir.X += 1;
            if (_inputs[3])
                inputDir.X -= 1;

            Move(inputDir);

        }

        public void Move(Vector3 direction)
        {
            Vector3 forward = Vector3.Transform(new Vector3(0, 0, 1), _rotation);
            //Vector3 right = Vector3.Normalize(Vector3.Cross(forward, new Vector3(0, 1, 0)));

            // Vector3 moveDir = right * direction.X + forward * direction.Y;
            //_position += moveDir * _moveSpeed;

            ServerSend.PlayerPosition(this);
            ServerSend.PlayerRotation(this); // let the client do the rotation not the server 
        }

        public void SetInput(bool[] inputs, Quaternion rot)
        {
            _inputs = inputs;
            _rotation = rot;
        }

    }
}

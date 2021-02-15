using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sPlayer : MonoBehaviour
{


        public int _id;
        public string _username;


        public float _moveSpeed = 5f / Constants.TICKS_PER_SECOND;
        private bool[] _inputs = new bool[4];

        public void Init(int id, string username)
        {
            _id = id;
            _username = username;


        }

        public void Update()
        {

            Vector3 inputDir = new Vector3(0, 0, 0);
            if (_inputs[0])
                inputDir.y += 1;
            if (_inputs[1])
                inputDir.y -= 1;
            if (_inputs[2])
                inputDir.x -= 1;
            if (_inputs[3])
                inputDir.x += 1;

            Move(inputDir);
        }

        public void Move(Vector3 direction)
        {

             Vector3 moveDir = transform.right * direction.x + transform.forward * direction.y;
            transform.position += moveDir * _moveSpeed;

            sServerSend.PlayerPosition(this);
            sServerSend.PlayerRotation(this); // let the client do the rotation not the server 
        }

        public void SetInput(bool[] inputs, Quaternion rot)
        {
            _inputs = inputs;
            transform.rotation = rot;
        }

    
}

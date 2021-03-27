using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sPlayer
{

    public int ID { get; private set; }
    public string Username { get; private set; }
    public int StationID { get; private set; }


    public sPlayer(int id, string username)
    {
        ID = id;
        Username = username;
        StationID = -1;
    }

    public void SetStationID(int id)
    {
        StationID = id;
    }


    #region OLD-ForServerSidePLayerMovement
    //public float _moveSpeed = 5f / Constants.TICKS_PER_SECOND;
    //private bool[] _inputs = new bool[4];
    //public void Update() ///NOTE Was Monobehavior on serverside
    //{

    //    Vector3 inputDir = new Vector3(0, 0, 0);
    //    if (_inputs[0])
    //        inputDir.y += 1;
    //    if (_inputs[1])
    //        inputDir.y -= 1;
    //    if (_inputs[2])
    //        inputDir.x -= 1;
    //    if (_inputs[3])
    //        inputDir.x += 1;

    //    Move(inputDir);
    //}

    //public void Move(Vector3 direction)
    //{

    //     Vector3 moveDir = transform.right * direction.x + transform.forward * direction.y;
    //    transform.position += moveDir * _moveSpeed;

    //    sServerSend.PlayerPosition(this);
    //    sServerSend.PlayerRotation(this); // let the client do the rotation not the server 
    //}

    //public void SetInput(bool[] inputs, Quaternion rot)
    //{
    //    _inputs = inputs;
    //    transform.rotation = rot;
    //}

    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutSocket : Socket
{
    public float xVelocity { get; private set; }
    public float yVelocity { get; private set; }

    private Vector3 _lastPos;

    private void Awake()
    {
        _in = false;
        _lastPos = transform.position;
    }
    private void Update()
    {
        ///Keep track of our movement Dir's
        var newPos = transform.position;
        xVelocity = (_lastPos.x - newPos.x);
        yVelocity = (_lastPos.y - newPos.y);
        _lastPos = newPos;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{


    private int _dampening = 10;


    public void DoRotation(Vector3 dir)
    {
        float dot;
        //find out if object is right side up in world 
        if (Vector3.Dot(transform.up, Vector3.up) >= 0)
            dot = -Vector3.Dot(dir, Camera.main.transform.right);
        else
            dot = Vector3.Dot(dir, Camera.main.transform.right);

        transform.Rotate(transform.up, dot / _dampening, Space.World);


        //Project the  dir changed onto the camera.Right 
        transform.Rotate(Camera.main.transform.right, Vector3.Dot(dir, Camera.main.transform.up) / _dampening, Space.World);

    }
}

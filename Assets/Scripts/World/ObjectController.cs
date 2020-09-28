﻿using System.Collections;
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

        //horiz
        transform.Rotate(transform.up, dot / _dampening, Space.World);

        //vertical 
        //Project the  dir changed onto the camera.Right 
        transform.Rotate(Camera.main.transform.right, Vector3.Dot(dir, Camera.main.transform.up) / _dampening, Space.World);

    }

    public void Follow(Vector3 loc)
    {
        this.transform.position = loc;
    }

    public void ChangeApperanceMoving()
    {
        this.transform.localScale =  new Vector3
            (0.75f * this.transform.localScale.x, 
            0.75f * this.transform.localScale.y ,
            0.75f * this.transform.localScale.z);

        ChangeMaterialColor(0.5f);


    }
    public void ChangeApperanceStill()
    {
        this.transform.localScale = new Vector3
            (1.25f * this.transform.localScale.x,
            1.25f * this.transform.localScale.y,
            1.25f * this.transform.localScale.z);


        ChangeMaterialColor(1f);
    }

    //METHOD REQUIRES SHADER TO SUPPORT ALPHA TRANSPARENCY (TODO)
    private void ChangeMaterialColor(float opacity)
    {
        var mrender = this.GetComponent<MeshRenderer>();
        if (mrender)
        {
            Material m = mrender.material;
            m.color = new Color(m.color.r, m.color.b, m.color.b, opacity);
        }

    }
}

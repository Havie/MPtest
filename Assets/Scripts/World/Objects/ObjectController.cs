using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{

    public ObjectManager.eItemID _myID;
    private int _dampening = 10;
    private Vector3 _startSize;
    private MeshRenderer _mr;
    private Rigidbody _rb;
    private Collider  _collider;
    private bool _hittingTable;
    private GameObject _table;

    private void Awake()
    {
        _startSize = this.transform.localScale;
        _mr = this.GetComponent<MeshRenderer>();
        _rb=this.gameObject.AddComponent<Rigidbody>();
        _collider = this.gameObject.GetComponent<Collider>();
        ToggleRB(true);
    }


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
       // transform.Rotate(Camera.main.transform.right, Vector3.Dot(dir, Camera.main.transform.up) / _dampening, Space.World);

    }

    public void Follow(Vector3 loc)
    {
        //this.transform.position = loc;
       // Debug.LogWarning("Told to go to:" + this.transform.position);
       if(!_hittingTable)
            this.transform.position = Vector3.Lerp(transform.position, loc, 0.5f);
       else
        {
           // if were going up, allow it
            if (loc.y > 0)
                this.transform.position = Vector3.Lerp(transform.position, loc, 0.5f);
            //else /if direction is going to go more into table prevent it,
        }

    }

    private void ToggleCollider(bool cond)
    {
        //good for testing to turn off print statements in socket collisions
        this.GetComponent<Collider>().enabled = cond;
    }

    public void ChangeApperanceMoving()
    {
        this.transform.localScale =  new Vector3
            (0.75f * this.transform.localScale.x, 
            0.75f * this.transform.localScale.y ,
            0.75f * this.transform.localScale.z); 

        ChangeMaterialColor(0.5f);

       // ToggleCollider(false);

    }
    public void ChangeApperanceNormal()
    {
        this.transform.localScale = _startSize;
        _mr.enabled = true;
        ChangeMaterialColor(1f);

        //ToggleCollider(true);
    }

    public void ChangeApperancePreview()
    {
        ChangeMaterialColor(0.5f);
    }
    public void ChangeAppearanceHidden()
    {
        _mr.enabled = false;
    }

    public void ToggleRB(bool cond)
    {
        _rb.isKinematic = cond;
        _collider.isTrigger = cond;
        _rb.useGravity = !cond;
    }

    public bool OnTable()
    {
        return _rb.useGravity == true;
    }

    public void ResetHittingTable()
    {
        _hittingTable = false;
    }

    ///METHOD REQUIRES SHADER TO SUPPORT ALPHA TRANSPARENCY ON MATERIAL
    private void ChangeMaterialColor(float opacity)
    {
        if (opacity > 1)
            Debug.LogWarning("Setting opacity > 1. Needs to be 0.0 - 1.0f");

        var mrender = this.GetComponent<MeshRenderer>();
        if (mrender)
        {
            Material m = mrender.material;
            Color color = m.color;
            color.a = opacity;
            m.color = color;
            mrender.material = m; 
        }

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Table"))
        {
          //  _hittingTable = true;
            _table = other.gameObject;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Table"))
            _hittingTable = false;
    }
}

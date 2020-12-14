using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Socket : MonoBehaviour
{

    public ObjectController Controller { get; private set; }
    public bool IsInit { get; private set; }

    //IN vars
    //not sure about this weird warning
    /*private int _requiredAttachmentID;
    private int _createdID; 
     protected float _inSensitivity = 0;
    */
    protected bool _in;


    private void Start()
    {
        Controller = this.GetComponentInParent<ObjectController>();
        if (Controller)
            IsInit = true;
    }











}

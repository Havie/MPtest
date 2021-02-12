using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Socket : MonoBehaviour
{

    public ObjectController Controller { get; private set; }
    public bool IsInit { get; private set; }

    protected bool _in;


    private void Start()
    {
        Controller = this.GetComponentInParent<ObjectController>();
        if (Controller)
            IsInit = true;
    }

}

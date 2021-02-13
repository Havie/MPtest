using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Socket : MonoBehaviour
{
    //[FormerlySerializedAs("_layerMask")]
    private int _forcedLayer = 2; ///IGNORE RAYCAST

    public ObjectController Controller { get; private set; }
    public bool IsInit { get; private set; }

    protected bool _in;


    private void Start()
    {
        Controller = this.GetComponentInParent<ObjectController>();
        if (Controller)
            IsInit = true;

        ///We don't want our userinput raycasts to hit sockets:
        this.gameObject.layer = _forcedLayer;
    }

}

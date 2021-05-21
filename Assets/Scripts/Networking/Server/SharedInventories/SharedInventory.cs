#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;


public class SharedInventory
{
    public KeyValuePair<int, int> Pair { get; protected set; }

    protected int _inID;
    protected int _outID;
    protected bool _isEmpty;
    protected float _distance;

    /************************************************************************************************************************/

    public SharedInventory(int inID, int outID, float dis)
    {
        Pair = new KeyValuePair<int, int>(inID, outID);
        _inID = Pair.Key;
        _outID = Pair.Value;
        _distance = dis;
        _isEmpty = false;
    }
    /************************************************************************************************************************/

    public int InOwnerID => _inID;
    public int OutOwnerID => _outID;
    public bool IsEmpty => _isEmpty;
    public bool IsInUse => !IsEmpty;
    public float TransportDelay => _distance;

}


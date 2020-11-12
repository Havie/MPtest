using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityAction 
{
    public enum eActionType { TAP, ROTATE}; ///static by default
    public eActionType _actionType;

    public Vector3 _location;
    public Vector2 _rotation;

    public QualityAction(eActionType action, Vector3 location, Vector2 rotation)
    {
        _actionType = action;
        _location = location;
        _rotation = rotation;
    }

   
}

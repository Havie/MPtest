using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QualityAction 
{
    public enum eActionType { TAP, ROTATE}; ///static by default
    public eActionType _actionType;


    public QualityAction(eActionType action)
    {
        _actionType = action;
    }

   
}

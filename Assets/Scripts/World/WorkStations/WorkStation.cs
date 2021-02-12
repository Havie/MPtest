﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WorkStation", menuName = "WorkStation/new Station")]
public class WorkStation : ScriptableObject
{
    //Which station sends its finished item to 
    //Eventually allow users to change this
    /*public static Dictionary<int, int> _stationFlow = new Dictionary<int, int>()
     {
        {0,0}, //for now send back to self 
        {1,2},
        {2,1}
      };*/

    public enum eStation { SELF, ONE, TWO, THREE, FOUR, FIVE, SHIPPING, NONE};
    public eStation _myStation;
    public eStation _sendOutputToStation;
    public string _myStationName = "";
    public string _stationName => _myStationName;


    public List<Task> _tasks;

    /************************************************************************************************************************/


    public bool isKittingStation()
    {
        return IsTaskType(Task.eStationType.Kitting);
    }

    public bool IsQAStation()
    {
        return IsTaskType(Task.eStationType.QA);
    }

    public bool IsShippingStation()
    {
        return IsTaskType(Task.eStationType.Shipping);
    }

    /************************************************************************************************************************/



    private bool IsTaskType(Task.eStationType type)
    {

        foreach (Task t in _tasks)
        {
            if (t._stationType == type)
            {
                if (_tasks.Count > 1 && type == Task.eStationType.Kitting)
                    Debug.LogWarning($"{_stationName} is a kitting station with more than 1 task, shouldn't happen");
                return true;
            }
        }

        return false;
    }

}

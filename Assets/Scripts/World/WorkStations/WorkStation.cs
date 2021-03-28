using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "WorkStation", menuName = "WorkStation/new Station")]
public class WorkStation : ScriptableObject
{
    /// <summary>
    /// Seems I was lazy when prototyping this and had everything public,
    /// im too concerned changing everything to seralized fields with getters will
    ///  reset all the seralized references, I am not going to take a chance on that
    /// </summary>

    public enum eStation { SELF, ONE, TWO, THREE, FOUR, FIVE, SHIPPING, NONE };
    public eStation _myStation;
    public eStation _sendOutputToStation;
    public string StationName => _myStationName;
    [SerializeField] string _myStationName = "";

    public Sprite StationInstructions => _myStationInstructions;
    [SerializeField] Sprite _myStationInstructions;

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
                {
                    Debug.LogWarning($"{StationName} is a kitting station with more than 1 task, shouldn't happen");
                }
                return true;
            }
        }

        return false;
    }

}

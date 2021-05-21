using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    ///Clone the list so no one gains access to our dataset
    public List<Task> Tasks => _tasks.Select(item => (Task) item).ToList();
    [SerializeField] List<Task> _tasks;
    public int TaskCount => _tasks.Count; //Because cloning the list isnt always a good idea

    ///A made up worldspace location for the server to use to calculate distance for transport times
    public Vector3 StationLocation => _location;
    [SerializeField] Vector3 _location;

    /************************************************************************************************************************/
    public bool isKittingStation()
    {
        return IsTaskType(Task.eStationType.Kitting);
    }
    public bool IsStackedKittingStation()
    {
        return IsTaskType(Task.eStationType.StackedKitting);
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

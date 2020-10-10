using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WorkStation : ScriptableObject
{
    //Which station sends its finished item to 
    //Eventually allow users to change this
    public static Dictionary<int, int> _stationFlow = new Dictionary<int, int>()
     {
        {0,0}, //for now send back to self 
        {1,2},
        {2,1}
      };
    public enum eStation { SELF, KITTING, TWO, THREE, FOUR, FIVE, SHIPPING};
    public eStation _myStation;
    public string _stationName =>_myStation.ToString();


    public List<Task> _tasks;


    //Need knowledge of what the UI is for this station, might become an enum if more than kitting is different
    public bool isKittingStation()
    {
        foreach(Task t in _tasks)
        {
            if (t.isKittingStation)
            {
                if (_tasks.Count > 1)
                    Debug.LogWarning($"{_stationName} is a kitting station with more than 1 task, shouldn't happen");
                return true;
            }
        }
        return false;
    }
}

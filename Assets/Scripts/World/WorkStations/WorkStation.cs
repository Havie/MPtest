using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class WorkStation : ScriptableObject
{
    //Which station sends its finished item to 
    public static Dictionary<int, int> _stationFlow = new Dictionary<int, int>()
     {
        {0,0 }, //for now send back to self 
        { 1,2 },
        {2,1 }
      };

    public enum eStation { SELF, CUBE, TRIANGLE};
    public eStation _myStation;
    public string _stationName;

}

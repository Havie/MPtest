using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Task", menuName = "Task/new Task")]
public class Task : ScriptableObject
{

    ///output
    [SerializeField]
    public List<ObjectRecord.eItemID> _finalItemID;

    ///input 
    [SerializeField]
    public List<ObjectRecord.eItemID> _requiredItemIDs;

    ///Ui info 
    //[SerializeField]
    //public bool isKittingStation; // might need to be an enum?

    ///For UI setup
    public enum eStationType { Normal, Kitting, QA, Shipping, StackedKitting}
    [SerializeField]
    public eStationType _stationType;
}

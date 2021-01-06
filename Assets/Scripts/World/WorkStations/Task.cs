using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Task", menuName = "Task/new Task")]
public class Task : ScriptableObject
{

    ///output
    [SerializeField]
    public List<ObjectManager.eItemID> _finalItemID;

    ///input 
    [SerializeField]
    public List<ObjectManager.eItemID> _requiredItemIDs;

    ///Ui info 
    //[SerializeField]
    //public bool isKittingStation; // might need to be an enum?

    ///For UI setup
    public enum eStationType { Normal, Kitting, QA, Shipping}
    [SerializeField]
    public eStationType _stationType;
}

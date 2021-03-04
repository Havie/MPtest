using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValidateStackableRestrictions : MonoBehaviour
{
    [SerializeField] Toggle _stackableToggle=default;
    //[SerializeField] BoolEvent _stackableEvent = default;

    public void BatchChanged(IntWrapper val) 
    { 
        
        if(val._value==1)
        {
            _stackableToggle.isOn = true;
            _stackableToggle.interactable = false;
        }
        else
        {
            _stackableToggle.interactable = true;
        }
    } 

   
}

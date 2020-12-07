using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Events/Void Event")]
public class VoidEvent : GameEvent<Void>
{

    public void Raise()
    {
        Raise(new Void());
    }
   
}

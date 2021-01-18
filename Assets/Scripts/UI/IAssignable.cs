using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAssignable 
{
    bool AssignItem(ObjectController oc, int count);

    bool AssignItem(int id, int count, List<QualityObject> qualities);
    
}

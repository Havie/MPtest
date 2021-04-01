using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ComponentList", menuName = "ComponentList")]
public class ComponentList : ScriptableObject
{
    [SerializeField] ObjectRecord.eItemID _finalItem1ID = default;
    [SerializeField] List<ObjectRecord.eItemID> _partList1 = default;

    ///Theres no great solution here, its hard AF to seralize and expose a dictonary in the Unity editor

    public List<ObjectRecord.eItemID> GetComponentListByItemID(int finalItemID)
    {
        if (finalItemID == (int)_finalItem1ID)
            return _partList1;

        return null;
    }

}

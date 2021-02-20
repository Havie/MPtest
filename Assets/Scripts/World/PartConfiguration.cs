using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PartConfiguration", menuName = "Parts/Configuration")]
public class PartConfiguration : ScriptableObject
{
    ///output
    [SerializeField]
    public ObjectManager.eItemID _producedItem;

    ///input 
    [SerializeField]
    public List<ObjectManager.eItemID> _requiredItems;
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.


[CreateAssetMenu(fileName = "PartConfiguration", menuName = "Parts/AssemblyBook")]
public class PartAssemblyBook : ScriptableObject
{
    [SerializeField] List<PartConfiguration> _configurations;
    public List<PartConfiguration> Configurations => _configurations;


    public List<int> GetRequiredComponentsForPart(int itemID)
    {
        List<int> parts = new List<int>();
        foreach (var prodItem in _configurations)
        {
            if((int)prodItem._producedItem == itemID)
            {
                foreach (var reqItem in prodItem._requiredItems)
                {
                    parts.Add((int)reqItem);
                }
            }
        }
        return parts;
    }

}

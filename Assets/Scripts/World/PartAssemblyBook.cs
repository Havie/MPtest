using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

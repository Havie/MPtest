#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;


public static class QualityCreator
{
    public static List<QualityData> GenerateMaxQualityForItem(int itemID)
    {
        /// Populate data from the prefabs themselves, thru the ObjectRecord
        List<QualityData> qData = new List<QualityData>();

        foreach (var quality in ObjectManager.Instance.GetQualitiesForItemID(itemID))
        {
            qData.Add(new QualityData(quality.ID, quality.MaxQuality));
        }
        return qData;
    }
}

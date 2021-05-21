#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public static class QualityConvertor
{
    public static QualityData ConvertToData(QualityObject qualityObj)
    {
        return new QualityData(qualityObj.ID, qualityObj.CurrentActions);
    }

    public static List<QualityData> ConvertListToData(List<QualityObject> list)
    {
        List<QualityData> qualityData = new List<QualityData>();
        foreach (var item in list)
        {
            qualityData.Add(ConvertToData(item));
        }
        return qualityData;
    }
}

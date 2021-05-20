#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System;
using System.Collections.Generic;
using UnityEngine;

public static class DebugQualities
{
    public static void DebugQuality(List<int> qualityData)
    {
        int count = qualityData.Count;

        for (int i = 0; i < count; i++)
        {
            UIManager.DebugLog($"<color=yellow>[QualityID]: {qualityData[i]} count=>{qualityData[++i]} </color>");
        }


    }

    public static void DebugQualitySlot(List<QualityData> qualityData)
    {
        int count = qualityData.Count;

        UIManager.DebugLog($"<color=white> count= {count} </color>");
        for (int i = 0; i < count; i++)
        {
            UIManager.DebugLog($"<color=blue>[QualityID]: {qualityData[i].ID} count=>{qualityData[i].Actions} </color>");
        }
    }

    public static void DebugQuality(List<QualityObject> qualityData)
    {
        int count = qualityData.Count;

        for (int i = 0; i < count; i++)
        {
            UIManager.DebugLog($"<color=orange>[QualityID]: {qualityData[i].ID} count=>{qualityData[i].CurrentQuality} </color>");
        }
    }

    public static void DebugQuality(List<QualityData> qualityData)
    {
        int count = qualityData.Count;

        for (int i = 0; i < count; i++)
        {
            UIManager.DebugLog($"<color=green>[QualityID]: {qualityData[i].ID} count=>{qualityData[i].Actions} </color>");
        }
    }
}


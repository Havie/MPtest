﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QualityChecker
{
    public static bool CheckFinalQuality(QualityOverall quality)
    {
        return true;

        if (quality.GetPercent() > 80)
        {
            UIManager.DebugLog($"The quality is : <color=green>{quality.GetPercent()}</color>%");
            return true;
        }
        else
            UIManager.DebugLog($"The quality is : <color=red>{quality.GetPercent()}</color>%");

        return false;
    }
}

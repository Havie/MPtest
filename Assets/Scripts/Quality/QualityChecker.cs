using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class QualityChecker 
{
   public static bool CheckFinalQuality(QualityOverall quality)
    {
       if( quality.GetPercent() > 80)
        {
            UIManager.Instance.DebugLog($"The quality is : <color=green>{quality.GetPercent()}</color>%");
            return true;
        }
       else
            UIManager.Instance.DebugLog($"The quality is : <color=red>{quality.GetPercent()}</color>%");

        return false;
    }
}

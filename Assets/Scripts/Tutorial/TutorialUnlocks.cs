﻿#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public static class TutorialUnlocks
{
    private static Dictionary<TutorialStage, bool> _stageMap ;
    private static Dictionary<TutorialItem, bool> _itemMap ;

    private static bool _hasBeenSet = false;
    /************************************************************************************************************************/
    public static void SetStages(TutorialStage[] stages)
    {
        ///We only want to run this once per session, so players can exit/rejoin tutorial perhaps from menu/scene changes?
        if (_hasBeenSet)
            return;
        _hasBeenSet = true;
        _stageMap = new Dictionary<TutorialStage, bool>();
        _itemMap = new Dictionary<TutorialItem, bool>();
        ///Parse and Init all the stages to false except for the first one
        bool isFirstStage = true;
        foreach (var stage in stages)
        {
            _stageMap.Add(stage, isFirstStage);
            if (isFirstStage)
            {
                isFirstStage = false;
            }
            ///Parse and Init all items in a stage to false except the first one
            bool isFirstItem = true;
            foreach (var item in stage.TutorialSequence)
            {
                _itemMap.Add(item, isFirstItem);
                if (isFirstItem)
                {
                    isFirstItem = false;
                }
            }
        }
    }
    public static void UnlockStage(TutorialStage stage)
    {
        _stageMap[stage] = true;
    }
    public static void UnlockStep(TutorialItem step)
    {
        _itemMap[step] = true;
    }

    public static bool isStageUnlocked(TutorialStage stage)
    {
        _stageMap.TryGetValue(stage, out bool cond);
        return cond;
    }

    public static bool IsStepUnlocked(TutorialItem step)
    {
        _itemMap.TryGetValue(step, out bool cond);
        return cond;
    }
}

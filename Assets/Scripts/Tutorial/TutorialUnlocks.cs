﻿#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public static class TutorialUnlocks
{
    public static System.Action OnStageUnlocked;
    public static System.Action OnStepUnlocked;

    private static Dictionary<TutorialStage, bool> _stageMap;
    private static Dictionary<TutorialItem, bool> _itemMap;

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
        Debug.Log($"<color=green>UNlockStage!</color> = {stage}");
        UnlockTrueInStageMap(stage);
        OnStageUnlocked?.Invoke();

    }
    private static void UnlockTrueInStageMap(TutorialStage stage)
    {
        if (_stageMap.ContainsKey(stage))
            _stageMap[stage] = true;
        else
            _stageMap.Add(stage, true);
    }
    public static void UnlockStep(TutorialItem step)
    {
        Debug.Log($"<color=green>UNlockS tEP!</color> = {step}");
        UnlockedTrueInStepMap(step);
        OnStepUnlocked?.Invoke();
    }

    private static void UnlockedTrueInStepMap(TutorialItem step)
    {
        if (_itemMap.ContainsKey(step))
            _itemMap[step] = true;
        else
            _itemMap.Add(step, true);
    }

    public static bool isStageUnlocked(TutorialStage stage)
    {
        _stageMap.TryGetValue(stage, out bool cond);
        return cond;
    }

    public static bool IsStepUnlocked(TutorialItem step)
    {
        _itemMap.TryGetValue(step, out bool cond);
        Debug.Log($"<color=green>IS {step} unlocked</color> = {cond}");
        return cond;
    }
}

#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using System;

public static class TutorialEvents 
{
    public enum eTutorialEvent { CONTINUE_BUTTON, INV_OPEN, INSTRUCTIONS, UIPART_REMOVED, PART_PICKEDUP, PART_DROPPED, PART_ROTATED ,HOLDING_HANDLE_BOLT, PART_CONSTRUCTED, QUALITY_ACTION_TAP, QUALITY_ACTION_ROTATE, SWITCH, ITEMASSIGNED, SHIP}
    public static bool ConstructionLocked { get; private set; }
    public static void LockConstruction(bool cond) { ConstructionLocked = cond; }
    /************************************************************************************************************************/

    public static void RegisterForTutorialEvent(eTutorialEvent index, Action<Void> callback)
    {
        Debug.Log($"<color=green>Registered</color> tut event at index : {index}");
        ///Arrays dont seem to work, somethings wrong with += assignment operator,
        ///doesnt seem to be the same thing in memory, under the hood conversions to delegates?
        //OnFirstContinueClicked += callback;
        //_tutorialEvents[(int)index] += callback;
        AlterEvent(index, callback, true);
    }
    public static void UnRegisterForTutorialEvent(eTutorialEvent index, Action<Void> callback)
    {
        Debug.Log($"<color=red>Unregistered</color> tut event at index : {index}");
        //_tutorialEvents[index] -= callback;
        AlterEvent(index, callback, false);
    }

    private static Action<bool> AlterEvent(eTutorialEvent index, Action<Void> callback, bool add)
    {
        switch (index)
        {
            case eTutorialEvent.CONTINUE_BUTTON:
                {
                    if (add)
                        OnFirstContinueClicked += callback;
                    else
                        OnFirstContinueClicked -= callback;
                    break;
                }
            case eTutorialEvent.INV_OPEN:
                {
                    if (add)
                        OnInventoryOpen += callback;
                    else
                        OnInventoryOpen -= callback;
                    break;
                }
            case eTutorialEvent.INSTRUCTIONS:
                {
                    if (add)
                        OnStationInstructionsClicked += callback;
                    else
                        OnStationInstructionsClicked -= callback;
                    break;
                }
            case eTutorialEvent.UIPART_REMOVED:
                {
                    if (add)
                        OnPartRemovedFromSlot += callback;
                    else
                        OnPartRemovedFromSlot -= callback;
                    break;
                }
            case eTutorialEvent.PART_PICKEDUP:
                {
                    if (add)
                        OnPartPickedUp += callback;
                    else
                        OnPartPickedUp -= callback;
                    break;
                }
            case eTutorialEvent.PART_DROPPED:
                {
                    if (add)
                        OnPartDropped += callback;
                    else
                        OnPartDropped -= callback;
                    break;
                }
            case eTutorialEvent.PART_ROTATED:
                {
                    if (add)
                        OnPartRotated += callback;
                    else
                        OnPartRotated -= callback;
                    break;
                }
            case eTutorialEvent.PART_CONSTRUCTED:
                {
                    if (add)
                        OnPartConstructed += callback;
                    else
                        OnPartConstructed -= callback;
                    break;
                } 
            case eTutorialEvent.HOLDING_HANDLE_BOLT:
                {
                    if (add)
                        OnHoldingHandleAndBolt += callback;
                    else
                        OnHoldingHandleAndBolt -= callback;
                    break;
                }
            case eTutorialEvent.QUALITY_ACTION_TAP:
                {
                    if (add)
                        OnQATap += callback;
                    else
                        OnQATap -= callback;
                    break;
                }
            case eTutorialEvent.QUALITY_ACTION_ROTATE:
                {
                    if (add)
                        OnQARotate += callback;
                    else
                        OnQARotate -= callback;
                    break;
                }
            case eTutorialEvent.SWITCH:
                {
                    if (add)
                        OnSwitch += callback;
                    else
                        OnSwitch -= callback;
                    break;
                }
            case eTutorialEvent.ITEMASSIGNED:
                {
                    if (add)
                        OnItemAssigned += callback;
                    else
                        OnItemAssigned -= callback;
                    break;
                }
            case eTutorialEvent.SHIP:
                {
                    if (add)
                        OnShip += callback;
                    else
                        OnShip -= callback;
                    break;
                }
        }

        return null;
    }

    /************************************************************************************************************************/
    public static void CallOnContinueClicked() { OnFirstContinueClicked?.Invoke(new Void()); }
    static event Action<Void> OnFirstContinueClicked;

    public static void CallOnInventoryOpened() { OnInventoryOpen?.Invoke(new Void()); }
    static event Action<Void> OnInventoryOpen;

    public static void CallOnStationInstructionsClicked() {OnStationInstructionsClicked?.Invoke(new Void()); }
    static event Action<Void> OnStationInstructionsClicked;

    public static void CallOnPartRemovedFromSlot( ) { OnPartRemovedFromSlot?.Invoke(new Void()); }
    static event Action<Void> OnPartRemovedFromSlot;

    public static void CallOnPartPickedUp( ) { OnPartPickedUp?.Invoke(new Void()); }
    static event Action<Void> OnPartPickedUp;

    public static void CallOnPartDropped( ) { OnPartDropped?.Invoke(new Void()); }
    static event Action<Void> OnPartDropped;

    public static void CallOnPartRotated() { OnPartRotated?.Invoke(new Void()); }
    static event Action<Void> OnPartRotated;

    public static void CallOnPartConstructed() { OnPartConstructed?.Invoke(new Void()); }
    static event Action<Void> OnPartConstructed;
    public static void CallOnHoldingHandleAndBolt() { OnHoldingHandleAndBolt?.Invoke(new Void()); }
    static event Action<Void> OnHoldingHandleAndBolt;
    public static void CallOnQATap() { OnQATap?.Invoke(new Void()); }
    static event Action<Void> OnQATap;
    public static void CallOnQARotate() { OnQARotate?.Invoke(new Void()); }
    static event Action<Void> OnQARotate;

    public static void CallOnSwitch() { OnSwitch?.Invoke(new Void()); }
    static event Action<Void> OnSwitch;

    public static void CallOnItemAssigned() { OnItemAssigned?.Invoke(new Void()); }
    static event Action<Void> OnItemAssigned;

    public static void CallOnShip() { OnShip?.Invoke(new Void()); }
    static event Action<Void> OnShip;

}


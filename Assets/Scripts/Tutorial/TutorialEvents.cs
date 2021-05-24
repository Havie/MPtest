#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using System;


public class TutorialEvents : MonoSingleton<TutorialEvents>
{
    public enum eTutorialEvent { FIRST_CONTINUE, INV_OPEN, INSTRUCTIONS, UIPART_REMOVED, PART_PICKEDUP, PART_DROPPED}

    /************************************************************************************************************************/

    public void RegisterForTutorialEvent(eTutorialEvent index, Action<Void> callback)
    {
        Debug.Log($"<color=green>Registered</color> tut event at index : {index}");
        ///Arrays dont seem to work, somethings wrong with += assignment operator,
        ///doesnt seem to be the same thing in memory, under the hood conversions to delegates?
        //OnFirstContinueClicked += callback;
        //_tutorialEvents[(int)index] += callback;
        AlterEvent(index, callback, true);
    }
    public void UnRegisterForTutorialEvent(eTutorialEvent index, Action<Void> callback)
    {
        Debug.Log($"<color=red>Unregistered</color> tut event at index : {index}");
        //_tutorialEvents[index] -= callback;
        AlterEvent(index, callback, false);
    }

    private Action<bool> AlterEvent(eTutorialEvent index, Action<Void> callback, bool add)
    {
        switch (index)
        {
            case eTutorialEvent.FIRST_CONTINUE:
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
        }

        return null;
    }

    /************************************************************************************************************************/
    public void CallOnFirstContinueClicked() { OnFirstContinueClicked?.Invoke(new Void()); }
    static Action<Void> OnFirstContinueClicked;

    public void CallOnInventoryOpened() { OnInventoryOpen?.Invoke(new Void()); }
    static event Action<Void> OnInventoryOpen;

    ///Called from HudButton under Timer on Canvas
    public void CallOnStationInstructionsClicked() {OnStationInstructionsClicked?.Invoke(new Void()); }
    static event Action<Void> OnStationInstructionsClicked;

    public void CallOnPartRemovedFromSlot( ) { OnPartRemovedFromSlot?.Invoke(new Void()); }
    static event Action<Void> OnPartRemovedFromSlot;

    public void CallOnPartPickedUp( ) { OnPartPickedUp?.Invoke(new Void()); }
    static event Action<Void> OnPartPickedUp;

    public void CallOnPartDropped( ) { OnPartDropped?.Invoke(new Void()); }
    static event Action<Void> OnPartDropped;


}
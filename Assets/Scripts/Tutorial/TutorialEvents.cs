#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using System;

public class TutorialEvents : MonoSingleton<TutorialEvents>
{
    [SerializeField] VoidEvent _invOpen;
    [SerializeField] VoidEvent _instructionClicked;

    List<Action<Void>> _tutorialEvents = new List<Action<Void>>();

    /************************************************************************************************************************/

    protected override void Awake()
    {
        base.Awake();
        RegisterEventCallbacks();
        SetUpEventArray();
    }

    private void RegisterEventCallbacks()
    {
        //if (_invOpen)
        //    _invOpen.OnEventRaised += CallOnInventoryOpened;

        if (_instructionClicked)
            _instructionClicked.OnEventRaised += CallOnStationInstructionsClicked;

    }
    private void SetUpEventArray()
    {
        _tutorialEvents = new List<Action<Void>>();
        _tutorialEvents.Add(OnFirstContinueClicked);

        Debug.Log($"<color=blue>Event Arr set</color>");
    }

    public void RegisterForTutorialEvent(int index, Action<Void> callback)
    {
        Debug.Log($"<color=green>Registered</color> tut event at index : {index}");
        //OnFirstContinueClicked += callback;
       //_tutorialEvents[index] += callback;

        switch(index)
        {
            case 0:
                {
                    OnFirstContinueClicked += callback;
                    break;
                }
        }
    }
    public void UnRegisterForTutorialEvent(int index, Action<Void> callback)
    {
        Debug.Log($"<color=red>UnRegistered</color> tut event at index : {index}");
        _tutorialEvents[index] -= callback;
    }

    /************************************************************************************************************************/
    public void CallOnFirstContinueClicked() 
    {
        Debug.Log($"called first button : regi? {OnFirstContinueClicked == null}");
        OnFirstContinueClicked?.Invoke(new Void());
    }
    static event Action<Void> OnFirstContinueClicked;

    public void CallOnInventoryOpened() { OnInventoryOpen?.Invoke(new Void()); }
    static event Action<Void> OnInventoryOpen;

    void CallOnStationInstructionsClicked(Void empty) { OnStationInstructionsClicked?.Invoke(empty); }
    static event Action<Void> OnStationInstructionsClicked;


    void CallOnPartRemovedFromSlot(Void empty) { OnPartRemovedFromSlot?.Invoke(empty); }
    static event Action<Void> OnPartRemovedFromSlot;


    void CallOnPartPickedUp(Void empty) { OnPartPickedUp?.Invoke(empty); }
    static event Action<Void> OnPartPickedUp;


    void CallOnPartDropped(Void empty) { OnPartDropped?.Invoke(empty); }
    static event Action<Void> OnPartDropped;


}
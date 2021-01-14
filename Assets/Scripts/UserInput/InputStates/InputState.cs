﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputState 
{
    public UserInput Brain => _brain;
    protected UserInput _brain;


    public IInteractable CurrentSelection => _currentSelection;
    protected IInteractable _currentSelection;


    /************************************************************************************************************************/

    public abstract void EnableState(IInteractable currentSelection);

    public abstract bool CanExitState(InputState nextState);

    public abstract void DisableState();

    /************************************************************************************************************************/

    public abstract void Execute(bool inputDown, Vector3 pos);


}

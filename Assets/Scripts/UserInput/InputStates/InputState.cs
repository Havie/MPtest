using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputState 
{
    public UserInputManager Brain => _brain;
    protected UserInputManager _brain;

    public IInteractable CurrentSelection => _currentSelection;
    protected IInteractable _currentSelection;

    protected float _zDepth = 1; //-9f;

    /************************************************************************************************************************/

    public abstract void EnableState(IInteractable currentSelection);

    public abstract bool CanExitState(InputState nextState);

    public abstract void DisableState();

    /************************************************************************************************************************/

    public abstract void Execute(bool inputDown, Vector3 pos);


}

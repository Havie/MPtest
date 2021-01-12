using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputState 
{

    protected UserInput _brain;
    public UserInput Brain => _brain;


    /************************************************************************************************************************/

    public abstract void EnableState();

    public abstract bool CanExitState(InputState nextState);

    public abstract void DisableState();

    /************************************************************************************************************************/

    public abstract void Execute();
}

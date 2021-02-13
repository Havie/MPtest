using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UserInput
{
    public abstract class InputState
    {
        public UserInputManager Brain => _brain;
        protected UserInputManager _brain;

        public IInteractable CurrentSelection => _currentSelection;
        protected IInteractable _currentSelection;

        protected float _zDepth => -SceneDepthInitalizer.Instance.PartDisFromCam; // 1ish;;
        protected float _partDepth => SceneDepthInitalizer.Instance.DepthOfParts; //-9ishf;
        /************************************************************************************************************************/

        public abstract void EnableState(IInteractable currentSelection);

        public abstract bool CanExitState(InputState nextState);

        public abstract void DisableState();

        /************************************************************************************************************************/

        public abstract void Execute(InputCommand command);


    }
}
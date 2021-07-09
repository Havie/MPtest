#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;


public class TutorialManager : InstanceMonoBehaviour<TutorialManager>
{
    [SerializeField] TutorialStage[] _stages = default;

    /************************************************************************************************************************/

    public TutorialStage[] GetStages()
    {
        ///Return a cloned copy so no1 can alter our original dataset
        return (TutorialStage[])_stages.Clone();
    }

    public void LoadStage(TutorialStage stage)
    {
        ///Updates the game logic to start running this stage of the tutorial
        ///cant really encapsulate inside of Stage cuz its read only data w no scene refs
    }
}

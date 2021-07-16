#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;


public class TutorialManager : MonoBehaviour //InstanceMonoBehaviour<TutorialManager>
{
    [SerializeField] TutorialLogic _logic = default;
    [SerializeField] UITutorialModalController _tutorialModalController = default;
    [SerializeField] TutorialStage[] _stages = default;
    private bool _isTutorial;
    private int _finishedStageCount = 0;
    /************************************************************************************************************************/

    private void Awake()
    {
        _isTutorial = GameManager.Instance.IsTutorial;
    }

    private void Start()
    {
        TutorialUnlocks.SetStages(_stages); ///safe guards against duplicates
        _tutorialModalController.Init(_isTutorial);
        _logic.OnSequenceFinished += FinishCurrentStage;
        _logic.SetModal(_tutorialModalController.gameObject);
    }
    private void OnDisable()
    {
        _logic.OnSequenceFinished -= FinishCurrentStage;
    }

    public TutorialStage[] GetStages()
    {
        ///Return a cloned copy so no1 can alter our original dataset
        return (TutorialStage[])_stages.Clone();
    }

    public void LoadStage(TutorialStage stage)
    {
        ///Updates the game logic to start running this stage of the tutorial
        ///cant really encapsulate inside of Stage cuz its read only data w no scene refs
        _tutorialModalController.LoadTutorialStage(stage);
        if(_isTutorial)
        {
            BeginExecuteTutorialLogic(stage);
        }
    }

    public void FinishCurrentStage()
    {
        ++_finishedStageCount;
        TutorialUnlocks.UnlockStage(_stages[_finishedStageCount]);
        ///Let the continue button in Modal advance this upon reaching finished Step
        //_tutorialModalController.ShowStageMenu(true); 
    }

    private void BeginExecuteTutorialLogic(TutorialStage stage)
    {
        _logic.InitTutorialStage(stage.TutorialSequence.ToArray());
    }
}

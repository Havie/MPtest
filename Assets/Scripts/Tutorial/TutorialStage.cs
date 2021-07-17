#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

[CreateAssetMenu(fileName = "TutorialStage", menuName = "Tutorial/ Tutorial Stage")]

public class TutorialStage : ScriptableObject, IButtonData
{
    public string StageName => _stageName;
    [SerializeField] string _stageName = default;
    //public string StageDescription => _stageDescription;
    //[SerializeField] [ResizableTextArea]  string _stageDescription = default;
    public List<TutorialItem> TutorialSequence => _tutorialSequence;
    [SerializeField] List<TutorialItem> _tutorialSequence = default;
    /************************************************************************************************************************/

    //interface- IButtonData
    public string GetID() => StageName;
}

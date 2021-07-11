#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialItem", menuName = "Tutorial/ Tutorial Stage")]

public class TutorialStage : ScriptableObject, IButtonData
{
    public string StageName => _stageName;
    [SerializeField] string _stageName = default;

    public List<TutorialItem> TutorialSequence => _tutorialSequence;
    [SerializeField] List<TutorialItem> _tutorialSequence = default;
    /************************************************************************************************************************/

    //interface- IButtonData
    public string GetID() => StageName;
}

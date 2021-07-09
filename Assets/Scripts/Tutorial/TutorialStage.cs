#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialItem", menuName = "Tutorial/ Tutorial Stage")]

public class TutorialStage : ScriptableObject
{
    public string StageName => _stageName;
    [SerializeField] string _stageName = default;
    /************************************************************************************************************************/

}

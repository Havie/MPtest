#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;


[CreateAssetMenu(fileName = "TutorialItem", menuName = "Tutorial/ Tutorial Item")]
public class TutorialItem : ScriptableObject
{
    public TutorialEvents.eTutorialEvent EventKey => _eventKey;
    [SerializeField] TutorialEvents.eTutorialEvent _eventKey;

    //public UITutorialModal.eFollowUpActions FollowUpResponse => _onCompleteResponse;
    //[SerializeField] UITutorialModal.eFollowUpActions _onCompleteResponse;
    public string TitleTxt => _title;
    [SerializeField] string _title;

    public string bodyTxt => _description;
    [ResizableTextArea]
    [SerializeField] string _description;

    public VideoClip VideoGif => _video;
    [SerializeField] VideoClip _video;

    public float TimeDelayBeforeNextInstruction => _timeInSecondsBeforeNextInstruction;
    [SerializeField] float _timeInSecondsBeforeNextInstruction =1;
    ///TODO how to detect action? Unity event i guess

}

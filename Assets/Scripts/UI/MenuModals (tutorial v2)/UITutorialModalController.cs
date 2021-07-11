#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class UITutorialModalController : MonoBehaviour
{

    [SerializeField] UITutorialTabManager _tabManager;
    /************************************************************************************************************************/

    ///If we're in the tutorial , show the initial menu 
    public void LoadTutorialMenu()
    {

    }
    ///Show the main functionality of the widget, which are the steps and videos
    public void LoadTutorialStage(TutorialStage stage)
    {
        _tabManager.LoadSequenece(stage.TutorialSequence);
    }
}

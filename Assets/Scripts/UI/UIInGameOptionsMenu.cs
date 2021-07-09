#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;


public class UIInGameOptionsMenu : UIMenuController
{

    /************************************************************************************************************************/
    private void Start()
    {
        CreateNewButton("Tutorial", ShowTutorial);
        CreateNewButton("Quit", SceneTracker.Instance.ExitScene);
    }
    private void ShowTutorial()
    {
        Debug.Log($"Show Tutorial");
    }
}

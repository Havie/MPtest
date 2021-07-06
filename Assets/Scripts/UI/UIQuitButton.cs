#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class UIQuitButton : MonoBehaviour
{
    public void Quit()
    {
        SceneTracker.Instance.ExitScene();
    }
}

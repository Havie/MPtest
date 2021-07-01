using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class UIHostMenu : MonoBehaviour
{
    public delegate void ConfirmSettings();
    public event ConfirmSettings OnConfirmSettings;


    private void OnDisable()
    {
        /// Ivoked from Button-Tab when User leaves Host Tab
        UpdateGameManager();
    }

    
    public void UpdateGameManager()
    {
        OnConfirmSettings?.Invoke();
    }

}

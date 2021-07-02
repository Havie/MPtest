using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class UIHostMenu : MonoBehaviour
{
    /// All of the button components subscribe to OnConfirmSettings via the serialized event interface
    public delegate void ConfirmSettings();
    public event ConfirmSettings OnConfirmSettings;

    public void OnEnable()
    {
        ///Dont like the circularness of this, but network menu needs to register the callback
        UIManagerNetwork.Instance.RegisterHostMenu(this);
    }

    private void OnDisable()
    {
        /// Ivoked from Button-Tab when User leaves Host Tab
        UpdateGameManager();
        UIManagerNetwork.Instance.UnRegisterHostMenu(this);
    }


    /// <summary> Ensure the components update their values/GM, also tells Server  </summary>
    public void UpdateGameManager()
    {
        OnConfirmSettings?.Invoke();
    }

}

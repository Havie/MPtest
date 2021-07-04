using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class UIHostMenu : MonoBehaviour
{
    /// All of the button components subscribe to OnConfirmSettings via the serialized event interface
    public System.Action OnConfirmSettings;

    public void OnEnable()
    {
        ///Dont like the circularness of this, but network menu needs to register the callback
        UIManagerNetwork.Instance.RegisterHostMenu(this);
    }

    private void OnDisable()
    {
        /// Ivoked from Button-Tab when User leaves Host Tab
        UpdateGameManager();
        UIManagerNetwork networkManager = UIManagerNetwork.Instance;
        if (networkManager)
        {
            networkManager.UnRegisterHostMenu(this);
        }
    }


    /// <summary> Ensure the components update their values/GM, also tells Server  </summary>
    public void UpdateGameManager()
    {
        ///WARNING: this actually isnt working becuz its being called from "OnDisable"
        ///and by the time all the events subscribed to this delegate are called, 
        ///their onDisable has already deregistered their listener, so they invoke nothing
        OnConfirmSettings?.Invoke();
    }

}

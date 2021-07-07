using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class UIHostMenu : MonoBehaviour
{
    ///When changes to the HostMenu options are confirmed this gets invoked
    public System.Action OnConfirmSettings;

    public void OnEnable()
    {
        ///Dont like the circularness of this, but network menu needs to register the callback,
        ///and this class shouldnt need to know about the actual network classes
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
        OnConfirmSettings?.Invoke();
    }

}

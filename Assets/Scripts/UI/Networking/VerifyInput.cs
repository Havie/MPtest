using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

/// <summary>
/// Script is obsolete but I am leaving it here because I want to find out why I cant do
/// <T, E, UER> properly.
/// </summary>

//public class VerifyInput<T, E, UER> : GameEventListener<T, E, UER>

public class VerifyInput : MonoBehaviour
{
    [System.Serializable]
    /// <summary>
    /// Function definition for a button click event.
    /// </summary>
    public class ButtonClickedEvent : UnityEvent { }

    [FormerlySerializedAs("onValidate")]
    // Event delegates triggered on click.
    [SerializeField]
    public ButtonClickedEvent m_OnClick = new ButtonClickedEvent();

    public ButtonClickedEvent onClick
    {
        get { return m_OnClick; }
        set { m_OnClick = value; }
    }

    public void InvokeThis()
    {
        m_OnClick.Invoke();
    }
}

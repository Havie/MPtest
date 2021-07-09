#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMenuController : MonoBehaviour
{
    [SerializeField] UIInGameMenuButton _menuButtonPREFAB = default;
    [SerializeField] Transform _instantationTransform = default;
    ///Call this when we close ourselves if anyone needs to know
    public System.Action OnClose;
    /************************************************************************************************************************/

    /// <summary>Called from X button on Module</summary>
    public void CloseSelf()
    {
        OnClose.Invoke();
        this.gameObject.SetActive(false);
    }

    protected UIInGameMenuButton CreateNewButton(string label, System.Action callback)
    {
        if (_menuButtonPREFAB)
        {
            UIInGameMenuButton button = GameObject.Instantiate(_menuButtonPREFAB, _instantationTransform);
            button.SetUpButton(label, callback);
            return button;
        }
        return null;
    }
    protected UIInGameMenuButton CreateNewButton(string label, System.Action<UIInGameMenuButton> wrappedCallBack)
    {
        if (_menuButtonPREFAB)
        {
            UIInGameMenuButton button = GameObject.Instantiate(_menuButtonPREFAB, _instantationTransform);
            button.SetUpButton(label, wrappedCallBack);
            return button;
        }
        return null;
    }

    ///Try to auto set up self
#if UNITY_EDITOR
    private void Reset()
    {
        TryFindComponents();
    }

    private void TryFindComponents()
    {
        if (_menuButtonPREFAB == null)
        {
            _menuButtonPREFAB = Resources.Load<UIInGameMenuButton>("Prefab/UI/IG_MenuButton");
        }
        if (_instantationTransform == null)
        {
            var vertLayoutChild = this.GetComponentInChildren<VerticalLayoutGroup>();
            if (vertLayoutChild)
            {
                _instantationTransform = vertLayoutChild.transform;
            }
        }
    }
#endif
}

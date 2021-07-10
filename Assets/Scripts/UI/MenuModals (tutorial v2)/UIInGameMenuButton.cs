#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[RequireComponent(typeof(Button))]
public class UIInGameMenuButton : MonoBehaviour
{
    [SerializeField] Button _button = default;
    [SerializeField] TextMeshProUGUI _buttonTxt = default;
    public IButtonData Data { get; private set; }
    System.Action<UIInGameMenuButton> _wrappedCallback;
    /************************************************************************************************************************/

    private void Awake()
    {
        FindComponents();
    }

    public void SetUpButton(string label, System.Action callback)
    {
        if (_buttonTxt)
        {
            _buttonTxt.text = label;
        }
        if(_button)
        {
            _button.onClick.AddListener(delegate { callback(); });
        }
    }   
    public void SetUpButton(string label, System.Action<UIInGameMenuButton> callback)
    {
        if (_buttonTxt)
        {
            _buttonTxt.text = label;
        }
        if(_button)
        {
            _button.onClick.AddListener(delegate { WrappedCallback(); });
            _wrappedCallback += callback;
        }
    }

    public void AssignData(IButtonData data)
    {
        Data = data;
    }
    private void WrappedCallback()
    {
        _wrappedCallback.Invoke(this);
    }


    private void FindComponents()
    {
        if (_button == null)
        {
            _button = this.GetComponent<Button>();
        }
        if (_buttonTxt == null)
        {
            _buttonTxt = this.GetComponentInChildren<TextMeshProUGUI>();
        }
    }
#if UNITY_EDITOR
    private void Reset()
    {
        FindComponents();
    }
#endif
}

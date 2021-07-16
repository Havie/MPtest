#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
[RequireComponent(typeof(Button))]
public class UIInGameMenuButton : MonoBehaviour
{
    [SerializeField] protected Button _button = default;
    [SerializeField] protected TextMeshProUGUI _buttonTxt = default;
    public IButtonData Data { get; private set; }
    System.Action<UIInGameMenuButton> _wrappedCallback;
    /************************************************************************************************************************/

    protected virtual void Awake()
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
#if UNITY_EDITOR
        Debug.Log($"<color=red> Changed Button name</color> --> {this.gameObject.name } to {"MenuButton_" + data.ToString()}");
        this.gameObject.name = "MenuButton_" + data.ToString();
#endif
        Data = data;
    }
    public void LockButton(bool cond)
    {
        Debug.Log($"<color=yellow>{this.gameObject.name}</color> LockButton =  {cond} ..  _button.interactable = {!cond}");
        _button.interactable = !cond;
        ///TODO display lock icon
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

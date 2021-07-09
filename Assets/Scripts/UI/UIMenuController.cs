#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;


public class UIMenuController : MonoBehaviour
{
    [SerializeField] GameObject _gameMenu = default;
    [SerializeField] GameObject _tutorialMenu = default;

    private bool _isTutorial;
    private bool _isOn;
    /************************************************************************************************************************/
    private void Awake()
    {
        _isTutorial = GameManager.Instance.IsTutorial;
    }
    public void ToggleMenu()
    {
        _isOn = !_isOn;
        if (_isTutorial)
        {
            _tutorialMenu.SetActive(_isOn);
            return;
        }
        _gameMenu.SetActive(_isOn);
    }
}

#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;


public class UIInGameMenuManager : MonoBehaviour
{
    [SerializeField] GameObject _gameMenu = default;
    [SerializeField] GameObject _tutorialMenu = default;

    private bool _isTutorial;
    private bool _isOn;
    /************************************************************************************************************************/
    private void Awake()
    {
        _isTutorial = GameManager.Instance.IsTutorial;
        ListenForMenuCloses();
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

    /// <summary> Menus are allowed to close themselves and we need to know if they do to reset our state </summary>
    private void ListenForMenuCloses()
    {
        UIInGameOptionsMenu igMenu = _gameMenu.GetComponent<UIInGameOptionsMenu>();
        if(igMenu)
        {
            igMenu.OnClose += WidgetClosedSelf;
        }
        ///Listen for this regardless of if its the tutorial or not, because we can still reach this menu
        UIInGameOptionsMenu tutMenu = _tutorialMenu.GetComponent<UIInGameOptionsMenu>();
        if (tutMenu)
        {
            tutMenu.OnClose += WidgetClosedSelf;
        }
    }
    private void WidgetClosedSelf()
    {
        if(_isOn!=true)
        {
            Debug.Log($"<color=yellow>[UIInGameMenuManager] </color> widget closed self but wasnt on?");
        }
        _isOn = false;
    }
}

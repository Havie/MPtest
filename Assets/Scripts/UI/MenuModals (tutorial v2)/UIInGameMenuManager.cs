#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;


public class UIInGameMenuManager : MonoBehaviour
{
    [SerializeField] GameObject _gameMenu = default; //UIMenuController
    [SerializeField] GameObject _tutorialMenu = default;

    private bool _isTutorial;
    private bool _isOn;
    /************************************************************************************************************************/
    private void Awake()
    {
        _isTutorial = GameManager.Instance.IsTutorial;
        //ListenForMenuCloses();
    }

    /// <summary>Called from close button's on both menus</summary>
    public void ToggleMenu()
    {
        _isOn = !_isOn;
        if (_isTutorial)
        {
            ToggleMenu(_tutorialMenu);
            return;
        }
        ToggleMenu(_gameMenu);
    }

    private void ToggleMenu(GameObject menu)
    {
        if(menu)
        {
            menu.SetActive(_isOn);
        }
    }
    
    /* Can bring this back if we want to be less dependent, but for now let the close button have a 
     * seralized reference to this.Toggle()
    /// <summary> Menus are allowed to close themselves and we need to know if they do to reset our state </summary>
    private void ListenForMenuCloses()
    {
        if(_gameMenu)
        {
            _gameMenu.OnClose += WidgetClosedSelf;
        }
        ///Listen for this regardless of if its the tutorial or not, because we can still reach this menu
        if (_tutorialMenu)
        {
            _tutorialMenu.OnClose += WidgetClosedSelf;
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
    */
}

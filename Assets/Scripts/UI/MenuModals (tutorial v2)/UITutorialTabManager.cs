#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using Helpers;

public class UITutorialTabManager : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] UITutorialTab _tabPREFAB = default;
    [Header("Other Components")]
    [SerializeField] UITutorialContentModal _contentModal = default;
    [Header("Self Components")]
    [SerializeField] private Transform _instantationLocation = default;
    [SerializeField] private UITutorialTab _endTab = default;
    private ManagedList<UITutorialTab, TutorialItem> _managedList = new ManagedList<UITutorialTab, TutorialItem>();
    private UITutorialTab _activeTab = default;
    private bool _isInitalized = false;
    /************************************************************************************************************************/

    private void Start()
    {
        if (!_isInitalized)
            Init();
    }
    private void Init()
    {
        ///Init our resizeable list
        _managedList.Init(_instantationLocation, _tabPREFAB, InitTab, AssignInfoToTab);
        /// Init our endTab
        _endTab.SetInfo(-1, "Finished");
        _endTab.SetUpButton("Finished", OnFinish);
        /// Init our content divs nav arrows to interact like  a tab 
        _contentModal.SetButtonNavigationCallBack(true, GoLeft);
        _contentModal.SetButtonNavigationCallBack(false, GoRight);
        _isInitalized = true;
    }
    /************************************************************************************************************************/

    public void LoadSequenece(List<TutorialItem> sequence)
    {
        if (!_isInitalized)
            Init();

        _managedList.DisplayList(sequence);
        _activeTab = _managedList.GetFirstItemInList();
        _activeTab.SetFocused(true);
        _endTab.transform.SetAsLastSibling();
    }
    public void OnFinish()
    {
        //The finished tab
        TabClickedCallBack(_endTab);
    }
    
    /************************************************************************************************************************/
    private void InitTab(UITutorialTab tab)
    {
        tab.SetUpButton("", TabClickedCallBack);
    }

    private void TabClickedCallBack(UIInGameMenuButton tab)
    {
        if(_activeTab)
        {
            _activeTab.SetFocused(false);
        }
        _activeTab = tab as UITutorialTab;
        _activeTab.SetFocused(true);
        ///Fill Content Div:
        _contentModal.DisplayInfo();
    }

    private void AssignInfoToTab(int index, UITutorialTab tab, TutorialItem item)
    {
        tab.SetInfo(index, item.TitleTxt);
        tab.SetFocused(tab == _activeTab); ///Should always be false?
     }

    private void GoRight()
    {

    }

    private void GoLeft()
    {

    }
}

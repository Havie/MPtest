#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using Helpers;

public class UITutorialTabManager : MonoBehaviour
{
    public enum eTabDir { LEFT, RIGHT}
    [Header("Prefab")]
    [SerializeField] UITutorialTab _tabPREFAB = default;
    [Header("Other Components")]
    [SerializeField] UITutorialContentModal _contentModal = default;
    [Header("Self Components")]
    [SerializeField] private Transform _instantationLocation = default;
    [SerializeField] private UITutorialTab _endTab = default;
    private IndexedManagedList<UITutorialTab, TutorialItem> _managedList = new IndexedManagedList<UITutorialTab, TutorialItem>();
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
        ///TODO We need passed in data about which item is unlocked or not
        _managedList.DisplayList(sequence);
        ///Force set our first tab to be clicked/display info
        TabClickedCallBack(_managedList.GetFirstItemInList());
        _endTab.transform.SetAsLastSibling();
    }
    public void OnFinish()
    {
        //The finished tab
        TabClickedCallBack(_endTab);
    }
    
    public void EnableGoingDirection(eTabDir dir)
    {
        _contentModal.EnableContentArrow(dir == eTabDir.LEFT);
    }
    public void DisableGoingDirection(eTabDir dir)
    {
        _contentModal.DisableContentArrow(dir == eTabDir.LEFT);
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
        ///Fill Content Div: (this interface is a bit sketchy)
        _contentModal.DisplayInfo(_activeTab.Data as TutorialItem);
    }

    private void AssignInfoToTab(int index, UITutorialTab tab, TutorialItem item)
    {
        tab.SetInfo(index, item.TitleTxt);
        tab.SetFocused(tab == _activeTab); ///Should always be false?
        tab.AssignData(item);
     }

    private void GoRight()
    {
        if(!TryClickTabAtIndex(_managedList.GetIndexOfManagedItem(_activeTab) + 1))
        {
            ///We are on the last item in the managed list, so manually click finish tab
            OnFinish();
        }
    }

    private void GoLeft()
    {
        int index = _managedList.GetIndexOfManagedItem(_activeTab) -1 ;
        if (_activeTab == _endTab)
        {
            /// We are on the finish tab which is not apart of the managed list, so click the last item in managed list
            index = _managedList.GetLastIndex();
        }
        TryClickTabAtIndex(index);

    }

    private bool TryClickTabAtIndex(int index)
    {
       return  _managedList.EnactOnManagedItemByIndex(index, TabClickedCallBack);
    }
}

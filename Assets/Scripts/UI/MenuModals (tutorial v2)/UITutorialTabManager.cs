#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using Helpers;

public class UITutorialTabManager : MonoBehaviour
{
    public enum eTabDir { LEFT, RIGHT }
    [Header("Prefab")]
    [SerializeField] UITutorialTab _tabPREFAB = default;
    [Header("Other Components")]
    [SerializeField] UITutorialContentModal _contentModal = default;
    [Header("Self Components")]
    [SerializeField] private Transform _instantationLocation = default;
    [SerializeField] private UITutorialTab _endTab = default;
    [Header("Settings")]
    [SerializeField] private int _headerPixelWidthMax = 1000; ///TODO implement logic for this 
    private IndexedManagedList<UITutorialTab, TutorialItem> _managedList = new IndexedManagedList<UITutorialTab, TutorialItem>();
    private UITutorialTab _activeTab = default;
    private bool _isInitalized = false;
    /************************************************************************************************************************/
    private void OnDestroy()
    {
        Subscribe(false);
    }
    private void Start()
    {
        Subscribe(true);
    }
    private void Subscribe(bool cond)
    {
        if (cond)
        {
            TutorialUnlocks.OnStepUnlocked += TutorialAdvanced;
            TutorialUnlocks.OnStageUnlocked += SequenceFinished;
        }
        else
        {
            TutorialUnlocks.OnStepUnlocked -= TutorialAdvanced;
            TutorialUnlocks.OnStageUnlocked -= SequenceFinished;
        }
    }
    private void Init(bool isTutorial)
    {
        ///Init our resizeable list
        _managedList.Init(_instantationLocation, _tabPREFAB, InitTab, AssignInfoToTab);
        /// Init our endTab
        _endTab.SetInfo(-1, "Finished");
        _endTab.SetUpButton("Finished", OnFinish); ///This is semi unused now becuz tabs cant be clicked anymore
        if (isTutorial)
        {
            ///Lock the finished tab so it can be unlocked after completing the last step
            _endTab.LockButton(true);
        }
        else
        {
            ///Dont show the finished tab in this version of the help menu
            _endTab.gameObject.SetActive(false);
        }
        /// Init our content divs nav arrows to interact like  a tab 
        _contentModal.SetButtonNavigationCallBack(true, GoLeft);
        _contentModal.SetButtonNavigationCallBack(false, GoRight);
        _isInitalized = true;
    }
    /************************************************************************************************************************/

    public void LoadSequenece(List<TutorialItem> sequence, bool isTutorial)
    {
        if (!_isInitalized)
            Init(isTutorial);
        _managedList.DisplayList(sequence);
        ///Force set our first tab to be clicked/display info
        TabClickedCallBack(_managedList.GetFirstItemInList());
        _endTab.transform.SetAsLastSibling();
    }
    public void HideTabs(bool cond)
    {
        _instantationLocation.gameObject.SetActive(!cond);
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
        tab.SetUpButton("", IgnoreTabClickCallBack);
        ///HACK- Disable button component here, to make ImageComponent work as expected with UIInGameMenuButton.SetFocused()
        ///In order to preserve some functionality if we ever want to reuse the logic for making tabs clickable
        tab.GetComponent<UnityEngine.UI.Button>().enabled = false;
    }
    private void IgnoreTabClickCallBack(UIInGameMenuButton tab)
    {
        ///Decided via design these tabs arent supposed to clicked, only the arrows
    }

    private void TabClickedCallBack(UIInGameMenuButton tab)
    {
        ///Reset old tab
        if (_activeTab)
        {
            _activeTab.SetFocused(false);
        }
        ///Cache our new tab
        _activeTab = tab as UITutorialTab;
        _activeTab.SetFocused(true);
        /// grab data from the button (this interface is a bit sketchy)
        var tutorialItem = _activeTab.Data as TutorialItem;
        Debug.Log($"<color=purple>DataFromTab= </color> {tutorialItem}");
        ///Fill Content Div:
        _contentModal.DisplayInfo(tutorialItem);
        /// figure out if we can go left/right on the modal based on this Item's index / ItemStep? (will lock arrows/other tabs)
        FigeOutIfActiveTabCanNavigate();
    }

    private void AssignInfoToTab(int index, UITutorialTab tab, TutorialItem item)
    {
        ///Assign the title text and keep track of our index in the list
        tab.SetInfo(index, item.TitleTxt);
        ///Change the display to if its the active tab or not
        tab.SetFocused(tab == _activeTab); ///Should always be false?
                                           ///Set the button to interactable based on if its been unlocked or not yet via the tutorial
        tab.LockButton(!TutorialUnlocks.IsStepUnlocked(item));
        ///Store the item data for later
        tab.AssignData(item);
    }
    private void OnFinish()
    {
        //The finished tab
        TabClickedCallBack(_endTab);
    }
    private void GoRight()
    {
        var rightIndex = _managedList.GetIndexOfManagedItem(_activeTab) + 1;
        if (!TryClickTabAtIndex(rightIndex))
        {
            ///We are on the last item in the managed list, so manually click finish tab
            OnFinish();
        }
    }
    private void GoLeft()
    {
        int leftIndex = _managedList.GetIndexOfManagedItem(_activeTab) - 1;
        if (_activeTab == _endTab)
        {
            /// We are on the finish tab which is not apart of the managed list, so click the last item in managed list
            leftIndex = _managedList.GetLastIndex();
        }
        TryClickTabAtIndex(leftIndex);

    }
    private bool TryClickTabAtIndex(int index)
    {
        return _managedList.EnactOnManagedItemByIndex(index, TabClickedCallBack);
    }
    private void TutorialAdvanced(TutorialItem item)
    {
        var currIndex = _managedList.GetIndexOfManagedItem(_activeTab);
        UITutorialTab tab = _managedList.GetManagedItemAtIndex(currIndex + 1);
        if (tab && tab.Data as TutorialItem == item)
        {
            GoRight();
        }
        else
        {
            Debug.Log($"<color=red> NO tab at index </color>: {currIndex + 1} for Unlockitem : {item} ");
        }

    }
    private void FigeOutIfActiveTabCanNavigate()
    {
        var currIndex = _managedList.GetIndexOfManagedItem(_activeTab);
        if (currIndex== -1)
        {
            ///We are outside of the list range 
            if(_activeTab==_endTab)
            {
                ///Force them to only be able to return to stage menu
                DisableGoingDirection(eTabDir.LEFT);
                DisableGoingDirection(eTabDir.RIGHT);
            }
            return;
        }
        if (!_managedList.EnactOnManagedItemByIndex(currIndex - 1, InspectTabLeft))
        {
            ///There are no more items to the left, so disable left arrow
            DisableGoingDirection(eTabDir.LEFT);
        }
        if (!_managedList.EnactOnManagedItemByIndex(currIndex + 1, InspectTabRight))
        {
            ///There are no more items to the right so disable right arrow
            DisableGoingDirection(eTabDir.RIGHT);
        }
    }
    private void InspectTabRight(UITutorialTab tab)
    {
        HandleTabAndArrow(tab, eTabDir.RIGHT);
    }
    private void InspectTabLeft(UITutorialTab tab)
    {
        HandleTabAndArrow(tab, eTabDir.LEFT);
    }
    private void HandleTabAndArrow(UITutorialTab tab, eTabDir dir)
    {
        bool isUnlocked = TutorialUnlocks.IsStepUnlocked(tab.Data as TutorialItem);
        Debug.Log($"{dir}...IsStepUnlocked : {tab.Data as TutorialItem}  = {isUnlocked}");
        ///Disable/Enable the tab in the header
        tab.LockButton(!isUnlocked);
        if (!isUnlocked)
        {
            ///Disable Arrow in the content
            DisableGoingDirection(dir);
        }
        else
        {
            ///Enable Arrow in the content
            EnableGoingDirection(dir);
        }
    }
    private void SequenceFinished(TutorialStage stage)
    {
        ///Display the final Tab
        OnFinish();
    }
    
}

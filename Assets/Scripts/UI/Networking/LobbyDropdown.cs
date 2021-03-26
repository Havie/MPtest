
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyDropdown : Dropdown
{

    private List<int> _invalidDropDownIndicies;

    /// <summary>Overriden to figure out when the menu is opened</summary>
    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        var dropDown=this.transform.Find("Dropdown List");
        LockDropDownItems(dropDown);
    }


    public void SetLockedDropDownIndicies(List<int> invalidIndicies)
    {
        _invalidDropDownIndicies = invalidIndicies;
    }


    //**************PRIVATE******************************************************************//

    /// <summary> Prevent user from selecting an inuse option in the dropdown menu </summary>
    private void LockDropDownItems(Transform dropdownTransform)
    {
        ///The true here included inactive buttons!?
        var toggleArr = dropdownTransform.GetComponentsInChildren<Toggle>(true);

        for (int i = 0; i < _invalidDropDownIndicies.Count; i++)
        {
            int invalidIndex = _invalidDropDownIndicies[i];
            if(invalidIndex != 0) ///option 0 is always allowed , its "None"
            {
                ///Off by 1 handle
                toggleArr[invalidIndex+1].interactable = false;
               // Debug.Log($"{i} .. set { toggleArr[invalidIndex].gameObject.name} to interactble = <color=red>false</color>");
            }
        }
    }
}

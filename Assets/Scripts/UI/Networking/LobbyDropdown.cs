
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyDropdown : Dropdown
{

    public System.Action OnDropdownShown;

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);
        OnDropdownShown?.Invoke();
        //Debug.Log("DROPDOWN SHOWN");

    }
}

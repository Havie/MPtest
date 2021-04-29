#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class LobbyInstructions : StaticMonoBehaviour<LobbyInstructions>
{
    [SerializeField] UIStationInstructions _instructions = default;

    public void ToggleInstructions(Sprite img)
    {
        if(_instructions)
        {
            _instructions.ToggleInstructions(img);
        }
    }

    public void ShowInstructions(bool cond)
    {
        if (_instructions)
        {
            _instructions.ShowInstructionsForced(false);
        }
    }
}


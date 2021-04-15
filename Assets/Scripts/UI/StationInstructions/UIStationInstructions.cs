﻿#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(AudioSource))]

public class UIStationInstructions : MonoBehaviour
{
    [SerializeField] Image _instructionIMG = default;
    [SerializeField] SoundHelper _soundHelper;
    bool _isOn;
    

    public void ToggleInstructions()
    {
        _isOn = !_isOn;
        this.gameObject.SetActive(_isOn);
        if (_isOn)
        {
            ///Get Most recent Instruction from GM
            Sprite img = GameManager.Instance._workStation.StationInstructions;
            AssignInstructions(img);

        }

        if (_soundHelper)
            _soundHelper.PlayAudio();

    }

    private void AssignInstructions(Sprite img)
    {
        _instructionIMG.sprite = img;
    }
}

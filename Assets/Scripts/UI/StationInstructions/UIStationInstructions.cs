#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.
using UnityEngine;
using UnityEngine.UI;

//[RequireComponent(typeof(AudioSource))]

public class UIStationInstructions : MonoBehaviour
{
    [SerializeField] Image _instructionIMG = default;
    [SerializeField] SoundHelper _soundHelper;
    bool _isOn;


    ///Called from button X / InventoryScene
    public void ToggleInstructions()
    {
        Sprite img = null;
        var gmWorkStation = GameManager.instance._workStation;
        if (gmWorkStation)
        {
            ///Get Most recent Instruction from GM
            img = gmWorkStation.StationInstructions;
        }
        ToggleInstructions(img);
    }

    public void ToggleInstructions(Sprite img)
    {
        _isOn = !_isOn;
        this.gameObject.SetActive(_isOn);
        if (_isOn)
        {
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

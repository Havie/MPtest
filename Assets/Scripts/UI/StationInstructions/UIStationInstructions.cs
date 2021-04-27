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
        if(img == null )
        {
            Debug.Log($"<color=yellow> NULL img..</color> wont show");
            _isOn = false;
        }
        this.gameObject.SetActive(_isOn);
        if (_isOn)
        {
            AssignInstructions(img);
        }

        if (_soundHelper)
            _soundHelper.PlayAudio();
    }

    public void ShowInstructionsForced(bool cond)
    {
        _isOn = !cond; ///Flip the key to what we dont want, then toggle will undo
        ToggleInstructions();
    }

    private void AssignInstructions(Sprite img)
    {
        _instructionIMG.sprite = img;
    }
}

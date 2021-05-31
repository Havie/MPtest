using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : InteractableObject
{

    [SerializeField] GameObject _switch = default;
    [SerializeField] GameObject _switchFlipped = default;
    [SerializeField] GameObject _lighteningVFXPREFAB = default;
    [SerializeField] Transform _lighteningVFXLocation = default;


    public bool On => _on;
    private bool _on;

    private MeshRenderer _mr1;
    private MeshRenderer _mr2;

    void Awake()
    {
        _mr1 = _switch.GetComponent<MeshRenderer>();
        _mr2 = _switch.GetComponent<MeshRenderer>();
    }


    public override void OnInteract()
    {
        if(GameManager.Instance.IsTutorial && TutorialEvents.SwitchLocked)
        {
            return;
        }
        Debug.Log($"What actt");
        _on = !On;
        ToggleChildren(On);
        CheckQualityConditions(On);
    }

    public void TurnOff()
    {
        if (_on)
        {
            OnInteract();
        }
    }

    private void CheckQualityConditions(bool On)
    {
        QualityOverall quality = this.GetComponentInParent<QualityOverall>();
        ObjectController oc = quality.GetComponent<ObjectController>();

        if (oc._myID == ObjectRecord.eItemID.RectwTopBotPurplePlug && On)  /// item 12
        {
            if (QualityChecker.CheckFinalQuality(quality))
            {
                ///SPAWN OBJ 13 in exact position
                ///Play VFX on OBJ 13
                ObjectManager.Instance.SpawnFinalPower(oc, quality.Qualities);
                if(GameManager.Instance.IsTutorial)
                {
                    TutorialEvents.CallOnSwitch();
                }
            }
            else
            {
                ///Play failure sound TODO
            }

            return;
        }
        else if (oc._myID == ObjectRecord.eItemID.finalPower)  /// item 13
        {
            ///Could check quality of this again but shud always be enough to pass since we got it spawned in the first place
            PlayVFX(On);
        }
    }

    void PlayVFX(bool cond)
    {
        Debug.Log("Play VFX=" + cond);
        var vfxIn = VFXManager.Instance;
        if (vfxIn)
        {
            if (cond)
            {
                vfxIn.PerformEffect(_lighteningVFXPREFAB, _lighteningVFXLocation, true);
            }
            else
            {
                vfxIn.StopEffect(_lighteningVFXPREFAB);
            }
        }
        else
            Debug.Log("<color=yellow>no vfx?</color>");
    }


    private void ToggleChildren(bool cond)
    {
        _switchFlipped.SetActive(cond);
        _switch.SetActive(!cond);
    }

    public MeshRenderer GetRightMR()
    {
        return On ? _mr2 : _mr1;
    }

    public void ShowNormal()
    {
        var _mr = GetRightMR();
        _mr.enabled = true;
        ChangeMaterialColor(1);
    }

    public void ShowInPreview()
    {
        var _mr = GetRightMR();
        _mr.enabled = true;
        ChangeMaterialColor(0.5f);
    }


    private void ChangeMaterialColor(float opacity)
    {

        if (opacity > 1)
            Debug.LogWarning("Setting opacity > 1. Needs to be 0.0 - 1.0f");

        var _mr = GetRightMR();
        if (_mr)
        {
            Material m = _mr.material;
            Color color = m.color;
            color.a = opacity;
            m.color = color;
            _mr.material = m;
        }
    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : InteractableObject
{

    [SerializeField] GameObject _switch= default;
    [SerializeField] GameObject _switchFlipped = default;



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
        _on = !On;
        ToggleChildren(On);
        PlayVFX(CheckQualityConditions(On));

    }

    private bool CheckQualityConditions(bool On)
    {
        if (On)
        {
            QualityOverall quality = this.GetComponentInParent<QualityOverall>();
            ObjectController oc = quality.GetComponent<ObjectController>();
           // if (oc._myID == ObjectManager.eItemID.GreenRect1)  ///FinalPower
            {
                if (QualityChecker.CheckFinalQuality(quality))
                {
                    return true;
                }
            }
        }

        return false;
    }

    void PlayVFX(bool cond)
    {
        Debug.Log("Play VFX=" + cond);
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

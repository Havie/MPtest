using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour, IInteractable
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


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            OnInteract();
    }

    public void OnInteract()
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
            if (oc._myID == ObjectManager.eItemID.GreenRect1)  ///FinalPower
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

    public GameObject GetGameObject() => gameObject;

    public Transform GetParent() => this.transform.parent;

    public Transform Transform() => this.transform;

    public void HandleInteractionTime(float time)
    {
        throw new System.NotImplementedException();
    }

    public void OnFollowInput(Vector3 worldPos)
    {
       ///DO nothing?
    }

    public Vector2 OnRotate(Vector3 dot)
    {
        return Vector2.zero;
    }

    public bool OutOfBounds() => false;

    public bool IsPickedUp() => false;


    public bool IsHighlighted() 
    {
        throw new System.NotImplementedException();
    }

    public void SetHighlighted(bool cond)
    {
        throw new System.NotImplementedException();
    }

    public void ChangeHighlightAmount(float amnt)
    {
        throw new System.NotImplementedException();
    }

    public void SetHandPreviewingMode(bool cond)
    {
        throw new System.NotImplementedException();
    }

    public void ChangeAppearanceMoving()
    {
        throw new System.NotImplementedException();
    }

    public void ChangeAppearanceNormal()
    {
        throw new System.NotImplementedException();
    }

    public void ChangeAppearanceHidden(bool cond)
    {
        throw new System.NotImplementedException();
    }

    public void ResetHittingTable()
    {
        throw new System.NotImplementedException();
    }

    public void SetResetOnNextChange()
    {
        throw new System.NotImplementedException();
    }
}

using HighlightPlus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightableObject : InteractableObject, IHighlightable
{
    protected HighlightTrigger _highlightTrigger;



    protected virtual void Awake()
    {
        SetUpHighlightComponent(); 
    }


    private void SetUpHighlightComponent()
    {
        var effect = transform.gameObject.AddComponent<HighlightEffect>();
        var profile = Resources.Load<HighlightProfile>("Shaders/Highlight Plus Profile");
        if (profile != null)
            effect.ProfileLoad(profile);
        _highlightTrigger = this.gameObject.AddComponent<HighlightTrigger>();

    }


    #region Interface

    ///IHighlightable
    private bool _isHighlighted;
    public bool IsHighlighted() => _isHighlighted;
    public void SetHighlighted(bool cond)
    {
        if (_highlightTrigger)
            _highlightTrigger.Highlight(cond);

        var childrenHighlights = GetComponentsInChildren<HighlightTrigger>();
        foreach (var item in childrenHighlights)
        {
            item.Highlight(cond);
        }

        _isHighlighted = cond;
    }
    public void ChangeHighlightAmount(float intensity)
    {
        if (_highlightTrigger)
        {
            var effect = this.GetComponent<HighlightEffect>();
            effect.outline = intensity;

            var childrenEffects = GetComponentsInChildren<HighlightEffect>();
            foreach (var item in childrenEffects)
            {
                item.outline = intensity;
            }
        }
    }
    public float GetHighlightIntensity()
    {
        if (_highlightTrigger)
        {
            var effect = this.GetComponent<HighlightEffect>();
            return effect.outline;
        }
        return 0;
    }
    public Color GetHighLightColor()
    {
        if (_highlightTrigger)
        {
            var effect = this.GetComponent<HighlightEffect>();
            return effect.outlineColor;
        }
        return Color.white;
    }
    public void ChangeHighLightColor(Color color)
    {
        if (_highlightTrigger)
        {
            var effect = this.GetComponent<HighlightEffect>();
            effect.outlineColor = color;

            var childrenEffects = GetComponentsInChildren<HighlightEffect>();
            foreach (var item in childrenEffects)
            {
                item.outlineColor = color;
            }
        }
    }
    public virtual void HandleHighlightPreview()
    {
        ///if its not highlighting turn it on 
        if (!IsHighlighted())
        {
            SetHighlighted(true);
            ChangeHighlightAmount(0);
        }
    }
    public virtual void CancelHighLightPreview()
    {
        if (IsHighlighted())
            SetHighlighted(false);
    }


    #endregion

}

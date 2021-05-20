using HighlightPlus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighlightableObject : InteractableObject, IHighlightable
{

    private CustomShaderController _controller;
    private CustomShaderController[] _childrenControllers;

    protected virtual void Awake()
    {
        _controller = this.gameObject.AddComponent<CustomShaderController>();
    }

    protected void Start()
    {
        _childrenControllers = GetComponentsInChildren<CustomShaderController>();
    }


    #region Interface

    ///IHighlightable
    private bool _isHighlighted;
    public bool IsHighlighted() => _isHighlighted;
    public void SetHighlighted(bool cond)
    {
        _isHighlighted = cond;
        if (_controller)
            _controller.Highlight(cond);
        if (_childrenControllers == null)
            return;

        foreach (var item in _childrenControllers)
        {
            item.Highlight(cond);
        }
    }
    public void ChangeHighlightAmount(float intensity)
    {
        if (_controller)
        {
            _controller.SetOutlineIntensity(intensity);
        }
        if (_childrenControllers == null)
            return;

        foreach (var item in _childrenControllers)
        {
            item.SetOutlineIntensity(intensity);
        }

    }
    public float GetHighlightIntensity()
    {
        if (_controller)
        {
            return _controller.GetOutlineIntensity();
        }
        return 0;
    }
    public Color GetHighLightColor()
    {
        if (_controller)
        {
            return _controller.GetOutlineColor();
        }
        return Color.white;
    }
    public void ChangeHighLightColor(Color color)
    {
        if (_controller)
        {
            _controller.SetOutlineColor(color);
        }
        if (_childrenControllers == null)
            return;

        foreach (var item in _childrenControllers)
        {
            item.SetOutlineColor(color);
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

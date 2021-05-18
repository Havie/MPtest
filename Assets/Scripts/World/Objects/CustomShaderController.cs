#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class CustomShaderController : MonoBehaviour
{
    static string DEFAULT_COLOR = "_Color";
    static string OUTLINE_COLOR = "_OutlineColor";

    MeshRenderer _meshRenderer;
    /************************************************************************************************************************/


    protected virtual void Awake()
    {
        _meshRenderer = this.GetComponent<MeshRenderer>();
    }

    public void ChangeMaterialColor(float opacity)
    {
        UpdateJoseMaterialAlpha(opacity, DEFAULT_COLOR);
    }
    public void Highlight(bool cond)
    {
        SetMaterialHighlighted(cond);
    }
    public void SetOutlineColor(Color c)
    {
        UpdateJoseMaterialColor(c, OUTLINE_COLOR);
    }

    public void SetOutlineIntensity(float intensity)
    {
        UpdateJoseMaterialAlpha(intensity, OUTLINE_COLOR);
    }
    public float GetOutlineIntensity()
    {
        //Unsure if this is as JOSE intended?
        var mat = GetMaterial();
        return mat.GetColor(OUTLINE_COLOR).a;
    }
    public Color GetOutlineColor()
    {
        var mat = GetMaterial();
        return mat.GetColor(OUTLINE_COLOR);
    }

    /************************************************************************************************************************/
    private Material GetMaterial()
    {
        var mat = _meshRenderer.material;
        return mat;
    }
    private void UpdateJoseMaterialColor(Color c, string key)
    {
        var mat = GetMaterial();
        mat.SetColor(key, c);
    }

    private void UpdateJoseMaterialAlpha(float opacity, string key)
    {
        var mat = GetMaterial();
        Color old = mat.GetColor(key);
        old.a = opacity;
        mat.SetColor(key, old);
    }

    private void SetMaterialHighlighted(bool cond)
    {
        if (cond)
        {
            Color currColor = GetMaterial().GetColor(DEFAULT_COLOR);
            bool isCurrentlyTransparent = currColor.a !=1;
           ///We might want to set DEFAULT_COLOR to whatever the current opacity is?
           ///Or just not alter it at all, will revist later
            if (isCurrentlyTransparent)
            {
                UpdateJoseMaterialAlpha( 0.5f, DEFAULT_COLOR);
                UpdateJoseMaterialAlpha( 1, OUTLINE_COLOR);
            }
            else //Not transparent
            {
                UpdateJoseMaterialAlpha( 1f, DEFAULT_COLOR);
                UpdateJoseMaterialAlpha( 1, OUTLINE_COLOR);
            }
        }
        else
        {
            ///Could also do a lamda above for if float amnt = cond ? 1 : 0
            UpdateJoseMaterialAlpha(0, OUTLINE_COLOR);
        }
    }
}

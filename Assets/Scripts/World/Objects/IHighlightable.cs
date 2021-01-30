using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHighlightable : IInteractable
{

    bool IsHighlighted();

    void SetHighlighted(bool cond);

    void ChangeHighlightAmount(float amnt);

    float GetHighlightIntensity();
    Color GetHighLightColor();
    void ChangeHighLightColor(Color color);

    void HandleHighlightPreview();


    void CancelHighLightPreview();



}

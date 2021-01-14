using UnityEngine;


public interface IInteractable 
{
    GameObject GetGameObject();

    Transform GetParent();

    Transform Transform();


    void OnInteract();




    void HandleInteractionTime(float time);

    bool IsHighlighted();

    void SetHighlighted(bool cond);

    void ChangeHighlightAmount(float amnt);



}

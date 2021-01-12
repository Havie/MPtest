using UnityEngine;


public interface IInteractable 
{
    GameObject GetGameObject();

    Transform GetParent();

    Transform Transform();


    void OnInteract();

    void HandleInteractionTime(float time);

    void OnFollowInput(Vector3 worldPos);

    Vector2 OnRotate(Vector3 dot);

    bool OutOfBounds();

    bool IsPickedUp();

    bool IsHighlighted();

    void SetHighlighted(bool cond);

    void ChangeHighlightAmount(float amnt);


    ///Probably need to be on a new class:

    void SetHandPreviewingMode(bool cond);

    void ChangeAppearanceMoving();

    void ChangeAppearanceNormal();

    void ChangeAppearanceHidden(bool cond);
    void ResetHittingTable();

    void SetResetOnNextChange();
}

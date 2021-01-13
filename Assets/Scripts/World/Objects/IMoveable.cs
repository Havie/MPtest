using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable : IInteractable
{
    void OnFollowInput(Vector3 worldPos);

    Vector2 OnRotate(Vector3 dot);

    bool OutOfBounds();

    bool IsPickedUp();

    ///Probably need to be on a new class:

    void SetHandPreviewingMode(bool cond);

    void ChangeAppearanceMoving();

    void ChangeAppearanceNormal();

    void ChangeAppearanceHidden(bool cond);
    void ResetHittingTable();

    void SetResetOnNextChange();
}

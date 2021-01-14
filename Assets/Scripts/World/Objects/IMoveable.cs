using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMoveable : IInteractable
{
    void OnFollowInput(Vector3 worldPos);

    Vector2 OnRotate(Vector3 dot);

    void AllowFollow();

    bool OutOfBounds();

    void SetResetOnNextChange();

    void ResetPosition();

    bool IsPickedUp();

    void ChangeAppearanceMoving();

    void ChangeAppearanceNormal();



}

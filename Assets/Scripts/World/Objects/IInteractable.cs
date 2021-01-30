using UnityEngine;


public interface IInteractable 
{
    GameObject GetGameObject();

    Transform GetParent();

    Transform Transform();


    void OnInteract();

    void HandleInteractionTime(float time);


}

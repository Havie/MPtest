using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    ///IInteractable
    public GameObject GetGameObject() => gameObject;
    public Transform GetParent() => this.transform.parent;
    public Transform Transform() => this.transform;
    public virtual void OnInteract()
    {
        ///Nothing really happens we click this object?
    }
    public virtual void HandleInteractionTime(float time){ }
}

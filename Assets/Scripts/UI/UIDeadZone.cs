using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDeadZone : MonoBehaviour
{
    [Header("Make Sure the Image component of the deadzone is on but alpha is 0, otherwise it wont raycast")]
    [Tooltip("The location where an item will reset to if placed in a deadzone")]
    [SerializeField] Transform _safePlace;

   public Transform GetSafePosition { get; private set; }

    void Start()
    {
        if (_safePlace)
            GetSafePosition = _safePlace.transform;
        else
            UIManager.instance.DebugLogWarning($"saFeplace not set up for {this.gameObject.transform.parent.gameObject.name}");
    }
}

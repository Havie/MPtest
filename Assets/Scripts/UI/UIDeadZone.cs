using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIDeadZone : MonoBehaviour
{
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

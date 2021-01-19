using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWorldObjectFollow : MonoBehaviour
{
    [SerializeField] Transform _worldObjectToFollow;
    [SerializeField] UserInputManager _inputManager;



    void Start()
    {
        if (_inputManager == null)
            _inputManager = FindObjectOfType<UserInputManager>();
        if (_inputManager == null)
            Debug.LogWarning($"Cant find UserInputManager for {this.gameObject.name}");
    }

    void Update()
    {
        if(_inputManager)
        {
            transform.position = GetCorrectPosition();
        }
    }


    public Vector3 GetCorrectPosition()
    {
        return _inputManager.WorldToScreenPoint(_worldObjectToFollow.position);
    }

}

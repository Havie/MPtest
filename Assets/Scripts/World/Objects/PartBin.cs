using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartBin : MonoBehaviour, IMoveable
{
    [SerializeField] Transform _mTransform;
    [SerializeField] GameObject _self;
    [SerializeField] Transform _parent;

    private bool _canFollow;

    private bool _resetOnChange;

    private void Follow(Vector3 toLoc)
    {
        //Debug.Log($"Trying to tell bluebin to follow");
        if (_canFollow)
        {
            Vector3 onlyHoriz = new Vector3(toLoc.x, _mTransform.position.y, toLoc.z);
            _mTransform.position = Vector3.Lerp(_mTransform.position, onlyHoriz, 0.5f);
        }
        else
            Debug.LogWarning("cant follow");

    }

    #region Interface
    public Transform Transform() => _mTransform;
    public GameObject GetGameObject() => _self;
    public Transform GetParent() => _parent;



    public void OnInteract()
    {
        //Debug.Log($"Interacted with {_self.name}");
    }

    public void OnFollowInput(Vector3 worldPos) {  Follow(worldPos); }


    public Vector2 OnRotate(Vector3 dot)  { return  Vector2.zero;  } ///Dont do 


    public bool OutOfBounds()
    {
        ///TODO figure out where it shudnt go
        return false;
    }

    public void SetResetOnNextChange()
    {
        _resetOnChange = true;
    }

    public void ResetPosition()
    {
        var mpos = transform.position;
        transform.position = new Vector3(mpos.x, mpos.y, mpos.z); ///Will have to change to be left/right scene bounds
    }
    public void AllowFollow()
    {
        _canFollow = true;
    }


    public void HandleInteractionTime(float time)
    {
       
    }

    public void ChangeAppearanceMoving()
    {
      
    }

    public void ChangeAppearanceNormal()
    {
       
    }


    public void SetHighlighted(bool cond)
    {
       
    }
    public bool IsHighlighted()
    {
        return false;
    }

    public void ChangeHighlightAmount(float amnt)
    {
       
    }



    public bool IsPickedUp()
    {
        return false;
    }




    #endregion

}

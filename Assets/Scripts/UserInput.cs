using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserInput : MonoBehaviour
{
    private bool _mbDown;
    private Vector3 _lastPos;
    private ObjectController _currentSelection;

    // Update is called once per frame
    void LateUpdate()
    {

        if(_mbDown)
        {
            if(Input.GetMouseButtonUp(0))
            {
                _mbDown = false;
                return;
            }

            //Tell the Object our movement?
            if(_currentSelection)
            {
                _currentSelection.DoRotation(Input.mousePosition- _lastPos );
                _lastPos = Input.mousePosition;
            }
        }
        else if(Input.GetMouseButtonDown(0))
        {
            _mbDown = true;
            _lastPos = Input.mousePosition;
            _currentSelection = CheckClick(_lastPos);
           
            //Any need to tell the obj its selected?
        }
    }



    public ObjectController CheckClick(Vector3 pos)
    {
        var ray = Camera.main.ScreenPointToRay(pos);
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            return (hit.transform.gameObject.GetComponent<ObjectController>())
;       }

        return null;

    }
}

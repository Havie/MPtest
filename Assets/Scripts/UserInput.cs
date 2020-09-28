using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{
    private bool _mbDown;   //LMB
    private bool _mbDown1;  //RMB
    private bool _mbDown2;  //middle mouse 
    private Vector3 _lastPos;
    private Vector3 _mOffset;
    private UIInventorySlot _lastSlot;

    private BuildableObject _buildableObject;
    private ObjectController _currentSelection;


    //UI
    [SerializeField]GraphicRaycaster _Raycaster;
    PointerEventData _PointerEventData;
    EventSystem _EventSystem;


    void Start()
    {
        //Fetch the Event System from the Scene
        _EventSystem = GameObject.FindObjectOfType<EventSystem>();
        _buildableObject = GameObject.FindObjectOfType<BuildableObject>();
    }


    // Update is called once per frame
    void LateUpdate()
    {

        if (!CheckRotation())
        {
            if(CheckDisplacement())
            {
                //Check if were over a invSlot
                UIInventorySlot slot = CheckRayCastForUI();
               // bool test = slot != null;
               // Debug.Log("SLOT = " + test);
                if (slot != null)
                {
                    if (!slot.GetInUse())
                    {
                        slot.SetLarger();
                        slot.PreviewSlot(_buildableObject.GetCurrentSprite());
                        _currentSelection.GetComponent<MeshRenderer>().enabled = false;
                        if (slot != _lastSlot && _lastSlot!=null)
                            _lastSlot.RestoreDefault();
                        _lastSlot = slot;
                    }
                }
                else if(_lastSlot != null)
                     ResetObjectAndSlot();
            }
        }
    }
    private void ResetObjectAndSlot()
    {
        _lastSlot.RestoreDefault();
        _currentSelection.GetComponent<MeshRenderer>().enabled = true;
        _lastSlot = null;
    }

    /** Player is rotating the object in the scene*/ 
    public bool CheckRotation()
    {
        if (_mbDown)
        {
            if (Input.GetMouseButtonUp(0))
            {
                _mbDown = false;
                return false;
            }

            //Tell the Object our movement?
            if (_currentSelection)
            {
                _currentSelection.DoRotation(Input.mousePosition - _lastPos);
                _lastPos = Input.mousePosition;
                return true;
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            _mbDown = true;
            _lastPos = Input.mousePosition;
            _currentSelection = CheckClick(_lastPos);

            //Any need to tell the obj its selected?
            return true;
        }

        return false;
    }

    /** Player is moving an object to or from inventory slot*/ 
    public bool CheckDisplacement()
    {
        if (_mbDown2)
        {
            if (Input.GetMouseButtonUp(2))
            {
                _mbDown2 = false;
                if(_currentSelection)
                {
                    //Check if landed in invSlot
                    UIInventorySlot slot = CheckRayCastForUI();
                    if (slot != null)
                    {
                        Debug.Log("FOUND UI SLOT");
                        slot.SetNormal();
                        slot.AssignItem(_buildableObject.GetCurrentSprite(), (int)_buildableObject._mlvl);
                        Destroy(_currentSelection);
                    }
                    else
                    {
                        //else reset size and Pos
                        _currentSelection.GetComponent<MeshRenderer>().enabled = true;
                        _currentSelection.ChangeApperanceStill();
                        _currentSelection.transform.position = Vector3.zero;
                    }
                }
                return false;
            }

            //Tell the Object our movement?
            if (_currentSelection)
            {
                float zCoord = Camera.main.WorldToScreenPoint(_currentSelection.transform.position).z;
                 Vector3 worldLoc = GetMouseWorldPos(zCoord);
                 _currentSelection.Follow(new Vector3(worldLoc.x, worldLoc.y, _currentSelection.transform.position.z) + _mOffset);
                 return true;
              
            }

        }
        else if (Input.GetMouseButtonDown(2))
        {
            _mbDown2 = true;
            _currentSelection = CheckClick(Input.mousePosition);
            if (_currentSelection)
            {
                _currentSelection.ChangeApperanceMoving();
                float zCoord = Camera.main.WorldToScreenPoint(_currentSelection.transform.position).z;
                _mOffset = _currentSelection.transform.position - GetMouseWorldPos(zCoord);
            }
                
            //Any need to tell the obj its selected?
            return true;
        }

        return false;
    }

    private Vector3 GetMouseWorldPos(float zLoc)
    {
        Vector2 mousepos = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(new Vector3(mousepos.x, mousepos.y, zLoc));
    }

    public ObjectController CheckClick(Vector3 pos)
    {
        var ray = Camera.main.ScreenPointToRay(pos);
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            return (hit.transform.gameObject.GetComponent<ObjectController>());
        }

        return null;

    }

    public bool CheckRayCastForUI(Vector3 pos)
    {
        //BROKEN GOING TO NEED TO FIX THIS FOR TOUCH
        //https://answers.unity.com/questions/979726/touch-to-ray-on-canvas.html
        var ray = Camera.main.ScreenPointToRay(pos);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach(var h in hits)
        {
            Debug.Log("hit =" + h.transform.gameObject);
            UIInventorySlot slot = h.transform.GetComponent<UIInventorySlot>();
            if (slot)
                return true;
        }


        return false;
    }

    public UIInventorySlot CheckRayCastForUI()
    {
        //Set up the new Pointer Event
         _PointerEventData = new PointerEventData(_EventSystem);
        //Set the Pointer Event Position to that of the mouse position
         _PointerEventData.position = Input.mousePosition; //Maybe I can use touch input last known

        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        _Raycaster.Raycast(_PointerEventData, results);

        //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
        foreach (RaycastResult result in results)
        {
            //Debug.Log("hit =" + result.gameObject);
            UIInventorySlot slot = result.gameObject.transform.GetComponent<UIInventorySlot>();
            if (slot)
                return slot;
        }

        return null;
    }
}

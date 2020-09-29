using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{
    public enum eState { FREE, ROTATION, DISPLACEMENT, UI };
    public eState _state;
    private bool _IsMobileMode;

    private float _pressTimeCURR = 0;
    private float _pressTimeMAX = 1.2f;
    private float _holdLeniency = 1.5f;
    private Vector3 _inputPos; //current input loc
    private Vector3 _lastPos; //prior input loc
    private Vector3 _mOffset; //distance between obj in world and camera
    private UIInventorySlot _lastSlot;
    private ObjectController _currentSelection;


    //UI
    [SerializeField] GraphicRaycaster _Raycaster;
    PointerEventData _PointerEventData;
    EventSystem _EventSystem;

    private void Awake()
    {
        _IsMobileMode = Application.isMobilePlatform;

    }
    void Start()
    {
        //Fetch the Event System from the Scene
        _EventSystem = GameObject.FindObjectOfType<EventSystem>();
    }


    // Update is called once per frame
    void LateUpdate()
    {


        switch (_state)
        {
            case eState.FREE:
                {
                    CheckFree();
                    break;
                }
            case eState.ROTATION:
                {
                    CheckRotation();
                    break;
                }
            case eState.DISPLACEMENT:
                {
                    CheckDisplacement();
                    break;
                }
            case eState.UI:
                {
                    CheckUI();
                    break;
                }
        }




    }

    public bool InputDown()
    {
        if (!_IsMobileMode)
        {
            _inputPos = Input.mousePosition;
            return Input.GetMouseButton(0);
        }
        else
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                bool touching = touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
                _inputPos = touch.position;
                return touching;
            }
            else
            {
                _inputPos = Vector3.zero; // will this work?
                return false;
            }
        }
    }

    /** Player is pressing to begin interaction with an obj or UI item */
    public bool CheckFree()
    {
        //press down 
        if (InputDown())
        {
            _lastPos = _inputPos;
            _currentSelection = CheckForObjectAtLoc(_lastPos);
            _pressTimeCURR = 0;
            if (_currentSelection)         //if you get an obj do rotation
            {
               // Debug.Log("CURR SELC= " + _currentSelection.gameObject);
                _state = eState.ROTATION;
            }
            else    //if u get UI do UI 
            {
                var slot = CheckRayCastForUI();
                if (slot != null)
                {
                    if (slot.GetInUse())
                    {
                        _state = eState.UI;
                    }
                }
            }

        }






        return false;
    }

    /** Player is rotating the object in the scene or pressing and holding to begin displacement */
    public bool CheckRotation()
    {
        if (InputDown() && _currentSelection)
        {
            //if no movement increment time 
            float dis = Vector3.Distance(_inputPos, _lastPos);
            if (dis < _holdLeniency)
                _pressTimeCURR += Time.deltaTime;
            else
                _pressTimeCURR = 0;

            //if time>max do displacement
            if (_pressTimeCURR >= _pressTimeMAX)
            {
                _currentSelection = CheckForObjectAtLoc(_inputPos);
                if (_currentSelection)
                {
                    _currentSelection.ChangeApperanceMoving();
                    float zCoord = Camera.main.WorldToScreenPoint(_currentSelection.transform.position).z;
                    _mOffset = _currentSelection.transform.position - GetInputWorldPos(zCoord);
                }
                _state = eState.DISPLACEMENT;
            }
            else //Do rotation
            {
                _currentSelection.DoRotation(_inputPos - _lastPos);
                _lastPos = _inputPos;
                return true;
            }


        }
        else
            _state = eState.FREE;

        return false;

    }

    /** Player is moving an object to or from inventory slot*/
    public bool CheckDisplacement()
    {

        UIInventorySlot slot = CheckRayCastForUI();
        if (InputDown() && _currentSelection)
        {
            float zCoord = Camera.main.WorldToScreenPoint(_currentSelection.transform.position).z;
            Vector3 worldLoc = GetInputWorldPos(zCoord);
            _currentSelection.Follow(new Vector3(worldLoc.x, worldLoc.y, _currentSelection.transform.position.z) + _mOffset);

            if (slot != null)
            {
                if (!slot.GetInUse())
                {
                    slot.PreviewSlot(BuildableObject.Instance.GetCurrentSprite());
                    _currentSelection.GetComponent<MeshRenderer>().enabled = false;
                    if (slot != _lastSlot && _lastSlot != null)
                        _lastSlot.RestoreDefault();
                    _lastSlot = slot;
                }
            }
            else if (_lastSlot != null)
                ResetObjectAndSlot();
        }
        else //Input UP
        {
            if (_currentSelection)
            {
                if (slot != null)
                {
                    Debug.Log($"FOUND UI SLOT {slot.name}");
                    slot.SetNormal();
                    slot.AssignItem(BuildableObject.Instance.GetCurrentSprite(), (int)BuildableObject.Instance._mlvl);
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
            _state = eState.FREE;
        }

        return false;
    }

    public bool CheckUI()
    {
        if (InputDown())
        {
            //If found slot in use 
            //spawn obj and go to displacement 
            var slot = CheckRayCastForUI();
            if (slot)
            {
                Debug.LogWarning($"Slot found= {slot.name}");
                int itemID = slot.GetItemID();
                slot.RestoreDefault();
                float zCoord = Camera.main.WorldToScreenPoint(slot.transform.position).z;
                var obj = BuildableObject.Instance.SpawnObject(itemID, GetInputWorldPos(zCoord)).GetComponent<ObjectController>();
                _currentSelection = obj;
                if (_currentSelection)
                {
                    Debug.Log($"OBJ loc {obj.transform.position}");
                    _currentSelection.ChangeApperanceMoving();
                    _mOffset = _currentSelection.transform.position - GetInputWorldPos(zCoord);
                    _state = eState.DISPLACEMENT;
                }

                else
                    Debug.LogWarning("This happened?1");

            }
            else
                Debug.LogWarning("This happened?2");
        }
        else
            _state = eState.FREE;

        return false;
    }

    private void ResetObjectAndSlot()
    {
        _lastSlot.RestoreDefault();
        _currentSelection.GetComponent<MeshRenderer>().enabled = true;
        _lastSlot = null;
    }

    private Vector3 GetInputWorldPos(float zLoc)
    {
        return Camera.main.ScreenToWorldPoint(new Vector3(_inputPos.x, _inputPos.y, zLoc));
    }

    public ObjectController CheckForObjectAtLoc(Vector3 pos)
    {
        var ray = Camera.main.ScreenPointToRay(pos);
        if (Physics.Raycast(ray, out RaycastHit hit))
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
        foreach (var h in hits)
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

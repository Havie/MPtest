using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{
    public static UserInput Instance { get; private set; }

    public enum eState { FREE, ROTATION, DISPLACEMENT, UI, PREVIEWCONSTRUCTION };
    public eState _state { get; private set; }
    public  bool _IsMobileMode { get; private set; }
    public Camera _mainCamera { get; private set; }

    public ObjectController _currentSelection { get; private set; }

    private float _pressTimeCURR = 0;
    private float _pressTimeMAX = 1.2f;
    private float _holdLeniency = 1.5f;
    private Vector3 _inputPos; //current input loc
    private Vector3 _lastPos; //prior input loc
    private Vector3 _mOffset; //distance between obj in world and camera
    private UIInventorySlot _lastSlot;

    private Vector3 _objStartPos;
    private Quaternion _objStartRot;
    //UI
    [SerializeField] GraphicRaycaster _Raycaster;
    PointerEventData _PointerEventData;
    EventSystem _EventSystem;


    private int _tmpZfix = -9;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        _IsMobileMode = Application.isMobilePlatform;

    }
    void Start()
    {
        //Fetch the Event System from the Scene
        _EventSystem = GameObject.FindObjectOfType<EventSystem>();
        _mainCamera = Camera.main;
    }


    // Update is called once per frame
    void FixedUpdate()
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
            case eState.PREVIEWCONSTRUCTION:
                {
                    CheckPreviewConstruction();
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
            //Somehow this is coming back null? fix is to toggle on and off the box collider on obj in scene? wth
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
                    float zCoord = _mainCamera.WorldToScreenPoint(_currentSelection.transform.position).z;
                    _mOffset = _currentSelection.transform.position - GetInputWorldPos(zCoord);
                    _objStartPos = _currentSelection.transform.position;
                    _objStartRot = _currentSelection.transform.rotation;
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
        if (InputDown())
        {
            if (_currentSelection)
            {
                Vector3 worldLoc = GetCurrentWorldLocBasedOnMouse(_currentSelection.transform);
                _currentSelection.Follow(worldLoc + _mOffset);

                if (slot != null) //we are hovering over a slot 
                {
                    if (!slot.GetInUse())
                    {
                        //Debug.Log($"trying to preview for itemID {(int)_currentSelection._myID}");
                        slot.PreviewSlot(BuildableObject.Instance.GetSpriteByID((int)_currentSelection._myID));
                        _currentSelection.ChangeAppearanceHidden();
                        if (slot != _lastSlot && _lastSlot != null)
                            _lastSlot.UndoPreview();
                        _lastSlot = slot;
                    }
                }
                else if (_lastSlot)
                    ResetObjectAndSlot();

                if (PreviewManager._inPreview)
                {
                    _state = eState.PREVIEWCONSTRUCTION;
                }
            }
        }
        else //Input UP
        {
            if (_currentSelection)
            {
                bool assigned = false;
                if (slot != null)
                {
                    //Debug.Log($"FOUND UI SLOT {slot.name}");
                    slot.SetNormal();
                    assigned= slot.AssignItem((int)_currentSelection._myID, 1);
                    if(assigned)
                        Destroy(_currentSelection.gameObject);
                }
                if (!assigned) 
                {
                    //put it back to where we picked it up 
                    if (slot) // we tried dropping in incompatible slot
                    {
                        _currentSelection.transform.position = _objStartPos;
                        _currentSelection.transform.rotation = _objStartRot;
                    }
                    _currentSelection.ChangeApperanceNormal();
                    //Really weird Fix to prevent raycast bug
                    FixRayCastBug();
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
                //Debug.LogWarning($"Slot found= {slot.name}");
                int itemID = slot.GetItemID();
               // Debug.Log($"Removing ItemID{itemID} from {slot.name}");
                slot.RemoveItem();
                Vector3 slotLoc = slot.transform.position;
                slotLoc.z = _tmpZfix; //somehow changing the scale messed things up so we cant use the worldcanvas UIs z Loc, its too far back
                float zCoord = _mainCamera.WorldToScreenPoint(slotLoc).z; // might want to cache the camera someday
                var obj = BuildableObject.Instance.SpawnObject(itemID, GetInputWorldPos(zCoord)).GetComponent<ObjectController>();
                _currentSelection = obj;
                //Debug.Log($"OBJ spawn loc={obj.transform.position}");
                if (_currentSelection)
                {
                    //Debug.Log($"OBJ loc {obj.transform.position}");
                    _currentSelection.ChangeApperanceMoving();
                    _mOffset = _currentSelection.transform.position - GetInputWorldPos(zCoord);
                    _state = eState.DISPLACEMENT;
                    _objStartPos = new Vector3(0, 0, _tmpZfix);
                    _objStartRot = Quaternion.identity;
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


    public bool CheckPreviewConstruction()
    {

        if (InputDown())
        {
            if (_currentSelection)
            {
                Vector3 worldLoc = GetCurrentWorldLocBasedOnMouse(_currentSelection.transform);
                _currentSelection.Follow(worldLoc + _mOffset);
            }

            if (!PreviewManager._inPreview)
                _state = eState.DISPLACEMENT;
        }
        else //Input UP
        {
            if (_currentSelection)
            {
                if (PreviewManager._inPreview)
                    PreviewManager.ConfirmCreation();

                _currentSelection = null;
            }
            _state = eState.FREE;
        }


      

        return false;
    }

    private void ResetObjectAndSlot()
    {
        if (_currentSelection)
            _currentSelection.ChangeApperanceNormal();
        if (_lastSlot)
        {
            _lastSlot.UndoPreview();
            _lastSlot = null;
        }
    }
    private Vector3 GetCurrentWorldLocBasedOnMouse(Transform transform)
    {
        float zCoord = _mainCamera.WorldToScreenPoint(transform.position).z;
        Vector3 worldLoc = GetInputWorldPos(zCoord);
        return new Vector3(worldLoc.x, worldLoc.y, transform.position.z) + _mOffset;
    }

    private Vector3 GetInputWorldPos(float zLoc)
    {
        return _mainCamera.ScreenToWorldPoint(new Vector3(_inputPos.x, _inputPos.y, zLoc));
    }

    public ObjectController CheckForObjectAtLoc(Vector3 pos)
    {
        var ray = _mainCamera.ScreenPointToRay(pos);
        //Debug.DrawRay( ray.origin, ray.direction*1350, Color.red, 5);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            //Debug.Log($"The Hit succeeded @loc:{pos} ,   the hit is: {hit.transform.gameObject}");
            return (hit.transform.gameObject.GetComponent<ObjectController>());
        }
       /* else
        {
            Debug.LogWarning($"The Hit @loc:{pos} somehow missed");
            var bo = GameObject.FindObjectOfType<BuildableObject>();
            if (bo && bo.transform.childCount>0)
            {
                
                var child = bo.transform.GetChild(0);
                var screenloc = _mainCamera.WorldToScreenPoint(child.position);
                Debug.LogError($"{child.gameObject.name} is @loc:{screenloc} , and world pos = {child.transform.position}");
            }

        }*/

        return null;

    }

    /**This is a really weird fix I found to prevent the raycast from missing the box */
    private void FixRayCastBug()
    {
        if(_currentSelection)
        {
            var box = _currentSelection.GetComponent<Collider>();
            box.enabled = false;
            _currentSelection = null;
            box.enabled = true;
        }
    }

    public bool CheckRayCastForUI(Vector3 pos)
    {
        //BROKEN GOING TO NEED TO FIX THIS FOR TOUCH
        //https://answers.unity.com/questions/979726/touch-to-ray-on-canvas.html
        var ray = _mainCamera.ScreenPointToRay(pos);
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

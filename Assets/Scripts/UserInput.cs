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

    //Actions
    private Vector2 _rotationAmount;

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

       /* var V1 = GetInputWorldPos(_tmpZfix);
       Debug.LogError($"(1)Mouse::{V1} + mOffset={_mOffset} = {V1 + _mOffset}");
        if (_currentSelection)

        {
            var v2 = GetCurrentWorldLocBasedOnMouse(_currentSelection.transform);
            Debug.LogError($"(2)Mouse::{v2} + mOffset={_mOffset} = {v2 + _mOffset}");

        }*/
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
                _inputPos = Vector3.zero; /// will this work?
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

                _rotationAmount =Vector2.zero; ///reset our rotation amount before re-entering
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
                _currentSelection= FindAbsoluteParent(_currentSelection);
                if (_currentSelection)
                {
                    _currentSelection.ChangeApperanceMoving();
                    float zCoord = _mainCamera.WorldToScreenPoint(_currentSelection.transform.position).z;
                    _mOffset = _currentSelection.transform.position - GetInputWorldPos(zCoord);
                    _objStartPos = _currentSelection.transform.position;
                    _objStartRot = _currentSelection.transform.rotation;

                    //only if on table
                    if (_currentSelection.OnTable())
                        ResetObjectOrigin(zCoord);

                    HandManager.PickUpItem(_currentSelection); //might have moved to the wrong spot
                }
                _state = eState.DISPLACEMENT;
            }
            else //Do rotation
            {
                ///Store rotation amount
                Vector3 rotation = _inputPos - _lastPos;
                _rotationAmount += _currentSelection.DoRotation(rotation);
                _lastPos = _inputPos;
                return true;
            }


        }
        else
        {
            if (_currentSelection)
            {
                TryPerformAction(QualityAction.eActionType.ROTATE);
                TryPerformAction(QualityAction.eActionType.TAP);
            }
            _state = eState.FREE;
        }

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
                   // HandManager.DropItem(_currentSelection);
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
                float zCoord = _mainCamera.WorldToScreenPoint(slotLoc).z; 
                var obj = BuildableObject.Instance.SpawnObject(itemID, GetInputWorldPos(zCoord)).GetComponent<ObjectController>();
                _currentSelection = obj;
                HandManager.PickUpItem(_currentSelection);
                //Debug.Log($"OBJ spawn loc={obj.transform.position}");
                if (_currentSelection)
                {
                    //Debug.Log($"OBJ loc {obj.transform.position}");
                    _currentSelection.ChangeApperanceMoving();
                    //_mOffset = _currentSelection.transform.position - GetInputWorldPos(zCoord);
                    _mOffset = Vector3.zero; ///same thing as above because it spawns here so no difference
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

    private void ResetObjectOrigin(float zCoord)
    {
        ///Reset the object to have the right orientation for construction when picked back up
        if (_currentSelection)
        {
            _currentSelection.ChangeApperanceMoving();
            Vector3 mouseLocWorld = GetInputWorldPos(zCoord);
            _objStartPos = new Vector3(mouseLocWorld.x, mouseLocWorld.y, _tmpZfix);
            //Debug.LogWarning($"mouseLocWorld={mouseLocWorld} , _objStartPos={_objStartPos}   _currentSelection.transform.position={_currentSelection.transform.position}");
            _objStartRot = Quaternion.identity;
            _mOffset = Vector3.zero;
            //new
            _currentSelection.transform.position = _objStartPos;
            _currentSelection.transform.rotation = _objStartRot;
            //Start moving the object
            _state = eState.DISPLACEMENT;
            _currentSelection.ResetHittingTable(); // so we can pick it up again
        }
        else
            Debug.LogWarning("This happened?1");
    }

    private void ResetObjectAndSlot()
    {
        if (_currentSelection)
        {
            _currentSelection.ChangeApperanceNormal();
           
        }
        if (_lastSlot)
        {
            _lastSlot.UndoPreview();
            _lastSlot = null;
        }
    }
    private Vector3 GetCurrentWorldLocBasedOnMouse(Transform transform)
    {
        //Debug.Log($"(1) {_inputPos.x},{_inputPos.y}");
        Vector3 screenPtObj = _mainCamera.WorldToScreenPoint(transform.position);
        ///gets the objects Z pos in world for depth
        float zCoord = screenPtObj.z;
        ///gets the world loc based on inputpos and gives it the z depth from the obj
        Vector3 worldLocInput = GetInputWorldPos(zCoord);
        return new Vector3(worldLocInput.x, worldLocInput.y, transform.position.z); 
    }

    private Vector3 GetInputWorldPos(float zLoc)
    {
        return _mainCamera.ScreenToWorldPoint(new Vector3(_inputPos.x, _inputPos.y, zLoc));
    }


    public ObjectController CheckForObjectAtLoc(Vector3 pos)
    {
        var ray = _mainCamera.ScreenPointToRay(pos);
        Debug.DrawRay( ray.origin, ray.direction*1350, Color.red, 5);
        if (Physics.Raycast(ray, out RaycastHit hit)) ///not sure why but i need a RB to raycast, think i would only need a collider??
        {
            //Debug.Log($"Raycast hit:" + (hit.transform.gameObject.GetComponent<ObjectController>()));
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


    #region QualityActions
    private bool TryPerformAction(QualityAction.eActionType type)
    {
        var objectQuality = _currentSelection.GetComponent<ObjectQuality>();
        if (objectQuality != null)
        {
            QualityAction action = new QualityAction(type, _inputPos, _rotationAmount);
            if (objectQuality.PerformAction(action))
                return true;
        }

        return false;
    }

    private ObjectController FindAbsoluteParent(ObjectController startingObj)
    {
        ObjectController parent = startingObj._parent;
        ObjectController child = startingObj;
        while(parent != null)
        {
            child = parent;
            parent = child._parent;
        }

        return child;
    }
    #endregion


    public void InjectItem(int itemID)
    {
       
        var tmp = _mainCamera.WorldToScreenPoint(new Vector3(0,0,_tmpZfix));
        var obj = BuildableObject.Instance.SpawnObject(itemID, GetInputWorldPos(tmp.z)).GetComponent<ObjectController>();

        if (Input.GetMouseButtonDown(0)) //if we wana pick it up , seems t get stuck on rotation but ok
        {
            _currentSelection = obj;
            HandManager.PickUpItem(_currentSelection);
            Debug.Log($"OBJ spawn loc={obj.transform.position}");
            if (_currentSelection)
            {
                _currentSelection.ChangeApperanceMoving();
                //_mOffset = _currentSelection.transform.position - GetInputWorldPos(zCoord);
                _mOffset = Vector3.zero; ///same thing as above because it spawns here so no difference
                _state = eState.DISPLACEMENT;
                _objStartPos = new Vector3(0, 0, _tmpZfix);
                _objStartRot = Quaternion.identity;

                Debug.Log($"Final loc={_currentSelection.transform.position}");
            }
        }
        

    }
}

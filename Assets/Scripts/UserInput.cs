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
    public bool _IsMobileMode { get; private set; }
    public Camera _mainCamera { get; private set; }

    public ObjectController _currentSelection { get; private set; }

    [HideInInspector]  //testing purposes
    public float _pressTimeCURR = 0;
    [HideInInspector]
    public float _pressTimeMAX = 0.75f; ///was 1.2f
    [HideInInspector]
    public float _holdLeniency = 1.5f;
    private Vector3 _inputPos; ///current input loc
    private Vector3 _lastPos; ///prior input loc
    private Vector3 _mOffset; ///distance between obj in world and camera
    private UIInventorySlot _lastSlot;

    [SerializeField] Transform _table;
    private float _tableHeight = -0.45f;
    private bool _justPulledOutOfUI;

    private Vector3 _objStartPos;
    private Quaternion _objStartRot;
    //UI
    [SerializeField] GraphicRaycaster _Raycaster;
    [SerializeField] GraphicRaycaster _RaycasterWorld;
    PointerEventData _PointerEventData;
    PointerEventData _pointerEventDataWorld;
    EventSystem _EventSystem;

    //Actions
    private Vector2 _rotationAmount;

    [HideInInspector]
    public int _tmpZfix = -9;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        _IsMobileMode = Application.isMobilePlatform;

        if (_IsMobileMode)
            _holdLeniency = 5f; ///Touch controls too sensitive

        if (_table == null)
        {
            var table = GameObject.FindGameObjectWithTag("Table").transform;
            if (table)
                _table = table.transform;
            else
                UIManager.instance.DebugLogWarning("Table not set up with tag for UserInput Awake");
        }

        if (_table)
            _tableHeight = _table.position.y;

    }
    void Start()
    {
        //Fetch the Event System from the Scene
        _EventSystem = GameObject.FindObjectOfType<EventSystem>();
        //Set up the new Pointer Event
        _PointerEventData = new PointerEventData(_EventSystem);
       
        _mainCamera = Camera.main;

        if (_Raycaster == null) ///when working between scenes sometimes i forget to set this
            _Raycaster = UIManager.instance._inventoryCanvas.GetComponent<GraphicRaycaster>();
    }


    void FixedUpdate()
    {
        if(Input.GetMouseButtonDown(1))
            Debug.Log($"Mouse={_inputPos}");

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
        if (InputDown())
        {
            _lastPos = _inputPos;
            _currentSelection = CheckForObjectAtLoc(_lastPos);
            _pressTimeCURR = 0;
            if (_currentSelection) ///if you get an obj do rotation
            {
                // Debug.Log("CURR SELC= " + _currentSelection.gameObject);    

                _rotationAmount = Vector2.zero; ///reset our rotation amount before re-entering
                _state = eState.ROTATION;

            }
            else ///if u get UI do UI 
            {
                var slot = RayCastForInvSlot();
                if (slot != null)
                {
                    if (slot.GetInUse())
                    {
                        _state = eState.UI;
                    }
                }
                else
                {
                   var instructions= RayCastForInstructions();
                    if (instructions)
                        instructions.InstructionsClicked();
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
            ///if no movement increment time 
            float dis = Vector3.Distance(_inputPos, _lastPos);
            if (dis < _holdLeniency)
            {
                _pressTimeCURR += Time.deltaTime;

                if (_pressTimeCURR > _pressTimeMAX / 10) ///dont show this instantly 10%filled
                {
                    ///Show the UI wheel for our TouchPhase 
                    UIManager.instance.ShowTouchDisplay(_pressTimeCURR, _pressTimeMAX,
                         new Vector3(_inputPos.x, _inputPos.y, _inputPos.z)
                         );

                    ///Cap our mats transparency fade to 0.5f
                    float changeVal = (_pressTimeMAX - _pressTimeCURR) / _pressTimeMAX;
                    changeVal = Mathf.Lerp(1, changeVal, 0.5f);
                    _currentSelection.ChangeMaterialColor(changeVal);

                    //Vibration.Vibrate(100); ///No haptic feedback on WiFi version of TabS5E :(
                }
            }
            else
            {
                _pressTimeCURR = 0;
                UIManager.instance.HideTouchDisplay();
            }

            ///if holding down do displacement
            if (_pressTimeCURR >= _pressTimeMAX)
            {
                UIManager.instance.HideTouchDisplay();

                _currentSelection = CheckForObjectAtLoc(_inputPos);
                _currentSelection = FindAbsoluteParent(_currentSelection);
                if (_currentSelection)
                {
                    _currentSelection.ChangeAppearanceMoving();
                    float zCoord = _mainCamera.WorldToScreenPoint(_currentSelection.transform.position).z;
                    _mOffset = _currentSelection.transform.position - GetInputWorldPos(zCoord);
                    _objStartPos = _currentSelection.transform.position;
                    _objStartRot = _currentSelection.transform.rotation;

                    ///only if on table
                   // if (_currentSelection.SetOnTable())  
                     if (_currentSelection._hittingTable)
                        ResetObjectOrigin(zCoord);

                    HandManager.PickUpItem(_currentSelection); //might have moved to the wrong spot
                }
                _state = eState.DISPLACEMENT;
            }
            else ///Do rotation
            {
                ///Store rotation amount
                Vector3 rotation = _inputPos - _lastPos;
                _rotationAmount += _currentSelection.DoRotation(rotation);
                _lastPos = _inputPos;
                HandleHighlightPreview();
                return true;
            }


        }
        else /// this input UP
        {
            if (_currentSelection)
            {
                TryPerformAction(QualityAction.eActionType.ROTATE);
                TryPerformAction(QualityAction.eActionType.TAP);
                CancelHighLightPreview();

                UIManager.instance.HideTouchDisplay();
                _currentSelection.ChangeMaterialColor(1);
            }
            _state = eState.FREE;
        }

        return false;

    }

    private void HandleHighlightPreview()
    {
        ///if its a current item being held in hand , return
        if (_currentSelection.IsPickedUp)
            return;

        ///if its not highlighting turn it on 
        if (!_currentSelection.IsHighlighted)
        {
            _currentSelection.SetHighlighted(true);
            _currentSelection.ChangeHighlightAmount(0);
        }

        HandManager.StartToHandleIntensityChange(_currentSelection);

    }

    private void CancelHighLightPreview()
    {
        _currentSelection.HandPreviewingMode = false;

        if (_currentSelection.IsPickedUp)
            return;

        HandManager.CancelIntensityChangePreview();

        if (_currentSelection.IsHighlighted)
            _currentSelection.SetHighlighted(false);


    }

    /** Player is moving an object to or from inventory slot*/
    public bool CheckDisplacement()
    {

        UIInventorySlot slot = RayCastForInvSlot();
        if (InputDown())
        {
            if (_currentSelection)
            {
                Vector3 worldLoc = GetCurrentWorldLocBasedOnMouse(_currentSelection.transform);
                _currentSelection.Follow(worldLoc + _mOffset);

                if (slot != null) ///we are hovering over a slot 
                {
                    if (!slot.GetInUse())
                    {
                        ///The slot can accept this item
                       if (slot.PreviewSlot(BuildableObject.Instance.GetSpriteByID((int)_currentSelection._myID)))
                        {
                            _currentSelection.ChangeAppearanceHidden(true);
                            UIManager.instance.ShowPreviewInvSlot(false, _inputPos, null);
                        }
                        else ///the slot can not accept this item so continue to show the dummy preview
                            ShowDummyPreviewSlot();

                        if (slot != _lastSlot && _lastSlot != null)
                            _lastSlot.UndoPreview();
                        _lastSlot = slot;
                    }
                    else
                    {
                        ///show a preview of just the icon floating around
                        if (slot != _lastSlot && _lastSlot != null)
                            _lastSlot.UndoPreview();
                        _lastSlot = slot;
                         ShowDummyPreviewSlot();
                    }
                }
                else
                    ResetObjectAndSlot();

                if (PreviewManager._inPreview)
                {
                    _state = eState.PREVIEWCONSTRUCTION;
                }
            }
        }
        else ///Input UP
        {
            if (_currentSelection)
            {
                bool assigned = false;
                if (slot != null)
                {
                    //Debug.Log($"FOUND UI SLOT {slot.name}");
                    slot.SetNormal();
                    assigned = slot.AssignItem(_currentSelection, 1);
                    if (assigned)
                        Destroy(_currentSelection.gameObject);
                }
                if (!assigned)
                {
                    ///put it back to where we picked it up 
                    if (slot) // we tried dropping in incompatible slot
                    {
                        _currentSelection.transform.position = _objStartPos;
                        _currentSelection.transform.rotation = _objStartRot;

                    }
                    else 
                    {
                        var dz = CheckRayCastForDeadZone();
                        if(dz)
                        {
                            ///If the item is dropped in a deadzone, reset it to a safe place
                            _currentSelection.transform.position = GetCurrentWorldLocBasedOnPos(dz.GetSafePosition);
                        }
                        else
                        {
                            ///Check were not below the table
                            if (_currentSelection._hittingTable)
                                _currentSelection.SetResetOnNextChange();

                               // Debug.Log($"curr: {_currentSelection.transform.position.y} vs table {_tableHeight}");

                        }
                    }
                    _currentSelection.ChangeAppearanceNormal();
                    // HandManager.DropItem(_currentSelection);
                    //Really weird Fix to prevent raycast bug
                    FixRayCastBug();
                }
            }

            _justPulledOutOfUI = false;
            _state = eState.FREE;
        }

        return false;
    }



    public bool CheckUI()
    {
        if (InputDown())
        {
            ///If found slot in use spawn obj and go to displacement 
            var slot = RayCastForInvSlot();
            if (slot)
            {
                //Debug.LogWarning($"Slot found= {slot.name}");
                int itemID = slot.GetItemID();
                // Debug.Log($"Removing ItemID{itemID} from {slot.name}");
                var qualityList = RebuildQualities(slot.Qualities);
                slot.RemoveItem();
                Vector3 slotLoc = slot.transform.position;
                slotLoc.z = _tmpZfix;
                float zCoord = _mainCamera.WorldToScreenPoint(slotLoc).z;
                var obj = BuildableObject.Instance.SpawnObject(itemID, GetInputWorldPos(zCoord), qualityList).GetComponent<ObjectController>();
                _currentSelection = obj;
                HandManager.PickUpItem(_currentSelection);
                //Debug.Log($"OBJ spawn loc={obj.transform.position}");
                if (_currentSelection)
                {
                    _mOffset = Vector3.zero; /// it spawns here so no difference
                    _objStartPos = new Vector3(0, 0, _tmpZfix);
                    _objStartRot = Quaternion.identity;
                    _justPulledOutOfUI = true;
                    _state = eState.DISPLACEMENT;
                    _currentSelection.ChangeAppearanceHidden(true); ///spawn it invisible till were not hovering over UI
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

    private List<ObjectQuality> RebuildQualities(List<ObjectQuality> toCopy)
    {
        List<ObjectQuality> newList = new List<ObjectQuality>();
        if (toCopy != null)
        {
            foreach (var q in toCopy)
                newList.Add(q);
        }

        return newList;
    }

    private void ShowDummyPreviewSlot()
    {
        Sprite img = BuildableObject.Instance.GetSpriteByID((int)_currentSelection._myID);
        _currentSelection.ChangeAppearanceHidden(true);
        UIManager.instance.ShowPreviewInvSlot(true, _inputPos, img);
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
            _currentSelection.ChangeAppearanceMoving();
            Vector3 mouseLocWorld = GetInputWorldPos(zCoord);
            _objStartPos = new Vector3(mouseLocWorld.x, mouseLocWorld.y, _tmpZfix);
            //Debug.LogWarning($"mouseLocWorld={mouseLocWorld} , _objStartPos={_objStartPos}   _currentSelection.transform.position={_currentSelection.transform.position}");
            _objStartRot = Quaternion.identity;
            _mOffset = Vector3.zero;
            ///new
            _currentSelection.transform.position = _objStartPos;
            _currentSelection.transform.rotation = _objStartRot;
            ///Start moving the object
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
            _currentSelection.ChangeAppearanceHidden(false);
        }
        if (_lastSlot)
        {
            _lastSlot.UndoPreview();
            _lastSlot = null;
        }

        UIManager.instance.ShowPreviewInvSlot(false, _inputPos, null);
    }
    private Vector3 GetCurrentWorldLocBasedOnMouse(Transform currSelectionTransform)
    {
        //Debug.Log($"(1) {_inputPos.x},{_inputPos.y}");
        Vector3 screenPtObj = _mainCamera.WorldToScreenPoint(currSelectionTransform.position);
        ///gets the objects Z pos in world for depth
        float zCoord = screenPtObj.z;
        ///gets the world loc based on inputpos and gives it the z depth from the obj
        Vector3 worldLocInput = GetInputWorldPos(zCoord);
        return new Vector3(worldLocInput.x, worldLocInput.y, currSelectionTransform.position.z);
    }
    private Vector3 GetCurrentWorldLocBasedOnPos(Transform safePlaceToGo)
    {
        Vector3 screenPtObj = _mainCamera.WorldToScreenPoint(_currentSelection.transform.position);
        float zCoord = screenPtObj.z;
        ///gets the world loc based on transform and gives it the z depth from the obj
        var v3= _mainCamera.ScreenToWorldPoint(
            new Vector3(safePlaceToGo.position.x, safePlaceToGo.position.y, zCoord)
            );

        v3.z = _currentSelection.transform.position.z;
        return v3;
    }

    private Vector3 GetInputWorldPos(float zLoc)
    {
        return _mainCamera.ScreenToWorldPoint(new Vector3(_inputPos.x, _inputPos.y, zLoc));
    }

    public Vector3 GetScreenPointBasedOnWorldLoc(Vector3 pos)
    {
        return _mainCamera.WorldToScreenPoint(pos);
    }

    public ObjectController CheckForObjectAtLoc(Vector3 pos)
    {
        var ray = _mainCamera.ScreenPointToRay(pos);
        Debug.DrawRay(ray.origin, ray.direction * 1350, Color.red, 5);
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
        if (_currentSelection)
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

    public UIInventorySlot RayCastForInvSlot()
    {
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
    public UIInstructions RayCastForInstructions()
    {
        var ray = _mainCamera.ScreenPointToRay(_inputPos);
        Debug.DrawRay(ray.origin, ray.direction * 1350, Color.green, 5);
        if (Physics.Raycast(ray, out RaycastHit hit)) ///not sure why but i need a RB to raycast, think i would only need a collider??
        {
            //Debug.Log($"Raycast hit:" + (hit.transform.gameObject.GetComponent<ObjectController>()));
            return (hit.transform.gameObject.GetComponent<UIInstructions>());
        }

        return null;
    }

    public UIDeadZone CheckRayCastForDeadZone()
    {
        //Set the Pointer Event Position to that of the mouse position
        _PointerEventData.position = Input.mousePosition; //Maybe I can use touch input last known

        List<RaycastResult> results = new List<RaycastResult>();

        //Raycast using the Graphics Raycaster and mouse click position
        _Raycaster.Raycast(_PointerEventData, results);

        //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
        foreach (RaycastResult result in results)
        {
            //Debug.Log("hit =" + result.gameObject);
            UIDeadZone dz = result.gameObject.transform.GetComponent<UIDeadZone>();
            if (dz)
                return dz;
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
        if (startingObj == null)
        {
            Debug.LogWarning($"Somehow we are passing ina  null startingObj???");
            return null;
        }

        ObjectController parent = startingObj._parent;
        ObjectController child = startingObj;
        while (parent != null)
        {
            child = parent;
            parent = child._parent;
        }

        return child;
    }
    #endregion


    public void InjectItem(int itemID)
    {

        var tmp = _mainCamera.WorldToScreenPoint(new Vector3(0, 0, _tmpZfix));
        var obj = BuildableObject.Instance.SpawnObject(itemID, GetInputWorldPos(tmp.z), null).GetComponent<ObjectController>();

        if (Input.GetMouseButtonDown(0)) //if we wana pick it up , seems t get stuck on rotation but ok
        {
            _currentSelection = obj;
            HandManager.PickUpItem(_currentSelection);
            Debug.Log($"OBJ spawn loc={obj.transform.position}");
            if (_currentSelection)
            {
                _currentSelection.ChangeAppearanceMoving();
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

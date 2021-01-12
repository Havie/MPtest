﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UserInput : MonoBehaviour
{
    public static UserInput Instance { get; private set; }

    public bool _IsMobileMode { get; private set; }
    public Camera _mainCamera { get; private set; }

    public ObjectController _currentSelection;

    [HideInInspector]  //testing purposes
    public float _pressTimeCURR = 0;
    public float _pressTimeMAX = 0.55f; ///was 1.2f
    [HideInInspector] ///NB: these are still serialized, need to make private to change 
    public float _holdLeniency = 1.5f;
    public Vector3 _inputPos; ///current input loc
    public Vector3 _lastPos; ///prior input loc
    public Vector3 _mOffset; ///distance between obj in world and camera
    public UIInventorySlot _lastSlot;

    public Transform _table = default;
    //private float _tableHeight = -0.45f;
    //private bool _justPulledOutOfUI= default;

    public Vector3 _objStartPos;
    public Quaternion _objStartRot;
    //UI
    [SerializeField] GraphicRaycaster _Raycaster = default;
    PointerEventData _PointerEventData;
    EventSystem _EventSystem;

    //Actions
    public Vector2 _rotationAmount;

    [HideInInspector]
    public int _tmpZfix = -9;


    #region InputStates
    public InputState _freeState { get; private set; }
    public InputState _rotationState { get; private set; }
    public InputState _displacementState { get; private set; }
    public InputState _uiState { get; private set; }
    public InputState _previewState { get; private set; }
    public InputState _currentState { get; private set; }

    #endregion



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

        //if (_table)
        //    _tableHeight = _table.position.y;

        CreateStates();

    }

    void CreateStates()
    {
        _freeState = new FreeState(this);
        _rotationState = new RotationState(this);
        _displacementState = new DisplacementState(this);
        _uiState = new UIState(this);
        _previewState = new PreviewConstructionState(this);
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

        _currentState = _freeState;
    }

    public void SwitchState(InputState nextState)
    {
        if(_currentState==null || _currentState.CanExitState(nextState))
        {
            _currentState.DisableState();
            _currentState = nextState;
            _currentState.EnableState();
        }    
        
    }

    void Update()
    {

        if (_currentState != null)
            _currentState.Execute();

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

    public void Destroy(ObjectController oc)
    {
        if(oc!=null)
            Destroy(oc.gameObject);
    }



    public Vector3 GetCurrentWorldLocBasedOnMouse(Transform currSelectionTransform)
    {
        //Debug.Log($"(1) {_inputPos.x},{_inputPos.y}");
        Vector3 screenPtObj = _mainCamera.WorldToScreenPoint(currSelectionTransform.position);
        ///gets the objects Z pos in world for depth
        float zCoord = screenPtObj.z;
        ///gets the world loc based on inputpos and gives it the z depth from the obj
        Vector3 worldLocInput = GetInputWorldPos(zCoord);
        return new Vector3(worldLocInput.x, worldLocInput.y, currSelectionTransform.position.z);
    }
    public Vector3 GetCurrentWorldLocBasedOnPos(Transform safePlaceToGo)
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

    public Vector3 GetInputWorldPos(float zLoc)
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
    public void FixRayCastBug()
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
    public bool TryPerformAction(QualityAction.eActionType type)
    {
        var objectQuality = _currentSelection.GetComponent<QualityObject>();
        if (objectQuality != null)
        {
            QualityAction action = new QualityAction(type, _inputPos, _rotationAmount);
            if (objectQuality.PerformAction(action))
                return true;
        }

        return false;
    }

    public ObjectController FindAbsoluteParent(ObjectController startingObj)
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

    public void CheckForSwitch()
    {
        if(_currentSelection)
        {
            var s = _currentSelection.GetComponent<Switch>();
            if(s)
            {
                s.OnPress();
            }
        }
    }

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
                SwitchState(_displacementState);
                _objStartPos = new Vector3(0, 0, _tmpZfix);
                _objStartRot = Quaternion.identity;

                Debug.Log($"Final loc={_currentSelection.transform.position}");
            }
        }


    }
}
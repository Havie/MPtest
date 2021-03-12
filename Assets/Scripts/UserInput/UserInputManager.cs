using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


///Might want to try to make this non-mono and cleanup/remove some dependencies from other classes 
///Could move the Awake/Start Functions into the constructor-they arent doing much


namespace UserInput
{
    public class UserInputManager : MonoBehaviour
    {
        public static UserInputManager Instance { get; private set; }


        [SerializeField] float _pressTimeMAX = 0.40f; ///was 1.2f
        private float _holdLeniency = 1.5f;

        private Vector3 _inputPos;

        ///Cant find a place for -shared between states, dont wana pass everytime
        [HideInInspector]
        public Vector3 _mOffset; ///distance between obj in world and camera
        public Vector3 ObjStartPos { get; private set; }
        public Quaternion ObjStartRot { get; private set; }

        [SerializeField] LayerMask _objectLayer = default;
        //UI
        [SerializeField] GraphicRaycaster _Raycaster = default;
        PointerEventData _PointerEventData;
        EventSystem _EventSystem;
        private Camera _mainCamera;


        #region InputStates
        public InputState _freeState { get; private set; }
        public InputState _rotationState { get; private set; }
        public InputState _displacementState { get; private set; }
        public InputState _uiState { get; private set; }
        public InputState _previewState { get; private set; }
        public InputState _currentState { get; private set; }

        #endregion


        /************************************************************************************************************************/
        //         Init
        /************************************************************************************************************************/
        #region
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(this);

            if (Application.isMobilePlatform)
                _holdLeniency = 5; ///Forgot what I had this set to

            _mainCamera = Camera.main;
            CreateStates();

        }

        void CreateStates()
        {
            ///This is going to need to be abstracted into the IMoveable interface:


            _freeState = new FreeState(this);
            _rotationState = new RotationState(this, _holdLeniency, _pressTimeMAX);
            _displacementState = new DisplacementState(this);
            _uiState = new UIState(this);
            _previewState = new PreviewConstructionState(this);
        }
        void Start()
        {
            ///Fetch the Event System from the Scene
            _EventSystem = GameObject.FindObjectOfType<EventSystem>();
            ///Set up the new Pointer Event
            _PointerEventData = new PointerEventData(_EventSystem);

            ///Might need the networking one from time to time?
            if (_Raycaster == null) ///when working between scenes sometimes i forget to set this
                _Raycaster = UIManagerGame.instance._inventoryCanvas.GetComponent<GraphicRaycaster>();

            _currentState = _freeState;
        }

        #endregion

        /************************************************************************************************************************/
        //        RunTime
        /************************************************************************************************************************/
        #region RunTime
        public void SetInputCommand(InputCommand command)
        {
            _inputPos = command.Position; /// Want to get rid of this but states on enable want this and some helpers
            if (_currentState != null)
                _currentState.Execute(command);
        }
        /************************************************************************************************************************/
        public IInteractable CurrentSelection => _currentState.CurrentSelection;
        public void SwitchState(InputState nextState, IInteractable currentSelection)
        {
            if (_currentState == null || _currentState.CanExitState(nextState))
            {
                _currentState.DisableState();
                _currentState = nextState;
                _currentState.EnableState(currentSelection);
            }

        }
        public void Destroy(IInteractable oc)
        {
            if (oc != null)
                Destroy(oc.GetGameObject());
        }
        public void SetObjectStartPos(Vector3 pos)  { ObjStartPos = pos; }
        public void SetObjectStartRot(Quaternion rot) { ObjStartRot = rot; }
        #endregion
        /************************************************************************************************************************/
        //          HELPERS FOR STATES
        /************************************************************************************************************************/
        #region StateHelpers
        public Vector3 WorldToScreenPoint(Vector3 pos) { return _mainCamera.WorldToScreenPoint(pos); }
        public Vector3 ScreenToWorldPoint(Vector3 pos) { return _mainCamera.ScreenToWorldPoint(pos); }
        public Vector3 GetCurrentWorldLocBasedOnMouse(Transform currSelectionTransform)
        {
            //Debug.Log($"(1) {_inputPos.x},{_inputPos.y}");
            Vector3 screenPtObj = WorldToScreenPoint(currSelectionTransform.position);
            ///gets the objects Z pos in world for depth
            float zCoord = screenPtObj.z;
            ///gets the world loc based on inputpos and gives it the z depth from the obj
            Vector3 worldLocInput = GetInputWorldPos(zCoord);
            // Debug.Log($" {(currSelectionTransform).position} Thinks ScreenSpace:zCoord is : <color=yellow> {zCoord} </color> which becomes :<color=Green> {worldLocInput} </color>");
            return new Vector3(worldLocInput.x, worldLocInput.y, currSelectionTransform.position.z);
        }
        public Vector3 GetCurrentWorldLocBasedOnPos(Transform safePlaceToGo, IInteractable currentSelection)
        {
            Vector3 screenPtObj = WorldToScreenPoint(currentSelection.Transform().position);
            float zCoord = screenPtObj.z;
            ///gets the world loc based on transform and gives it the z depth from the obj
            var v3 = _mainCamera.ScreenToWorldPoint(
                new Vector3(safePlaceToGo.position.x, safePlaceToGo.position.y, zCoord)
                );

            v3.z = currentSelection.Transform().position.z;
            return v3;
        }

        public Vector3 GetInputWorldPos(float zLoc)
        {
            //Debug.Log($" based on zLoc={zLoc} Think we shud spawn at : {_mainCamera.ScreenToWorldPoint(new Vector3(_inputPos.x, _inputPos.y, zLoc))}");
            return ScreenToWorldPoint(new Vector3(_inputPos.x, _inputPos.y, zLoc));
        }

        public IInteractable CheckForObjectAtLoc(Vector3 pos)
        {
            var ray = _mainCamera.ScreenPointToRay(pos);
            Debug.DrawRay(ray.origin, ray.direction * 1350, Color.red, 5);
            if (Physics.Raycast(ray, out RaycastHit hit, 10000, _objectLayer, QueryTriggerInteraction.Collide)) ///Need to set QueryTriggerInteraction.Collide becuz our objs are Triggers
            {
               // Debug.Log($"Raycast hit: {hit.transform.gameObject} ::" + (hit.transform.gameObject.GetComponent<IInteractable>()));
                return (hit.transform.gameObject.GetComponent<IInteractable>());
            }

            return null;

        }

        public IAssignable RayCastForInvSlot()
        {
            //Set the Pointer Event Position to that of the mouse position
            _PointerEventData.position = Input.mousePosition; ///Seems to work for touch input too, idk how

            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            _Raycaster.Raycast(_PointerEventData, results);

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                //Debug.Log("hit =" + result.gameObject);
                IAssignable slot = result.gameObject.transform.GetComponent<IAssignable>();

                if (slot != null)
                    return slot;
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

        #endregion
        /************************************************************************************************************************/
        //         CHEATING / INJECTOR
        /************************************************************************************************************************/
        public void InjectItem(int itemID)
        {

            //var tmp = _mainCamera.WorldToScreenPoint(new Vector3(0, 0, -9));
            //var obj = BuildableObject.Instance.SpawnObject(itemID, GetInputWorldPos(tmp.z), null).GetComponent<ObjectController>();

            //if (Input.GetMouseButtonDown(0)) //if we wana pick it up , seems t get stuck on rotation but ok
            //{
            //    IInteractable currentSelection = obj.GetComponent<IInteractable>();
            //    HandManager.PickUpItem(currentSelection.GetGameObject().GetComponent<ObjectController>());
            //    Debug.Log($"OBJ spawn loc={obj.transform.position}");
            //    if (currentSelection != null)
            //    {
            //        IMoveable moveableObject = currentSelection as IMoveable;
            //        if (moveableObject != null)
            //        {
            //            moveableObject.OnFollowInput(_inputPos);
            //            //_mOffset = _currentSelection.transform.position - GetInputWorldPos(zCoord);
            //            _mOffset = Vector3.zero; ///same thing as above because it spawns here so no difference
            //            SwitchState(_displacementState, currentSelection);
            //            _objStartPos = new Vector3(0, 0, -9);
            //            _objStartRot = Quaternion.identity;

            //            //Debug.Log($"Final loc={_currentSelection.transform.position}");
            //        }
            //    }
            //}


        }
    }
}
using HighlightPlus;
using System.Collections.Generic;
using UnityEditor;

using UnityEngine;

public class ObjectController : HighlightableObject, IConstructable
{

    public ObjectRecord.eItemID _myID;
    ///Rotational / Movement
    public enum eRotationAxis { YAXIS, XAXIS, BOTH, NONE };
    [HideInInspector]
    public eRotationAxis _rotateAroundAxis = eRotationAxis.YAXIS;
    [HideInInspector]
    public bool _canFollow = true; ///will be true for parents, children shouldbe set to false via inspector
    private int _dampening = 10;
    ///effect stuff
    private Vector3 _startSize;
    private MeshRenderer _meshRenderer;
    private List<MeshRenderer> _childrenMeshRenderers;
    ///Components
    private Rigidbody _rb;
    private Collider _collider;
    ///Reset Edge cases
    private bool _hittingTable;
    private bool _isSubObject;
    private bool _isInPreview;
    [HideInInspector]
    public ObjectController _parent;
    ///Hand Stuff
    public Transform _handLocation;
    private bool _pickedUp;
    [HideInInspector]
    public int _handIndex = 1;
    private Vector3 _handOffset;

    private Switch _switch;
    private Quaternion _startingRotation;

    /************************************************************************************************************************/
    #region Init
    protected override void Awake()
    {
        base.Awake();
        _startSize = this.transform.localScale;
        _meshRenderer = this.GetComponent<MeshRenderer>();
        _rb = this.gameObject.AddComponent<Rigidbody>();
        _collider = this.gameObject.GetComponent<Collider>();
        _isSubObject = this.GetComponent<QualityOverall>() == null;
        _switch = this.GetComponentInChildren<Switch>();
        if (transform.parent == null)
        {
            _canFollow = true;
            ///Cache the meshrenders of the children
            _childrenMeshRenderers = new List<MeshRenderer>();
            var childrenMeshRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var item in childrenMeshRenderers)
            {
                ///but do not include the ones on sockets, they are for development debuging is all
                if (!item.transform.GetComponent<Socket>())
                {
                    _childrenMeshRenderers.Add(item);
                }
            }
        }
        else
        {
            _parent = transform.parent.GetComponentInParent<ObjectController>(); //cache this if it works    
            _canFollow = false;
        }
        ToggleRB(true); ///turn off physics 

        DetermineHandLocation();
        _rotateAroundAxis = DetermineRotationAccess();

    }


    //void Start()
    //{
    //    if(_meshRenderer)
    //        UpdateJoseMaterialColor(_meshRenderer, 0, "_OutlineColor");
    //}
    //private void TempSetHighlighted(bool isTransparent)
    //{
    //    var mr = this.GetComponent<MeshRenderer>();
    //    if (mr)
    //    {
    //        if(isTransparent)
    //        {
    //            UpdateJoseMaterialColor(mr, 0.5f, "_Color");
    //            UpdateJoseMaterialColor(mr, 1, "_OutlineColor");
    //        }
    //        else //Not transparent
    //        {
    //            UpdateJoseMaterialColor(mr, 1f, "_Color");
    //            UpdateJoseMaterialColor(mr, 1, "_OutlineColor");
    //        }


    //    }
    //}

    private eRotationAxis DetermineRotationAccess()
    {
        if (_parent != null && (_myID == ObjectRecord.eItemID.PinkTop || _myID == ObjectRecord.eItemID.RedBot))
        {
            ///Dont rotate for final power, otherwise rotate discs
            if (_parent._myID == ObjectRecord.eItemID.finalPower)
                return eRotationAxis.NONE;
            else
                return eRotationAxis.XAXIS;
        }
        else if (_myID == ObjectRecord.eItemID.BlueBolt)
            return eRotationAxis.NONE;
        else
            return eRotationAxis.YAXIS;
    }

    private void DetermineHandLocation()
    {
        if (_isSubObject)
            return;

        _handOffset = _handLocation.position - this.transform.position;
    }
    #endregion
    /************************************************************************************************************************/
    private void Update()
    {
        if (_handLocation)
        {
            _handLocation.position = this.transform.position + _handOffset;
            if (_pickedUp && !HandPreviewingMode)
            {
                UIManager.UpdateHandLocation(_handIndex, _handLocation.position);
            }
        }
    }
    /************************************************************************************************************************/
    public void PickedUp(int handIndex)
    {
        ToggleRB(true);
        SetHighlighted(true);
        ///This better encapsulates this responsiblity inside the class
        if (RotationShouldBeReset())
        {
            this.transform.rotation = _startingRotation;
        }

        //HandManager.OrderChanged += UpdateHand;
        _pickedUp = true;
        _handIndex = handIndex;
        ChangeHighLightColor(handIndex);
        //Debug.Log($"Setting <color=blue>{this.gameObject.name}</color> to handIndex: <color=red>{handIndex} </color>");
    }
    public void PutDown()
    {
        ToggleRB(false);
        SetHighlighted(false);
        ChangeAppearanceNormal(); ///Did this fix the dropping reset issue
        //HandManager.OrderChanged -= UpdateHand;
        _pickedUp = false;
    }

    /************************************************************************************************************************/


    #region INTERFACE

    ///IInteractable
    public override void HandleInteractionTime(float time)
    {
        ChangeHighlightAmount(time);
        ///TODO make this take in an Interface?
        HandManager.StartToHandleIntensityChange(this);
    }

    ///IHighlightable
    public override void HandleHighlightPreview()
    {
        ///if its a current item being held in hand , return
        if (IsPickedUp())
            return;

        ///if its not highlighting turn it on 
        if (!IsHighlighted())
        {
            SetHighlighted(true);
            ChangeHighlightAmount(0);
        }

    }
    public override void CancelHighLightPreview()
    {
        SetHandPreviewingMode(false);

        if (IsPickedUp())
            return;

        HandManager.CancelIntensityChangePreview();

        if (IsHighlighted())
            SetHighlighted(false);

    }

    ///IMoveable (should make a base class to inherit from but have to combine IHighlightable in it)
    public void OnFollowInput(Vector3 worldPos) { Follow(worldPos); }
    public Vector2 OnRotate(Vector3 dot) { return DoRotation(dot); }
    public void OnBeginFollow()
    {
        ChangeAppearanceMoving();
        ResetHittingTable();
        HandManager.PickUpItem(this);

        // this is a copy of the method used in PreviewManager, should maybe make an AudioManager or improve the helper
        AudioSource audio = this.GetComponent<AudioSource>();
        if (audio != null)
        {
            audio.volume = 0.5f;
            audio.pitch = 1.0f;
            audio.Play();
        }
    }
    public void OnEndFollow() { ChangeAppearanceNormal(); }
    public bool OutOfBounds()
    {
        return _hittingTable;
    }
    private bool _resetOnChange;
    public void SetResetOnNextChange()
    {
        _resetOnChange = true;
        // Debug.DrawRay(_collider.bounds.min, -Vector3.forward, Color.red, 1);
    }
    public Quaternion GetDefaultOrientation() => _startingRotation;
    public bool CanRotate()
    {
        ///Can only rotate picked up parts, or subparts of picked up parts
        if (_isSubObject)
        {
            return _parent.CanRotate();
        }

        return _pickedUp;
    }
    public void ResetPosition()
    {
        ///Will move the obj up from being below, but does not reset rotation
        var mpos = transform.position;
        transform.position = new Vector3(mpos.x, 0, mpos.z);
        _hittingTable = false; ///Forwhatever reason this prevents us from calling OnTriggerExit
        _resetOnChange = false;
    }

    public void ResetPositionHard(Vector3 objStartPos, Quaternion objStartRot)
    {
        transform.position = objStartPos;
        transform.rotation = _startingRotation;
        UIManager.DebugLog($"RESET rot={_startingRotation}");
    }
    public bool IsPickedUp() => _pickedUp;

    ///Trying to phase these out of interface:
    public void ChangeAppearanceMoving()
    {
        Vector3 smaller = new Vector3
            (0.75f * this.transform.localScale.x,
            0.75f * this.transform.localScale.y,
            0.75f * this.transform.localScale.z);

        // this.transform.localScale = smaller;

        if (_handLocation)
            UIManager.ChangeHandSize(_handIndex, true);

        ChangeMaterialColor(0.5f);

        TrySetChildren(0.5f);

        if (_switch)
            _switch.ShowInPreview();

        // ToggleCollider(false);

        //Debug.Log($"{this.gameObject.name} heard change moving");

    }
    public void ChangeAppearanceNormal()
    {
        // Debug.Log($"Setting {this.gameObject.name} normal ");
        this.transform.localScale = _startSize;
        if (_handLocation)
            UIManager.ChangeHandSize(_handIndex, false);

        _meshRenderer.enabled = true;
        _isInPreview = false;
        ChangeMaterialColor(1f);
        TrySetChildren(1f);
        if (_switch)
            _switch.ShowNormal();

        if (_resetOnChange)
            ResetPosition();
    }
    public float DesiredSceneDepth() => SceneDepthInitalizer.Instance.DepthOfParts; //-9.253f;

    ///IConstructable
    private bool HandPreviewingMode;
    public void SetHandPreviewingMode(bool cond) { HandPreviewingMode = cond; }
    public void ChangeAppearanceHidden(bool cond)
    {
        // Debug.Log($"Setting {this.gameObject.name} hidden = {!cond}");
        _meshRenderer.enabled = !cond;
        /// i have to do this for all children as well 
        if (_parent == null && _childrenMeshRenderers != null)
            foreach (var mr in _childrenMeshRenderers)
                mr.enabled = !cond;

        ///TURN OFF ANY VFX associated w lightening
        if (cond && _myID == ObjectRecord.eItemID.finalPower)
        {
            var aSwitch = this.GetComponentInChildren<Switch>();
            if (aSwitch)
            {
                aSwitch.TurnOff();
            }
        }
    }
    public void ChangeAppearancePreview()
    {
        _isInPreview = true;
        ChangeMaterialColor(0.5f);
        TrySetChildren(0.5f);
    }
    public IConstructable FindAbsoluteParent()
    {
        return _parent == null ? this : _parent.FindAbsoluteParent();
    }
    public int MyID() => (int)_myID;
    #endregion

    /************************************************************************************************************************/
    ///I think i am doing both TriggerAndCollision becuz we change toggle IsTrigger on Collider when pickingup>?
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Table"))
        {
            _hittingTable = true;
            //_lastGoodYAboveTable = this.transform.position.y;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Table"))
        {
            _hittingTable = false;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!_hittingTable)
        {
            var otherGo = other.gameObject;
            if (otherGo.tag.Equals("Table"))
            {
                _hittingTable = true;
                return;
            }
            ///Bolts were able to land ontop of eachother after colliders got bigger,
            ///do this to push them off eachother
            var rb = otherGo.GetComponent<Rigidbody>();
            if (rb)
            {
                Vector3 dir = (otherGo.transform.position - this.transform.position).normalized;
                rb.AddForce(dir * 10);
            }
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag.Equals("Table"))
            _hittingTable = false;
    }

    private void OnDestroy()
    {
        HandManager.DropItem(this);
        if (_handLocation)
            Destroy(_handLocation.gameObject);
        _handLocation = null;
    }
    /************************************************************************************************************************/

    ///METHOD REQUIRES SHADER TO SUPPORT ALPHA TRANSPARENCY ON MATERIAL
    private void ChangeMaterialColor(float opacity) { ChangeMaterialColor(_meshRenderer, opacity); }
    private void ChangeMaterialColor(MeshRenderer mr, float opacity)
    {
        if (opacity > 1)
            Debug.LogWarning("Setting opacity > 1. Needs to be 0.0 - 1.0f");

        if (mr)
        {
            ///TODO cache this
            CustomShaderController shaderController = this.GetComponent<CustomShaderController>();
            shaderController.ChangeMaterialColor(opacity);
            //UpdateUnityDefaultMaterialColor(mr, opacity);
        }
    }

    private void ChangeHighLightColor(int handIndex)
    {
        Color color = handIndex == 1 ? ObjectManager.Instance._colorHand1 : ObjectManager.Instance._colorHand2;
        ChangeHighLightColor(color);
    }
    private void ResetHittingTable() { _hittingTable = false; }
    private void TrySetChildren(float opacity)
    {
        if (_parent != null)
            return; /// we are a child so our parent will handle this


        foreach (var mr in _childrenMeshRenderers)
        {
            mr.enabled = true;
            //UnityDefaultColorChange(mr, opacity);
            CustomShaderController shaderController = mr.GetComponent<CustomShaderController>();
            if (shaderController)
                shaderController.ChangeMaterialColor(opacity);
        }
    }
    protected Vector2 DoRotation(Vector3 dir)
    {

        float dot;

        //find out if object is right side up in world 

        if (_rotateAroundAxis == eRotationAxis.YAXIS)
        {
            if (Vector3.Dot(transform.up, Vector3.up) >= 0)
                dot = -Vector3.Dot(dir, Camera.main.transform.right);
            else
                dot = Vector3.Dot(dir, Camera.main.transform.right);

            var angle = dot / _dampening;
            ///Horiz  Project the  dir changed onto the camera.Up 
            transform.Rotate(transform.up, angle, Space.World);

            return new Vector2(0, angle);

        }
        if (_rotateAroundAxis == eRotationAxis.XAXIS)
        {
            dot = Vector3.Dot(dir, Camera.main.transform.up);

            ///Vertical  Project the  dir changed onto the camera.Right 
            var angle = dot / _dampening;

            transform.Rotate(-transform.forward, angle, Space.World);

            //Debug.Log($"New rotation z= {transform.rotation.z} vs  {transform.rotation.eulerAngles.z}");

            ///None of this worked below to zero out the other angles
            //var idk = Vector3.Cross(transform.right, transUp);
            //transform.Rotate(idk, dot / _dampening);

            //transform.rotation = Quaternion.FromToRotation(
            //    transform.rotation.eulerAngles, new Vector3(0,0,transform.rotation.z));
            //transform.rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z);

            //transform.rotation = Quaternion.Euler(oldX, oldY, transform.rotation.z);
            //transform.localEulerAngles = new Vector3(0, 0, transform.rotation.z);

            return new Vector2(angle, 0);
        }
        else if (_rotateAroundAxis == eRotationAxis.BOTH)
        {
            Vector2 retVal = Vector2.zero;
            if (Vector3.Dot(transform.up, Vector3.up) >= 0)
                dot = -Vector3.Dot(dir, Camera.main.transform.right);
            else
                dot = Vector3.Dot(dir, Camera.main.transform.right);

            ///Horiz
            transform.Rotate(transform.up, dot / _dampening, Space.World);

            retVal.y = dot / _dampening;

            if (Vector3.Dot(transform.up, Vector3.up) >= 0)
                dot = Vector3.Dot(dir, Camera.main.transform.up);
            else
                dot = Vector3.Dot(dir, Camera.main.transform.up);

            ///Vertical
            transform.Rotate(Camera.main.transform.right, Vector3.Dot(dir, Camera.main.transform.up) / _dampening, Space.World);

            retVal.x = dot / _dampening;
            ///***RetVal will be wrong not sure how to handle rotation values on both axes, dont think this should ever happen
            return retVal; ///return 0 to be safe were not getting confusing #s out
                           ///Could return a vector3 instead and have UserInputManagerkeep track of it there
        }
        else ///NONE
            return Vector2.zero;


        //return dot / _dampening;
    }
    protected void Follow(Vector3 loc)
    {
        if (_canFollow)
        {
            //this.transform.position = loc;
            // Debug.LogWarning("Told to go to:" + this.transform.position);
            if (!_hittingTable)
            {
                this.transform.position = Vector3.Lerp(transform.position, loc, 0.5f);
            }
            else
            {
                // if were going up, allow it
                if (loc.y > this.transform.position.y)
                {
                    // UIManager.DebugLog($"Trying to move UP from {this.transform.position.y} to <color=green> {loc.y} </color>");
                    this.transform.position = Vector3.Lerp(transform.position, loc, 0.5f);
                }
                //else /if direction is going to go more into table prevent it,
            }
        }
        else if (_parent)
        {
            _parent.Follow(loc);
        }
        else
            UIManager.DebugLogWarning("No Parent for this object and follow set to false, prefab possibly set wrong");
    }
    private void ToggleRB(bool cond)
    {
        if (_rb)  ///This gets kind of weird with the subobjects
        {
            _rb.isKinematic = cond;
            _rb.useGravity = !cond;
        }
        if (_collider)
            _collider.isTrigger = cond;
    }
    public void SetStartingRotation(Quaternion rot)
    {
        _startingRotation = rot;
        //Debug.Log($"SetRot={_startingRotation}");
    }
    private bool RotationShouldBeReset()
    {
        ///If its on the table, we gotta reset the orientation when pickedup
        if (_hittingTable)
        {
            return true;
        }
        ///Rotation can also be off if its not picked up and ontop of another item
        if (!_pickedUp && !_isInPreview) //&& not hitting table
        {
            return true;
        }
        return false;
    }

    /************************************************************************************************************************/
    ///UNUSED??
    private bool SetOnTable()
    {
        return _rb.useGravity == true;
    }
    private void UpdateHand(Queue<ObjectController> queue)
    {
        ///find my order in the Queue
        _handIndex = 1;
        while (queue.Count != 0)
        {
            var controller = queue.Dequeue();
            ++_handIndex;
            if (controller == this)
            {
                return;
            }
        }
    }

    private void ToggleCollider(bool cond)
    {
        //good for testing to turn off print statements in socket collisions
        this.GetComponent<Collider>().enabled = cond;
    }


    /************************************************************************************************************************/



#if UNITY_EDITOR
    #region Custom Inspector Settings
    /// Will hide the _requiredRotationThreshold if we aren't doing a rotation action
    [CustomEditor(typeof(ObjectController))]
    [CanEditMultipleObjects]
    public class ObjectControllerEditor : Editor
    {
        //SerializedProperty typeProp;
        string[] _enumList;

        private void OnEnable()
        {
            //typeProp = serializedObject.FindProperty("test");
            _enumList = GetEnumList();
        }

        public override void OnInspectorGUI()
        {
            ///Cant figure out how to completely redraw the array so best I can do is provide a numbered preview list
           // DrawPreviewDropDown();

            base.OnInspectorGUI();

        }

        private void DrawPreviewDropDown()
        {
            int selected = 0;
            string[] options = _enumList;
            selected = EditorGUILayout.Popup("Numbered Reference list", selected, options);

        }

        private string[] GetEnumList()
        {
            var arrList = System.Enum.GetValues(typeof(ObjectRecord.eItemID));
            string[] list = new string[arrList.Length];
            int index = 0;
            foreach (var item in arrList)
            {
                list[index++] = $"{index}: {item}";
            }


            return list;
        }

        private ObjectRecord.eItemID AssignByID(int id)
        {
            return (ObjectRecord.eItemID)id + 1;
        }


    }

    #endregion

#endif


}

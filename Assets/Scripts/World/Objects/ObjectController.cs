using HighlightPlus;
using System.Collections.Generic;
using UnityEditor;

using UnityEngine;

public class ObjectController : MonoBehaviour, IConstructable
{

    public ObjectManager.eItemID _myID;
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
    private HighlightTrigger _highlightTrigger;
    ///Components
    private Rigidbody _rb;
    private Collider _collider;
    public bool _hittingTable { get; private set; }
    private float _lastGoodYAboveTable;
    private bool _isSubObject;
    [HideInInspector]
    public ObjectController _parent;
    ///Hand Stuff
    [HideInInspector]
    public Transform _handLocation;
    private bool _pickedUp;
    [HideInInspector]
    public int _handIndex = 1;
    private Vector3 _handOffset;
    private float _handStartZ;


    /************************************************************************************************************************/
    #region Init
    private void Awake()
    {
        _startSize = this.transform.localScale;
        _meshRenderer = this.GetComponent<MeshRenderer>();
        _rb = this.gameObject.AddComponent<Rigidbody>();
        _collider = this.gameObject.GetComponent<Collider>();
        _isSubObject = this.GetComponent<QualityOverall>() == null;

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
        SetUpHighlightComponent();
        DetermineHandLocation();
        _rotateAroundAxis = DetermineRotationAccess();

    }

    private void SetUpHighlightComponent()
    {
        var effect = transform.gameObject.AddComponent<HighlightEffect>();
        var profile = Resources.Load<HighlightProfile>("Shaders/Highlight Plus Profile");
        if (profile != null)
            effect.ProfileLoad(profile);
        _highlightTrigger = this.gameObject.AddComponent<HighlightTrigger>();

    }

    private eRotationAxis DetermineRotationAccess()
    {
        if (_parent != null && (_myID == ObjectManager.eItemID.PinkTop || _myID == ObjectManager.eItemID.RedBot))
            return eRotationAxis.XAXIS;
        else
            return eRotationAxis.YAXIS;
    }

    private void DetermineHandLocation()
    {
        if (_isSubObject)
            return;

        var collider = this.GetComponent<Collider>();
        float bottom = collider.bounds.center.y - collider.bounds.extents.y;
        float top = collider.bounds.center.y + collider.bounds.extents.y;
        float front = collider.bounds.center.z + collider.bounds.extents.z;
        float back = collider.bounds.center.z - collider.bounds.extents.z;
        float left = collider.bounds.center.x + collider.bounds.extents.x;
        float right = collider.bounds.center.x - collider.bounds.extents.x;

        var prefab = Resources.Load<GameObject>("Prefab/hand_loc_dummy");
        if (prefab)
        {
            var dummy = GameObject.Instantiate<GameObject>(prefab, BuildableObject.Instance.transform);
            _handLocation = dummy.transform;
            var index = this.gameObject.name.IndexOf("(Clone)");
            if (index != -1)
                this.gameObject.name = this.gameObject.name.Substring(0, this.gameObject.name.IndexOf("(Clone)")) + "_" + _myID;
            _handLocation.gameObject.name = this.gameObject.name + "_hand_dummy";
            _handOffset = new Vector3(left, bottom, front) - this.transform.position;
            _handStartZ = (this.transform.position + _handOffset).z;
        }
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
                UIManager.instance.UpdateHandLocation(_handIndex, _handLocation.position);
            }
        }

    }
    /************************************************************************************************************************/


    public void ToggleRB(bool cond)
    {
        if (_rb)  ///This gets kind of weird with the subobjects
        {
            _rb.isKinematic = cond;
            _rb.useGravity = !cond;
        }
        if (_collider)
            _collider.isTrigger = cond;
    }


    #region Highlight Outline

    ///From states mainly:
    private bool _isHighlighted;
    public bool IsHighlighted() => _isHighlighted;

    public void SetHighlighted(bool cond)
    {
        if (_highlightTrigger)
            _highlightTrigger.Highlight(cond);

        var childrenHighlights = GetComponentsInChildren<HighlightTrigger>();
        foreach (var item in childrenHighlights)
        {
            item.Highlight(cond);
        }

        _isHighlighted = cond;
    }



    ///HandMAnager
    public void PickedUp(int handIndex)
    {
        SetHighlighted(true);

        //HandManager.OrderChanged += UpdateHand;
        _pickedUp = true;
        _handIndex = handIndex;
        ChangeHighLightColor(handIndex);
        // Debug.Log($"Setting <color=blue>{this.gameObject.name}</color> to handIndex: <color=red>{handIndex} </color>");
    }
    public void PutDown()
    {
        SetHighlighted(false);

        //HandManager.OrderChanged -= UpdateHand;
        _pickedUp = false;
    }
    public float GetHighlightIntensity()
    {
        if (_highlightTrigger)
        {
            var effect = this.GetComponent<HighlightEffect>();
            return effect.outline;
        }
        return 0;
    }
    public Color GetHighLightColor()
    {
        if (_highlightTrigger)
        {
            var effect = this.GetComponent<HighlightEffect>();
            return effect.outlineColor;
        }
        return Color.white;
    }
    public void ChangeHighlightAmount(float intensity)
    {
        if (_highlightTrigger)
        {
            var effect = this.GetComponent<HighlightEffect>();
            effect.outline = intensity;

            var childrenEffects = GetComponentsInChildren<HighlightEffect>();
            foreach (var item in childrenEffects)
            {
                item.outline = intensity;
            }
        }
    }
    private void ChangeHighLightColor(int handIndex)
    {
        Color color = handIndex == 1 ? BuildableObject.Instance._colorHand1 : BuildableObject.Instance._colorHand2;
        ChangeHighLightColor(color);
    }
    public void ChangeHighLightColor(Color color)
    {
        if (_highlightTrigger)
        {
            var effect = this.GetComponent<HighlightEffect>();
            effect.outlineColor = color;

            var childrenEffects = GetComponentsInChildren<HighlightEffect>();
            foreach (var item in childrenEffects)
            {
                item.outlineColor = color;
            }
        }
    }
    #endregion


    #region INTERFACE

    ///IInteractable
    public GameObject GetGameObject() => gameObject;
    public Transform GetParent() => this.transform.parent;
    public Transform Transform() => this.transform;
    public void OnInteract()
    {
        ///Nothing really happens we click this object?
    }
    public void HandleInteractionTime(float time)
    {
        ChangeHighlightAmount(time);
    }


    ///IMoveable
    public void OnFollowInput(Vector3 worldPos)
    {
        Follow(worldPos);
    }
    public Vector2 OnRotate(Vector3 dot) { return DoRotation(dot); }
    public void AllowFollow() { ResetHittingTable(); }
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
    public void ResetPosition()
    {
        ///fck all this somethings off w the scaling , just set it to 0.
        //var tableY = -0.455f;
        //var bonusAmnt = 0.02f;
        //var min = _collider.bounds.min;
        //var diffBelowTable = min.y - tableY;  /// add cuz both negative 
        //Debug.LogWarning($"min.y={min.y} and diff={diffBelowTable}   oldy= {mpos.y} newy= {mpos.y + Mathf.Abs(diffBelowTable)}  ....... my scale= {this.transform.localScale}");


        var mpos = transform.position;
        transform.position = new Vector3(mpos.x, 0, mpos.z);
    }
    public bool IsPickedUp() => _pickedUp;
    public void ChangeAppearanceMoving()
    {
        Vector3 smaller = new Vector3
            (0.75f * this.transform.localScale.x,
            0.75f * this.transform.localScale.y,
            0.75f * this.transform.localScale.z);

        this.transform.localScale = smaller;

        if (_handLocation)
            UIManager.instance.ChangeHandSize(_handIndex, true);

        ChangeMaterialColor(0.5f);

        TrySetChildren(0.5f);

        // ToggleCollider(false);

        //Debug.Log($"{this.gameObject.name} heard change moving");

    }
    public void ChangeAppearanceNormal()
    {
        // Debug.Log($"Setting {this.gameObject.name} normal ");
        this.transform.localScale = _startSize;
        if (_handLocation)
            UIManager.instance.ChangeHandSize(_handIndex, false);

        _meshRenderer.enabled = true;

        ChangeMaterialColor(1f);
        TrySetChildren(1f);


        if (_resetOnChange)
            ResetPosition();
    }



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

    }
    public void ChangeAppearancePreview()
    {
        ChangeMaterialColor(0.5f);
        TrySetChildren(0.5f);
    }

    #endregion


    /************************************************************************************************************************/
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Table"))
        {
            _hittingTable = true;
            _lastGoodYAboveTable = this.transform.position.y;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Table"))
            _hittingTable = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.tag.Equals("Table"))
        {
            _hittingTable = true;
            _lastGoodYAboveTable = this.transform.position.y;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.tag.Equals("Table"))
            _hittingTable = false;
    }

    private void OnDestroy()
    {
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
            Material m = mr.material;
            Color color = m.color;
            color.a = opacity;
            m.color = color;
            mr.material = m;
        }
    }

    private void ResetHittingTable() { _hittingTable = false; }

    private void TrySetChildren(float opacity)
    {
        if (_parent != null)
            return; /// we are a child so our parent will handle this


        foreach (var mr in _childrenMeshRenderers)
        {
            mr.enabled = true;
            Material m = mr.material;
            Color color = m.color;
            color.a = opacity;
            m.color = color;
            mr.material = m;
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
                this.transform.position = Vector3.Lerp(transform.position, loc, 0.5f);
            else
            {
                // if were going up, allow it
                if (loc.y > this.transform.position.y)
                    this.transform.position = Vector3.Lerp(transform.position, loc, 0.5f);
                //else /if direction is going to go more into table prevent it,
            }
        }
        else if (_parent)
        {
            _parent.Follow(loc);
        }
        else
            UIManager.instance.DebugLogWarning("No Parent for this object and follow set to false, prefab possibly set wrong");
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
            var arrList = System.Enum.GetValues(typeof(ObjectManager.eItemID));
            string[] list = new string[arrList.Length];
            int index = 0;
            foreach (var item in arrList)
            {
                list[index++] = $"{index}: {item}";
            }


            return list;
        }

        private ObjectManager.eItemID AssignByID(int id)
        {
            return (ObjectManager.eItemID)id + 1;
        }


    }

    #endregion

#endif


}

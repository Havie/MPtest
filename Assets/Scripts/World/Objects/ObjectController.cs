using HighlightPlus;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{

    public ObjectManager.eItemID _myID;
    ///Rotational / Movement
    public enum eRotationAxis { YAXIS, XAXIS, BOTH, NONE};
    public eRotationAxis _rotationAxis= eRotationAxis.YAXIS;
    public bool _canFollow = true;
    private int _dampening = 10;
    ///effect stuff
    private Vector3 _startSize;
    private MeshRenderer _meshRenderer;
    private MeshRenderer[] _childrenMeshRenderers;
    private HighlightTrigger _highlightTrigger;
    ///Components
    private Rigidbody _rb;
    private Collider  _collider;
    private bool _hittingTable;
    private bool _isSubObject;
    [HideInInspector]
    public ObjectController _parent;
    ///Hand Stuff
    [HideInInspector]
    public Transform _handLocation;
    private bool _pickedUp;
    private int _handIndex=1;
    private Vector3 _handOffset;
    private float _handStartZ;




    private void Awake()
    {
        _startSize = this.transform.localScale;
        _meshRenderer = this.GetComponent<MeshRenderer>();
        _childrenMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        _rb = this.gameObject.AddComponent<Rigidbody>();
        _collider = this.gameObject.GetComponent<Collider>();
        _isSubObject = this.GetComponent<OverallQuality>() == null; 

        if (transform.parent==null)
            _parent = null;
        else
            _parent = transform.parent.GetComponentInParent<ObjectController>(); //cache this if it works    
       
        ToggleRB(true); ///turn off physics 
        SetUpHighlightComponent();
        DetermineHandLocation();

    }

    private void SetUpHighlightComponent()
    {
        var effect = transform.gameObject.AddComponent<HighlightEffect>();
        var profile = Resources.Load<HighlightProfile>("Shaders/Highlight Plus Profile");
        if (profile != null)
            effect.ProfileLoad(profile);
        _highlightTrigger = this.gameObject.AddComponent<HighlightTrigger>();

    }

    private void Update()
    {
        if (_handLocation)
        {
            _handLocation.position = this.transform.position + _handOffset;
            if (_pickedUp)
            {
                UIManager.instance.UpdateHandLocation(_handIndex, _handLocation.position);
            }
        }
    }

    private void DetermineHandLocation()
    {
        if (_isSubObject)
            return;

        var collider = this.GetComponent<Collider>();
        float bottom = collider.bounds.center.y - collider.bounds.extents.y ;
        float top = collider.bounds.center.y + collider.bounds.extents.y;
        float front = collider.bounds.center.z + collider.bounds.extents.z;
        float back = collider.bounds.center.z - collider.bounds.extents.z;
        float left = collider.bounds.center.x + collider.bounds.extents.x;
        float right = collider.bounds.center.x - collider.bounds.extents.x;

        var prefab = Resources.Load<GameObject>("Prefab/hand_loc_dummy");
        if(prefab)
        {
            var dummy =GameObject.Instantiate<GameObject>(prefab, BuildableObject.Instance.transform);
            _handLocation = dummy.transform;
            var index = this.gameObject.name.IndexOf("(Clone)");
            if(index!=-1)
                this.gameObject.name = this.gameObject.name.Substring(0 ,this.gameObject.name.IndexOf("(Clone)"));
            _handLocation.gameObject.name = this.gameObject.name + "_hand_dummy";
            _handOffset = new Vector3(left, bottom, front) - this.transform.position ;
            _handStartZ = (this.transform.position + _handOffset).z;
        }    
    }

    public Vector2 DoRotation(Vector3 dir)
    {

        float dot;

        //find out if object is right side up in world 

        if (_rotationAxis == eRotationAxis.YAXIS)
        {
            if (Vector3.Dot(transform.up, Vector3.up) >= 0)
                dot = -Vector3.Dot(dir, Camera.main.transform.right);
            else
                dot = Vector3.Dot(dir, Camera.main.transform.right);

            ///Horiz  Project the  dir changed onto the camera.Up 
            transform.Rotate(transform.up, dot / _dampening, Space.World);

            return new Vector2(0, dot / _dampening);

        }
        if (_rotationAxis == eRotationAxis.XAXIS)
        {
            if (Vector3.Dot(transform.up, Vector3.up) >= 0)
                dot = Vector3.Dot(dir, Camera.main.transform.up);
            else
                dot = Vector3.Dot(dir, Camera.main.transform.up);

            ///Vertical  Project the  dir changed onto the camera.Right 

            transform.Rotate(Camera.main.transform.right, dot / _dampening, Space.World);

            return new Vector2(dot / _dampening, 0);
        }
        else if (_rotationAxis == eRotationAxis.BOTH)
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
                           ///Could return a vector3 instead and have userInput keep track of it there
        }
        else ///NONE
            return Vector2.zero;


        //return dot / _dampening;
    }

    public void Follow(Vector3 loc)
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
                if (loc.y > 0)
                    this.transform.position = Vector3.Lerp(transform.position, loc, 0.5f);
                //else /if direction is going to go more into table prevent it,
            }
        }
        else if (_parent)
        {
            _parent.Follow(loc);
        }
    }

    private void ToggleCollider(bool cond)
    {
        //good for testing to turn off print statements in socket collisions
        this.GetComponent<Collider>().enabled = cond;
    }

    private void TrySetChildren(float opacity)
    {
        if (_parent != null)
            return; /// we are a child so our parent will handle this

        foreach (var mr in _childrenMeshRenderers)
        {
            ChangeMaterialColor(mr, opacity);
        }
    }

    public void ChangeAppearanceMoving()
    {
        Vector3 smaller= new Vector3
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
        this.transform.localScale = _startSize;
        if (_handLocation)
            UIManager.instance.ChangeHandSize(_handIndex, false);

        _meshRenderer.enabled = true;
        ChangeMaterialColor(1f);
        TrySetChildren(1f);
        //ToggleCollider(true);

        //Debug.Log($"{this.gameObject.name} heard change normal");
    }

    public void ChangeAppearancePreview()
    {
        ChangeMaterialColor(0.5f);
    }
    public void ChangeAppearanceHidden(bool cond)
    {
        _meshRenderer.enabled = !cond;
    }

    public void PickedUp()
    {
        if (_highlightTrigger)
            _highlightTrigger.Highlight(true);

        var childrenHighlights = GetComponentsInChildren<HighlightTrigger>();
        foreach (var item in childrenHighlights)
        {
            item.Highlight(true);
        }

        HandManager.OrderChanged += UpdateHand;
        _pickedUp = true;
        _handIndex = 1;
    }

    public void PutDown()
    {
        if (_highlightTrigger)
            _highlightTrigger.Highlight(false);

        var childrenHighlights = GetComponentsInChildren<HighlightTrigger>();
        foreach (var item in childrenHighlights)
        {
            item.Highlight(false);
        }

        HandManager.OrderChanged -= UpdateHand;
        _pickedUp = false;
    }
    private void UpdateHand(Queue<ObjectController> queue)
    {
        ///find my order in the Queue
        _handIndex = 1;
        while(queue.Count!=0)
        {
            var controller = queue.Dequeue();
            ++_handIndex;
            if (controller == this)
            {
                return;
            }
        }
    }

    public void ToggleRB(bool cond)
    {
        if (_rb)  ///This gets kind of weird with the subobjects
        {
            _rb.isKinematic = cond;
            _rb.useGravity = !cond;
        }
        if(_collider)
            _collider.isTrigger = cond;
    }

    public bool OnTable()
    {
        return _rb.useGravity == true;
    }

    public void ResetHittingTable()
    {
        _hittingTable = false;
    }

    ///METHOD REQUIRES SHADER TO SUPPORT ALPHA TRANSPARENCY ON MATERIAL
    private void ChangeMaterialColor(float opacity)
    {
        ChangeMaterialColor(_meshRenderer, opacity);

    }

    private void ChangeMaterialColor(MeshRenderer mr , float opacity)
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
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals("Table"))
        {
            _hittingTable = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Equals("Table"))
            _hittingTable = false;
    }

    private void OnDestroy()
    {
        if (_handLocation)
            Destroy(_handLocation.gameObject);
        _handLocation = null;
    }
}

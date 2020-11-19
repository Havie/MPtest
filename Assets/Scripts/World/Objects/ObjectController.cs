using HighlightPlus;
using UnityEngine;

public class ObjectController : MonoBehaviour
{

    public ObjectManager.eItemID _myID;
    public enum eRotationAxis { YAXIS, XAXIS, BOTH, NONE};
    public eRotationAxis _rotationAxis= eRotationAxis.YAXIS;
    public bool _canFollow = true;
    private int _dampening = 10;
    private Vector3 _startSize;
    private MeshRenderer _meshRenderer;
    private MeshRenderer[] _childrenMeshRenderers;
    private Rigidbody _rb;
    private Collider  _collider;
    private bool _hittingTable;
    private bool _isSubObject;
    [HideInInspector]
    public ObjectController _parent;
    [HideInInspector]
    public Vector3 _handLocation;
    private HighlightTrigger _highlightTrigger;
    


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
       
        ToggleRB(true);

        var effect = transform.gameObject.AddComponent<HighlightEffect>();
        var profile = Resources.Load<HighlightProfile>("Shaders/Highlight Plus Profile");
        if(profile!=null)
            effect.ProfileLoad(profile);
        _highlightTrigger = this.gameObject.AddComponent<HighlightTrigger>();

        DetermineHandLocation();

    }

    private void DetermineHandLocation()
    {
        var collider = this.GetComponent<Collider>();
        float bottom = collider.bounds.center.y - collider.bounds.extents.y ;
        float top = collider.bounds.center.y + collider.bounds.extents.y;
        float front = collider.bounds.center.z + collider.bounds.extents.z;
        float back = collider.bounds.center.z - collider.bounds.extents.z;
        float left = collider.bounds.center.x + collider.bounds.extents.x;
        float right = collider.bounds.center.x - collider.bounds.extents.x;

        _handLocation = new Vector3(right, bottom, front);
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
        this.transform.localScale =  new Vector3
            (0.75f * this.transform.localScale.x, 
            0.75f * this.transform.localScale.y ,
            0.75f * this.transform.localScale.z); 

        ChangeMaterialColor(0.5f);

        TrySetChildren(0.5f);

       // ToggleCollider(false);

    }
    public void ChangeAppearanceNormal()
    {
        this.transform.localScale = _startSize;
        _meshRenderer.enabled = true;
        ChangeMaterialColor(1f);
        TrySetChildren(1f);
        //ToggleCollider(true);
    }

    public void ChangeAppearancePreview()
    {
        ChangeMaterialColor(0.5f);
    }
    public void ChangeAppearanceHidden()
    {
        _meshRenderer.enabled = false;
    }

    public void ChangeAppearancePickedUp()
    {
        if (_highlightTrigger)
            _highlightTrigger.Highlight(true);
    }

    public void ChangeAppearancePutDown()
    {
        if (_highlightTrigger)
            _highlightTrigger.Highlight(false);
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
}

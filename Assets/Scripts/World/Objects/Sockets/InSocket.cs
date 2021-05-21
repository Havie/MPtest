using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UserInput;
public class InSocket : Socket
{
    //[Range(-0.5f, 0.5f)]
     private float _attachmentSensitivity = 0.5f; ///Closeness Threshold
    [SerializeField] ObjectRecord.eItemID[] _requiredAttachmentID = default;
    [SerializeField] ObjectRecord.eItemID[] _createdID = default;
    GameObject _snapVfxPREFAB;

    private bool _canCollide;

    private void Awake()
    {
        _in = true;
        _snapVfxPREFAB = Resources.Load<GameObject>("Prefab/VFX/FX_Snap");
        StartCoroutine(WaitDelay());
    }

    IEnumerator WaitDelay()
    {
        /// I am not sure why i was doing this, perhaps some sub component werent init,
        /// dont go to 2 seconds, or its possible first attachment fails
        yield return new WaitForSeconds(0.75f);
        _canCollide = true;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (!_canCollide)
            return;

        if (_in) // only have the female sockets checking collisions 
        {
            //Debug.Log($"{this.transform.gameObject.name} triggered with {other.gameObject.name}");
            var socket = other.GetComponent<Socket>();
            for (int i = 0; i < _requiredAttachmentID.Length; i++)
            {
                int requiredAttachmentID = (int)_requiredAttachmentID[i];
                if (socket && CheckConditions(socket, requiredAttachmentID))
                {
                   // Debug.Log($"match for  {socket.gameObject}:{this.gameObject.name} and {socket.gameObject}:{socket}");
                    PreviewManager.ShowPreview(Controller, socket.Controller, (int)_createdID[i], _snapVfxPREFAB, transform);
                }
                /* else if (socket)  ///for debugging to get more info 
                 {
                     if (i < _requiredAttachmentID.Length)
                     {
                         Debug.Log($"..looping thru to check other attachments myID={requiredAttachmentID}...");
                     }
                     else if (requiredAttachmentID != (int)socket._controller._myID)
                         Debug.Log($"{requiredAttachmentID} does not match {(int)socket._controller._myID}");
                     else if (UserInput.Instance._currentSelection == _controller)
                         Debug.Log(" Cant attach female to male, select and move male part");
                     else
                         Debug.Log($"Attachment Angle was invalid"); 
                  }
                */
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_canCollide)
            return;


        if (_in) // only have the female sockets checking collisions 
        {
            //Debug.Log($"{this.transform.gameObject.name} triggered with {other.gameObject.name}");

            var socket = other.GetComponent<Socket>();
            for (int i = 0; i < _requiredAttachmentID.Length; i++)
            {
                int requiredAttachmentID = (int)_requiredAttachmentID[i];
                if (socket && CheckConditions(socket, requiredAttachmentID))
                    PreviewManager.UndoPreview();
            }

        }
    }

    private bool CheckConditions(Socket socket, int requiredAttachmentID)
    {
        if (!socket.IsInit)
            return false;

        bool valid = false;

        ///Make this a requirement so edge cases dont happen
        bool bothItemsArePickedUp = socket.Controller.IsPickedUp() && Controller.IsPickedUp();

        ///TODO wish i cud clean up this dependency of UserInputManager, but no great way to tell
        /// which part is moving, could maybe do on hand Index if we keep hand limit at 2
        //Not moving the female part and items match              
        //if my IDs = the incomming ID
        if (bothItemsArePickedUp &&
            requiredAttachmentID == (int)socket.Controller._myID &&
            UserInputManager.Instance.CurrentSelection as ObjectController != Controller //Save most expensive check for last, cant ram female into male
            )
        {
            //check the angles of attachment
            //Vector3 dir = socket.transform.forward - this.transform.forward;
            float cosAngleBetween = Vector3.Dot(this.transform.forward.normalized, socket.transform.forward.normalized);
            ///Tablet processes these single precision floating point numbers differently than PC, and rounding errors can occur: (dot product has multiple multiplcations and additions for room for rounding error)
            //bool roughlyAligned = Mathf.Abs(cosAngleBetween - 1) <= _attachmentSensitivity; ///Try to match machine epsilon? kind of magic number solution cuz no better one 
            bool roughlyOpposite = Mathf.Abs(cosAngleBetween + 1) <= _attachmentSensitivity;

            var maleSocket = socket as OutSocket;
            //Debug.Log($"NORMALIZEDangle=<color=purple>{angle}</color> for ID:{requiredAttachmentID} ?< {_attachmentSensitivity}  and inprev= {PreviewManager._inPreview}");
            if (!PreviewManager._inPreview) //OnTriggerEnter
            {
                if (roughlyOpposite)
                {
                    // -1 is perfect match 
                    valid = isProperAttachmentVelocity(maleSocket);
                }
                else
                {
                    //Debug.Log($"The angle did not match for {requiredAttachmentID} , or velocity was inverted");
                }
            }
            else //OnTriggerExit
            {
                valid = true;
            }
        }
        else if(bothItemsArePickedUp && requiredAttachmentID != (int)socket.Controller._myID)
        {
            //Play Negative Sound effect
        }
        //  Debug.LogWarning($"incomming::{(int)socket._controller._myID} != {requiredAttachmentID}");

        return valid;

    }


    private bool isProperAttachmentVelocity(OutSocket maleSocket)
    {
        if (!maleSocket)
            return false;

        ///This might not work for avg ppl during UX who just try to drop the part on another..
        if (maleSocket.AttachesHorizontal)
        {
            return Mathf.Sign(maleSocket.xVelocity) == Mathf.Sign(maleSocket.transform.forward.x);
        }
        else
        {
            return Mathf.Sign(maleSocket.yVelocity) == Mathf.Sign(maleSocket.transform.forward.y);
        }

    }
    

#if UNITY_EDITOR
    #region Custom Inspector Settings
    /// Will hide the _requiredRotationThreshold if we aren't doing a rotation action
    [CustomEditor(typeof(InSocket))]
    [CanEditMultipleObjects]
    public class InSocketEditor : Editor
    {
        //SerializedProperty forcedLayer;
        string[] _enumList;
        SerializedProperty _attachmentSensitivity;
        SerializedProperty _requiredAttachmentID;
        SerializedProperty _createdID;

        private void OnEnable()
        {
            //typeProp = serializedObject.FindProperty("test");
            //forcedLayer = serializedObject.FindProperty("_forcedLayer");  ///Have to do a string becuz var is private
            _enumList = GetEnumList();
            _attachmentSensitivity = serializedObject.FindProperty(nameof(_attachmentSensitivity));
            _requiredAttachmentID = serializedObject.FindProperty(nameof(_requiredAttachmentID));
            _createdID = serializedObject.FindProperty(nameof(_createdID));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // ///Cant figure out how to completely redraw the array so best I can do is provide a numbered preview list
            //DrawPreviewDropDown();

            //EditorGUILayout.PropertyField(forcedLayer);
            //forcedLayer= EditorGUILayout.EnumPopup((LayerMask)forcedLayer.enumValueIndex);
            //EditorGUILayout.PropertyField(_attachmentSensitivity);
            EditorGUILayout.PropertyField(_requiredAttachmentID);
            EditorGUILayout.PropertyField(_createdID);


            serializedObject.ApplyModifiedProperties();


            // base.OnInspectorGUI();

        }

        private void DrawPreviewDropDown()
        {
            int selected = 0;
            string[] options = _enumList;
            selected = EditorGUILayout.Popup("Numbered Reference list", selected, options);
            
            //EditorGUILayout.EnumPopup()

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

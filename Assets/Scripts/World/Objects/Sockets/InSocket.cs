using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UserInput;
public class InSocket : Socket
{
    [Range(-0.5f, 0.5f)]
    [SerializeField] private float _attachmentSensitivity = 0;
    [SerializeField] ObjectManager.eItemID[] _requiredAttachmentID = default;
    [SerializeField] ObjectManager.eItemID[] _createdID = default;


    public enum test { opONE, opTWO }

    private bool _canCollide;

    private void Awake()
    {
        _in = true;
        StartCoroutine(WaitDelay());
     
    }

    IEnumerator WaitDelay()
    {
        yield return new WaitForSeconds(2);
        _canCollide = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_canCollide)
            return;

        //Debug.Log("Called ENTEr" +this.gameObject.name);
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
                    PreviewManager.ShowPreview(Controller, socket.Controller, (int)_createdID[i]);
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

        ///TODO wish i cud clean up this dependency of UserInputManager
        //Not moving the female part and items match              //if one of my IDs = the incomming ID
        if (UserInputManager.Instance.CurrentSelection as ObjectController != Controller && requiredAttachmentID == (int)socket.Controller._myID)
        {
            //check the angles of attachment
            //Vector3 dir = socket.transform.forward - this.transform.forward;
            float angle = Vector3.Dot(this.transform.forward.normalized, socket.transform.forward.normalized);
            Debug.Log($"NORMALIZEDangle=<color=purple>{angle}</color> for ID:{requiredAttachmentID} ?< {_attachmentSensitivity}  and inprev= {PreviewManager._inPreview}");
            if (!PreviewManager._inPreview) //OnTriggerEnter
            {
                if (angle < _attachmentSensitivity) // -1 is perfect match 
                    valid = true;
                else
                {
                    //Debug.Log($"The angle did not match for {requiredAttachmentID}");
                }
            }
            else  //OnTriggerExit
                valid = true;
        }
        // else
        //  Debug.LogWarning($"incomming::{(int)socket._controller._myID} != {requiredAttachmentID}");

        return valid;

    }

    

#if UNITY_EDITOR
    #region Custom Inspector Settings
    /// Will hide the _requiredRotationThreshold if we aren't doing a rotation action
    [CustomEditor(typeof(InSocket))]
    [CanEditMultipleObjects]
    public class InSocketEditor : Editor
    {
        //SerializedProperty typeProp;
        string[] _enumList;
        SerializedProperty _attachmentSensitivity;
        SerializedProperty _requiredAttachmentID;
        SerializedProperty _createdID;

        private void OnEnable()
        {
            //typeProp = serializedObject.FindProperty("test");
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

            EditorGUILayout.PropertyField(_attachmentSensitivity);
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

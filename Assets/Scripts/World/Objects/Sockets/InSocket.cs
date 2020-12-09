using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InSocket : Socket
{
    [Range(-0.5f, 0.5f)]
    [SerializeField] private float _attachmentSensitivity = 0;
    [SerializeField] ObjectManager.eItemID[] _requiredAttachmentID;
    [SerializeField] ObjectManager.eItemID[] _createdID;


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
                    //Debug.Log($"match for  {_controller.gameObject}:{this.gameObject.name}:{_controller._myID}  and {socket._controller.gameObject}:{socket}{socket._controller._myID}");
                    PreviewManager.ShowPreview(_controller, socket._controller, (int)_createdID[i]);
                }
                /* else if (socket)
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
        bool valid = false;

        //Not moving the female part and items match              //if one of my IDs = the incomming ID
        if (UserInput.Instance._currentSelection != _controller && requiredAttachmentID == (int)socket._controller._myID)
        {
            //check the angles of attachment
            Vector3 dir = socket.transform.position - this.transform.position;
            float angle = Vector3.Dot(this.transform.forward.normalized, dir.normalized);
            //Debug.Log($"angle={angle} for {requiredAttachmentID}   and inprev= {PreviewManager._inPreview}");
            if (!PreviewManager._inPreview) //OnTriggerEnter
            {
                if (angle > _attachmentSensitivity) // 1 is perfect match 
                    valid = true;
                else
                {
                    Debug.Log($"The angle did not match for {requiredAttachmentID}");
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

        private void OnEnable()
        {
            //typeProp = serializedObject.FindProperty("test");
            _enumList = GetEnumList();
        }

        public override void OnInspectorGUI()
        {
            ///Cant figure out how to completely redraw the array so best I can do is provide a numbered preview list
            DrawPreviewDropDown();

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

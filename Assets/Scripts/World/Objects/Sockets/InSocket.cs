using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InSocket : Socket
{
    [Range(-0.5f, 0.5f)]
    [SerializeField] private float _attachmentSensitivity = 0;
    [SerializeField] int _requiredAttachmentID;
    [SerializeField] int _createdID;

    private void Awake()
    {
        _in = true;
    }

    private void OnTriggerEnter(Collider other)
    {

        Debug.Log("Called ENTEr" +this.gameObject.name);
        if (_in) // only have the female sockets checking collisions 
        {
            //Debug.Log($"{this.transform.gameObject.name} triggered with {other.gameObject.name}");

            var socket = other.GetComponent<Socket>();
            if (socket && CheckConditions(socket))
            {
                //Debug.Log("match");
                PreviewManager.ShowPreview(_controller, socket._controller, _createdID);
            }
            else if (socket)
            {
                if (_requiredAttachmentID != (int)socket._controller._myID)
                    Debug.Log($"{_requiredAttachmentID} does not match {(int)socket._controller._myID}");
                else if (UserInput.Instance._currentSelection == _controller)
                    Debug.Log(" Cant attach female to male, select and move male part");
                else
                    Debug.Log($"Attachment Angle was invalid");
            }
            //else
            //    Debug.Log("No socket on " + other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_in) // only have the female sockets checking collisions 
        {
            //Debug.Log($"{this.transform.gameObject.name} triggered with {other.gameObject.name}");

            var socket = other.GetComponent<Socket>();
            if (socket && CheckConditions(socket))
            {
                PreviewManager.UndoPreview();
            }

        }
    }

    private bool CheckConditions(Socket socket)
    {
        bool valid = false;

        //Not moving the female part and items match
        if (UserInput.Instance._currentSelection != _controller && _requiredAttachmentID == (int)socket._controller._myID)
        {
            //check the angles of attachment
            Vector3 dir = socket.transform.position - this.transform.position;
            float angle = Vector3.Dot(this.transform.forward.normalized, dir.normalized);
            //Debug.Log($"angle={angle}");
            if (!PreviewManager._inPreview) //OnTriggerEnter
            {
                if (angle > _attachmentSensitivity) // 1 is perfect match 
                    valid = true;
            }
            else  //OnTriggerExit
                valid = true;
        }

        return valid;

    }
}

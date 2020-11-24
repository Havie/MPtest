using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LEAN
{
    public class Socket : MonoBehaviour
    {

        public ObjectController _controller { get; private set; }


        //IN vars
        //not sure about this weird warning
        /*private int _requiredAttachmentID;
        private int _createdID; 
         protected float _inSensitivity = 0;
        */
        protected bool _in;


        private void Start()
        {
            _controller = this.GetComponentInParent<ObjectController>();
        }

    }









}

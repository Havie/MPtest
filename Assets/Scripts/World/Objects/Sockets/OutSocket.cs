using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OutSocket : Socket
{
    enum eAttachmentAngle { HORIZ, VERT}

    [SerializeField] eAttachmentAngle _attachmentAngle = default;
    public float xVelocity { get; private set; }
    public float yVelocity { get; private set; }

    private Vector3 _lastPos;
    public bool AttachesHorizontal => _attachmentAngle == eAttachmentAngle.HORIZ;

    private void Awake()
    {
        _in = false;
        _lastPos = transform.position;
    }
    private void Update()
    {
        ///Keep track of our movement Dir's
        var newPos = transform.position;
        xVelocity = (newPos.x- _lastPos.x );
        yVelocity = (newPos.y -_lastPos.y );
        _lastPos = newPos;

    }

}

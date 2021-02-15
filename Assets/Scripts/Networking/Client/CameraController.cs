using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

     public PlayerManager _player;
    public float _sensitivity = 100f;
    public float _clampAngle = 85f;

    private float _vertRot;
    private float _horiRot;



    // Start is called before the first frame update
    void Start()
    {
        _vertRot = transform.localEulerAngles.x;
        _horiRot = _player.transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        Look();
        Debug.DrawLine(transform.position, transform.forward * 2, Color.red);
    }

    private void Look()
    {
        float mouseV = -Input.GetAxis("Mouse Y");
        float mouseH = Input.GetAxis("Mouse X");

        _vertRot += mouseV * _sensitivity * Time.deltaTime;
        _horiRot += mouseV * _sensitivity * Time.deltaTime;

        _vertRot= Mathf.Clamp(_vertRot, -_clampAngle, _clampAngle);

        transform.localRotation = Quaternion.Euler(_vertRot, 0f, 0f);
        _player.transform.rotation = Quaternion.Euler(0f, _horiRot, 0f);
    }
}

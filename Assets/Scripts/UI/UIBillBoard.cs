using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBillBoard : MonoBehaviour
{
    private void Start()
    {
        this.transform.LookAt(Camera.main.transform);
        this.enabled = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sHostConnection : MonoBehaviour
{
    [SerializeField] GameObject _NetworkManagerPREFAB;
    public void HostConnection()
    {
        if (GameObject.FindObjectOfType<sNetworkManager>() == null && _NetworkManagerPREFAB!=null)
            GameObject.Instantiate(_NetworkManagerPREFAB);
    }
}

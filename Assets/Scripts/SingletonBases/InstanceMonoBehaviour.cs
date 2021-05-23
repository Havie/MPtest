using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InstanceMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
{
    protected InstanceMonoBehaviour() { }

    protected static T _instance;
    public static T Instance => GetInstance();

    private static bool m_applicationIsQuitting = false;

    private static T GetInstance()
    {
        if (m_applicationIsQuitting) { return null; }

        if (_instance == null)
        {
            _instance = FindObjectOfType<T>();
            if (_instance == null)
            {
                ///Mybe i dont want this
                GameObject obj = new GameObject();
                obj.name = typeof(T).Name;
                _instance = obj.AddComponent<T>();
            }
        }
        return _instance;
    }
    protected virtual void Awake()
    {

        if (_instance == null)
            _instance = (T)(object)this;
        else if (_instance != this)
            Destroy(this);
    }

}


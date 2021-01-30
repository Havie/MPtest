using UnityEngine;

public abstract class MonoSingletonBackwards<T> : MonoBehaviour where T : Component
{

    private static T _instance;
    public static T instance => Instance;///hack to avoid refactoring all my old classes
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
        {
            _instance = this as T;

        }
        else if (_instance != this as T)
        {
            Destroy(_instance);
        }

        //if (transform.parent != null)
        //{
        //    Debug.LogWarningFormat(" [Talk To Frank]: Singleton of type {0} cannot be set DontDestroyOnLoad as it is a child of another object", _instance.GetType().ToString());
        //}
    }

    private void SetDontDestroyOnLoad()
    {
        Instance.transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()
    {
        m_applicationIsQuitting = true;
    }
}

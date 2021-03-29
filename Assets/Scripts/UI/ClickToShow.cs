
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class ClickToShow : MonoBehaviour
{
    [SerializeField] GameObject _gameObjectToShow;

    bool shown = false;


    void Awake()
    {
        if (_gameObjectToShow)
            shown = _gameObjectToShow.activeSelf;


    }

    void Start()
    {
        if (shown)
            ClickToShowObject();
    }


    public void ClickToShowObject()
    {
        shown = !shown;
        if (_gameObjectToShow)
            _gameObjectToShow.SetActive(shown);
    }


}

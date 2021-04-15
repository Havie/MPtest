
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

[RequireComponent(typeof(AudioSource))]

public class ClickToShow : MonoBehaviour
{
    [SerializeField] GameObject _gameObjectToShow;
    [SerializeField] AudioClip clip;
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

        AudioSource.PlayClipAtPoint(clip, new Vector3(0, 0, 0));
    }


}

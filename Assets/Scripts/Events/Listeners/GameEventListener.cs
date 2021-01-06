using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Adapted from Ryan Hipples GDC talk:
///https://www.youtube.com/watch?v=iXNwWpG7EhM
/// </summary>
/// <typeparam name="T"></typeparam>
/// 

[DefaultExecutionOrder(-10000)] ///we need to have this load asap to counter race conditions 
public abstract class GameEventListener<T, E, UER> : MonoBehaviour,
    IGameEventListener<T> where E : GameEvent<T> where UER : UnityEvent<T>
{
    [SerializeField]protected E _gameEvent;
    public E Event { get { return _gameEvent; } set { _gameEvent = value; } }

    [SerializeField] UER _unityEventResponse = default;

    public void OnEnable()
    {
        if (!_gameEvent)
            return;

        _gameEvent.RegisterListener(this);

    }

    private void OnDisable()
    {
        if (!_gameEvent)
            return;

        _gameEvent.DeregisterListener(this);
    }

    public void OnEventRaised(T item)
    {
        if (_unityEventResponse !=null)
            _unityEventResponse.Invoke(item);
    }
}

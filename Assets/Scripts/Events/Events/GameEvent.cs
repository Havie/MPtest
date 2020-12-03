using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Adapted from Ryan Hipples GDC talk:
///https://www.youtube.com/watch?v=iXNwWpG7EhM
/// </summary>
/// <typeparam name="T"></typeparam>


public abstract class GameEvent<T> : ScriptableObject
{

    private readonly List<IGameEventListener<T>> _listeners = new List<IGameEventListener<T>>();


    public void RegisterListener(IGameEventListener<T> listener)
    {
        if (!_listeners.Contains(listener))
            _listeners.Add(listener);
    }

    public  void DeregisterListener(IGameEventListener<T> listener)
    {
        if (_listeners.Contains(listener))
            _listeners.Remove(listener);
    }


    public  void Raise(T type)
    {
       //Debug.Log("Raise was called count=" + _listeners.Count);
       for(int i = _listeners.Count-1; i>=0; --i)
        {
            _listeners[i].OnEventRaised(type);
        }
    }

}

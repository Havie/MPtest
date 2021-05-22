using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoSingleton<VFXManager>
{

    private Dictionary<GameObject, ParticleSystem> _vfxMap = new Dictionary<GameObject, ParticleSystem>();

    private bool _copyingQueue = false;
    Queue<ParticleSystem> _toBeStopped = new Queue<ParticleSystem>();
    float _timeSinceLastQueue = 0;
    float _delay = 25f;

    /************************************************************************************************************************/

    private void LateUpdate()
    {
        CheckIfTimeToStop();
    }

    public void PerformEffect(GameObject prefab, Transform location, bool isLooping)
    {

        ///do any VFX 
        if (prefab != null)
        {
            if (_vfxMap.TryGetValue(prefab, out ParticleSystem vfx))
            {
                if (vfx != null)
                {
                    vfx.Stop();
                    vfx.transform.parent = location;
                    vfx.transform.localPosition = Vector3.zero;
                    vfx.Play();
                    if (!isLooping)
                    {
                        if (_toBeStopped.Contains(vfx))
                        {
                            _toBeStopped = RemoveFromQueue(vfx);
                            _toBeStopped.Enqueue(vfx);
                        }
                        else
                            _toBeStopped.Enqueue(vfx);
                    }
                }
                else
                {
                    _vfxMap.Remove(prefab);
                    PerformEffect(prefab, location, isLooping);
                    if (!isLooping &&  _toBeStopped.Contains(vfx))
                    {
                        _toBeStopped = RemoveFromQueue(vfx);
                    }
                }
            }
            else
            {
                var go = GameObject.Instantiate<GameObject>(prefab, location);
                if (go)
                {
                    vfx = go.GetComponent<ParticleSystem>();
                    vfx.Play();
                    _vfxMap.Add(prefab, vfx);
                    if (!isLooping)
                    {
                        _toBeStopped.Enqueue(vfx);
                    }
                }
            }
        }

    }

    public void StopEffect(GameObject prefab)
    {
        if (_vfxMap.TryGetValue(prefab, out ParticleSystem vfx))
        {
            if (vfx != null)
            {
                vfx.Stop();
                if (_toBeStopped.Contains(vfx))
                {
                    _toBeStopped = RemoveFromQueue(vfx);
                }
            }
        }
    }

    /************************************************************************************************************************/

    /// <summary> Removes an item from and reorders the queue</summary>
    private Queue<ParticleSystem> RemoveFromQueue(ParticleSystem toRemove)
    {
        _copyingQueue = true;
        Queue<ParticleSystem> copy = new Queue<ParticleSystem>();
        while (_toBeStopped.Count > 0)
        {
            var item = _toBeStopped.Dequeue();
            if (item != toRemove)
                copy.Enqueue(item);
        }


        _copyingQueue = false;
        return copy;

    }

    /// <summary> Determines whether its time to stop playing an effect from our queue</summary>
    private void CheckIfTimeToStop()
    {
        if (_toBeStopped.Count != 0 && !_copyingQueue)
        {
            _timeSinceLastQueue -= Time.deltaTime;
            if (_timeSinceLastQueue < 0)
            {
                var item = _toBeStopped.Dequeue();
                if (item)
                {
                    item.Stop();
                }
                else
                {
                    Debug.Log($"<color=yellow>vfx missing from queue</color>");
                }
                SetNextDuration();
            }
        }
    }
    /// <summary>Sets the nextTime delay based off items in vfx queue </summary>
    private void SetNextDuration()
    {
        if (_toBeStopped.Count != 0)
        {
            var next = _toBeStopped.Peek();
            if (next.main.loop == false) //does VFX loop
            {
                _timeSinceLastQueue = next.main.duration;
                return;
            }
        }
        _timeSinceLastQueue = _delay;
    }

}

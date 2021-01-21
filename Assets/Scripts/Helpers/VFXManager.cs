using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    private Dictionary<GameObject, ParticleSystem> _vfxMap = new Dictionary<GameObject, ParticleSystem>();

    private bool _copyingQueue = false;
    Queue<ParticleSystem> _toBeStopped = new Queue<ParticleSystem>();
    float _timeSinceLastQueue = 0;
    float _delay = 25f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

    }

    void LateUpdate()
    {
        CheckIfTimeToStop();
    }


    public void PerformEffect(GameObject prefab, Transform location)
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
                    if (_toBeStopped.Contains(vfx))
                    {
                        _toBeStopped = RemoveFromQueue(vfx);
                        _toBeStopped.Enqueue(vfx);
                    }
                    else
                        _toBeStopped.Enqueue(vfx);
                }
                else
                {
                    _vfxMap.Remove(prefab);
                    PerformEffect(prefab, location);
                    if (_toBeStopped.Contains(vfx))
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
                    _toBeStopped.Enqueue(vfx);
                }
            }
        }

    }

    Queue<ParticleSystem> RemoveFromQueue(ParticleSystem toRemove)
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

    void CheckIfTimeToStop()
    {
        if (_toBeStopped.Count != 0 && !_copyingQueue)
        {
            _timeSinceLastQueue -= Time.deltaTime;
            if (_timeSinceLastQueue < 0)
            {
                var item = _toBeStopped.Dequeue();
                item.Stop();
                SetNextDuration();

            }
        }
    }

    void SetNextDuration()
    {
        if (_toBeStopped.Count != 0)
        {
            var next = _toBeStopped.Peek();
            if (next.main.loop == false)
            {
                _timeSinceLastQueue = next.main.duration;
                return;
            }
        }
        _timeSinceLastQueue = _delay;
    }

}

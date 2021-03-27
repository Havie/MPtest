using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class ShippingListener : MonoBehaviour, IGameEventListener<BatchWrapper> //GameEventListener<BatchWrapper, BatchEvent, UnityBatchEvent>
{
    [SerializeField] BatchEvent _batchEvent;


    private void Awake()
    {
        _batchEvent.RegisterListener(this);
    }


    public void OnEventRaised(BatchWrapper batch)
    {
        //Gave up on this class, and am doing this thru BatchSent() now
        //ClientSend.Insance.BatchShipped(batch);
    }

    private void OnDisable()
    {
        _batchEvent.DeregisterListener(this);
    }
}

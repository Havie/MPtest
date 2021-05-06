using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIShipping : UIOrdersIn
{
    [Header("Event")]
    [SerializeField] BatchEvent _batchSentEvent;

    protected override bool CheckShouldDropParts()
    {
        return false;
    }

    protected void ShipItem()
    {
        ///This is kind of hacky, but since bSlots do not auto send per item any more,
        ///we have to mimic the OutStations batch send event,
        ///this will have to evolve in lean version 2 to actually encode the proper batch data
        if (_batchSentEvent)
        {
            WorkStation ws = GameManager.Instance._workStation;
            _batchSentEvent.Raise(new BatchWrapper((int)ws._myStation, 1, ws.IsShippingStation()));
        }
    }

    protected override IEnumerator ButtonShipped(OrderButton orderButton)
    {
        yield return base.ButtonShipped(orderButton);
        ShipItem();
    }
}

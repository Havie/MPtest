#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class sOrderManager
{
    private int _orderFrequency;
    private int _expectedDeliveryTime;
    private float _timeToOrder;
    List<int> _revelantClientIDs;

    private bool _roundActive;
    /************************************************************************************************************************/

    public sOrderManager()
    {
        _revelantClientIDs = new List<int>();
        Reset();
    }
    /************************************************************************************************************************/

    public void RegisterClientID(int clientId)
    {
        if (_revelantClientIDs.Contains(clientId))
            return;

        //Debug.Log($"<color=white>RegisterClientID</color>: {clientId}");
        _revelantClientIDs.Add(clientId);
    }
    public void UnregisterClientID(int clientId)
    {
        if (!_revelantClientIDs.Contains(clientId))
            return;
        //Debug.Log($"<color=red>UNRegisterClientID</color>: {clientId}");
        _revelantClientIDs.Remove(clientId);
    }
    public void Reset()
    {
        _timeToOrder = float.MaxValue;
        _roundActive = false;
        _revelantClientIDs.Clear();
    }
    public void BeginRound(int orderFrequency, int expectedDeliveryDelay)
    {
        _orderFrequency = orderFrequency;
        _expectedDeliveryTime = expectedDeliveryDelay;
        _roundActive = true;
    }
    public void EndRound()
    {
        _roundActive = false;
    }
    public void Tick()
    {
        if (!_roundActive)
            return;
        
        if (_timeToOrder > _orderFrequency)
            SendInNewOrder();
        else
            _timeToOrder += Time.deltaTime;
    }
    /************************************************************************************************************************/

    private void SendInNewOrder()
    {
         _timeToOrder = 0;
        ///Invoke call that calls client who owns UIOrdersIn.cs . SendInNewOrder()
        foreach (var clientID in _revelantClientIDs)
        {
            int itemID = -1; //TODO
            float currTime = Time.time;
            sServerSend.NewOrderCreated(clientID, itemID, currTime, currTime + _expectedDeliveryTime);
        }
    }

}

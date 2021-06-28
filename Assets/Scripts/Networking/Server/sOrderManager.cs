#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class sOrderManager
{
    private int _orderFrequency;
    private int _expectedDeliveryTime;
    private float _timeToOrder;
    List<int> _clientIDsTakeCare;

    private bool _roundActive;
    /************************************************************************************************************************/

    public sOrderManager(int orderFrequency, int expectedDeliveryDelay)
    {
        _orderFrequency = orderFrequency;
        _expectedDeliveryTime = expectedDeliveryDelay;
        _timeToOrder = float.MaxValue;
        _clientIDsTakeCare = new List<int>();
        _roundActive = false;
    }
    /************************************************************************************************************************/

    public void RegisterClientID(int clientId)
    {
        if (_clientIDsTakeCare.Contains(clientId))
            return;

        _clientIDsTakeCare.Add(clientId);
    }
    public void UnregisterClientID(int clientId)
    {
        if (!_clientIDsTakeCare.Contains(clientId))
            return;

        _clientIDsTakeCare.Remove(clientId);
    }
    public void Reset()
    {
        _timeToOrder = 0;
        _clientIDsTakeCare.Clear();
    }
    public void BeginRound()
    {
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
        Debug.Log($"<color=white>WANT to send in new order..? </color>");
        _timeToOrder = 0;
        ///Invoke call that calls client who owns UIOrdersIn.cs . SendInNewOrder()
        foreach (var clientID in _clientIDsTakeCare)
        {
            int itemID = -1; //TODO
            float currTime = Time.time;
            sServerSend.NewOrderCreated(clientID, itemID, currTime, currTime + _expectedDeliveryTime);
            Debug.Log($"<color=green>Sent in a new order!</color>");
        }
    }

}

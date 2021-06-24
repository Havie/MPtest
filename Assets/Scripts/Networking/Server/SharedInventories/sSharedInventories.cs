#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class sSharedInventories
{
    private enum eInventoryType { IN, OUT }
    private List<SharedInventory> _inventories = new List<SharedInventory>();

    private Dictionary<int, int> _stationIdsToClientIds = new Dictionary<int, int>();
    private Dictionary<int, int> _clientIdsToStationIds = new Dictionary<int, int>();

    public void RegisterClientToStationId(int clientID, int stationID)
    {
        //Debug.Log($"<color=green>..register </color>client#:{clientID} to station#:{stationID}");
        _stationIdsToClientIds[stationID] = clientID;
        _clientIdsToStationIds[clientID] = stationID;
    }
    public int GetClientIDForStation(int stationID)
    {
        //Debug.Log($"..trying to get client for stationID#:{stationID}");
        if (_stationIdsToClientIds.TryGetValue(stationID, out int clientID))
        {
            return clientID;
        }
        return -1;
    }
    private int GetStationIDForClient(int clientID)
    {
        if (_clientIdsToStationIds.TryGetValue(clientID, out int stationID))
        {
            return stationID;
        }
        return -1;
    }
    ///Something  monitors all the inventories and maps them by getting who they send to
    public void BuildInventory(int inStationID, int outStationID, float distance)
    {
        ///This is flipped because of how they are shared
        _inventories.Add(new SharedInventory(outStationID, inStationID, distance));
    }
    /// <summary>
    /// Returns the inventory correlated to this client for IN/OUT
    /// </summary>
    public SharedInventory GetSharedInventory(bool isInInventory, int fromClientID)
    {
        int stationID = GetStationIDForClient(fromClientID);
        eInventoryType type = isInInventory ? eInventoryType.IN : eInventoryType.OUT;
        //Debug.Log($"..trying to get station for clientID#:{fromClientID} was : {stationID}:{type}");
        return  FindAnInventory(type, stationID);
    }
    public int GetSharedStationID(bool isInInventory, int fromClientID, out float delayTime)
    {
        int stationID = GetStationIDForClient(fromClientID);
        eInventoryType type = isInInventory ? eInventoryType.IN : eInventoryType.OUT;
        //Debug.Log($"..trying to get station for clientID#:{fromClientID} was : {stationID}:{type}");
        var inv= FindAnInventory(type, stationID);
        if (inv != null)
        {
            delayTime = inv.TransportDelay;
            return isInInventory ? inv.OutOwnerID : inv.InOwnerID;
        }
        delayTime = 0;
        return -1;
    }
    /************************************************************************************************************************/
    /// <summary>
    /// Finds the inventory where the typed is mapped to the clients ID, null if not found
    /// </summary>
    private SharedInventory FindAnInventory(eInventoryType type, int stationID)
    {
        foreach (var inv in _inventories)
        {
            int idOwner = GetStationIDForInventoryByType(inv, type);
            Debug.Log($"Gotten ID for type {type} was : {idOwner}");
            if (idOwner == stationID)
            {
                return inv;
            }
        }

        return null;
    }
    /// <summary>
    /// Returns the ID for client for this inventories mapped type
    /// </summary>
    private int GetStationIDForInventoryByType(SharedInventory inv, eInventoryType type)
    {
        if (type == eInventoryType.IN)
        {
            return inv.InOwnerID;
        }
        return inv.OutOwnerID;
    }

}
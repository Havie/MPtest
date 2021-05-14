#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class sSharedInventories
{
    private enum eInventoryType { IN, OUT }
    private List<SharedInventory> _inventories;

    private Dictionary<int, int> _stationIdsToClientIds = new Dictionary<int, int>();
    private Dictionary<int, int> _clientIdsToStationIds = new Dictionary<int, int>();

    public void RegisterClientToStationId(int clientID, int stationID)
    {
        _stationIdsToClientIds[stationID] = clientID;
        _clientIdsToStationIds[clientID] = stationID;
    }
    public int GetClientIDForStation(int stationID)
    {
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
    public void BuildInventory(int inStationID, int outStationID, sServerHandle.KanbanChangedEvent onChanged)
    {
        _inventories.Add(new SharedInventory(inStationID, outStationID, onChanged));
    }

    public void AddedItem(bool isInInventory, int clientID)
    {
        AlterInventory(isInInventory, clientID, false);
    }
    public void RemovedItem(bool isInInventory, int clientID)
    {
        AlterInventory(isInInventory, clientID, true);
    }
    /// <summary>
    /// If found, sets an inventory to either inUse or Empty
    /// </summary>
    private void AlterInventory(bool isInInventory, int clientID, bool isEmpty)
    {
        int stationID = GetStationIDForClient(clientID);
        eInventoryType type = isInInventory ? eInventoryType.IN : eInventoryType.OUT;
        var inventory = FindAnInventory(type, stationID);
        if (inventory != null)
        {
            inventory.SetInUse(!isEmpty, stationID);
        }
    }
    /// <summary>
    /// Finds the inventory where the typed is mapped to the clients ID, null if not found
    /// </summary>
    private SharedInventory FindAnInventory(eInventoryType type, int stationID)
    {
        foreach (var inv in _inventories)
        {
            int idOwner = GetClientIDForInventoryByType(inv, type);
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
    private int GetClientIDForInventoryByType(SharedInventory inv, eInventoryType type)
    {
        if (type == eInventoryType.IN)
        {
            return inv.InOwnerID;
        }
        return inv.OutOwnerID;
    }

}



public class SharedInventory
{

    private int _inID;
    private int _outID;
    private bool _isEmpty;
    private sServerHandle.KanbanChangedEvent _onChanged;

    public SharedInventory(int inID, int outID, sServerHandle.KanbanChangedEvent onChanged)
    {
        _inID = inID;
        _outID = outID;
        _isEmpty = false;
        _onChanged = onChanged;
    }

    public int InOwnerID => _inID;
    public int OutOwnerID => _outID;
    public bool IsEmpty => _isEmpty;
    public bool IsInUse => !IsEmpty;

    public void SetInUse(bool cond, int changedByStationID)
    {
        _isEmpty = !cond;


        bool changerWasInInventory = changedByStationID == _inID;
        ///Set the clientID for the caller who changed the inventory state
        int caller = changerWasInInventory ? _inID : _outID;
        ///Set the clientID for the client who needs to know of the state change
        int needsToKnow = !changerWasInInventory ? _inID : _outID;

        _onChanged?.Invoke(caller, needsToKnow, changerWasInInventory, _isEmpty);
    }

}
#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

public class sSharedInventories
{
    public enum eInventoryType { IN, OUT }
    private List<SharedInventory> _inventories;

    //TODO
    ///SomethingSomewhere ,looks at all the IN inventories and maps them by getting who they send to

    public void BuildInventory(int inClientdID, int outClientID)
    {
        _inventories.Add(new SharedInventory(inClientdID, outClientID, sServerHandle.KanbanFlagChanged));
    }


    public void AddedItem(eInventoryType type, int myID)
    {
        AlterInventory(type, myID, false);
    }
    public void RemovedItem(eInventoryType type, int myID)
    {
        AlterInventory(type, myID, true);
    }
    /// <summary>
    /// If found, sets an inventory to either inUse or Empty
    /// </summary>
    private void AlterInventory(eInventoryType type, int myID, bool isEmpty)
    {
        var inventory = FindAnInventory(type, myID);
        if (inventory != null)
        {
            inventory.SetInUse(!isEmpty, myID);
        }
    }
    /// <summary>
    /// Finds the inventory where the typed is mapped to the clients ID, null if not found
    /// </summary>
    private SharedInventory FindAnInventory(eInventoryType type, int myID)
    {
        foreach (var inv in _inventories)
        {
            int idOwner = GetClientIDForInventoryByType(inv, type);
            if (idOwner == myID)
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
    private sServerHandle.InventoryChanged _onChanged;

    public SharedInventory(int inID, int outID, sServerHandle.InventoryChanged onChanged)
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

    public void SetInUse(bool cond, int byClient)
    {
        _isEmpty = !cond;

        ///Set the clientID for the caller who changed the inventory state
        int caller = byClient == _inID ? _inID : _outID ;
        ///Set the clientID for the client who needs to know of the state change
        int needsToKnow = byClient != _inID ? _inID : _outID;

        _onChanged?.Invoke(caller, needsToKnow, _isEmpty);
    }

}
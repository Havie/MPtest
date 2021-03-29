using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value.

public class DropPartsOnClick : MonoBehaviour
{
    [SerializeField] PartDropper _dropper;
    [SerializeField] UIInventoryManager _inventory;



    private void OnEnable()
    {
        Debug.Log("pls");
    }

    /// <summary>Called from button </summary>
    public void DropPartsFromInventory()
    {
        if (_dropper && _inventory)
        {

            ///Have to pull out whatever is Stored in slot data And pass it to the partDropper
            _dropper.DropPartsOnDemand(_inventory.GetAllSlotsInUse());



        }

    }

}

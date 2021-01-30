using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropPartsOnClick : MonoBehaviour
{
    [SerializeField] PartDropper _dropper;
    [SerializeField] UIInventoryManager _inventory;


    public void DropPartsFromInventory()
    {
        if (_dropper && _inventory)
        {

            ///Have to pull out whatever is Stored in slot data And pass it to the partDropper
            _dropper.DropPartsOnDemand(_inventory.GetAllSlotsInUse());



        }

    }

}

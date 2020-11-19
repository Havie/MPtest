﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PreviewManager 
{
    
    private static List<ObjectController> _previewedItems = new List<ObjectController>();
    private static GameObject _previewItem;
    public static bool _inPreview { get; private set; }


    public static void ShowPreview(ObjectController controller, ObjectController otherController, int createdID)
    {
        if(_inPreview)
        {
            Debug.LogError("trying to preview again too fast ??");
            return;
        }

        ///disable both items mesh renderers
        controller.ChangeAppearanceHidden(true);
        otherController.ChangeAppearanceHidden(true);
        ///Store for later to undo
        _previewedItems.Add(controller);
        _previewedItems.Add(otherController);
          //Spawn a new obj via CreatedID and set opacity to preview 
        //Debug.LogError("createdid=" + createdID);
        var obj = BuildableObject.Instance.SpawnObject(createdID);
        obj.GetComponent<ObjectController>().ChangeAppearancePreview();
        ///Set its orientation to match its female parent
        obj.transform.position = controller.gameObject.transform.position;
        obj.transform.rotation = controller.gameObject.transform.rotation;
        _previewItem = obj;
        _inPreview = true;
    }

    public static void UndoPreview()
    {
        foreach (var item in _previewedItems)
        {
            item.ChangeAppearanceNormal();
        }
        BuildableObject.Instance.DestroyObject(_previewItem);
        ResetSelf();
    }

    public static void ConfirmCreation()
    {
        //Debug.Log("....called Confirm Creation ");

        List<ObjectQuality> qualities = new List<ObjectQuality>();

        foreach (var item in _previewedItems)
        {
            var overallQuality = item.GetComponent<OverallQuality>();
            if(overallQuality)
            {
                foreach (var quality in overallQuality._qualities)
                {
                    qualities.Add(quality);
                }
            }

            HandManager.RemoveDeletedItem(item);
            // HandManager.PrintQueue();
            BuildableObject.Instance.DestroyObject(item.gameObject);
        }
        var oc = _previewItem.GetComponent<ObjectController>();
        oc.ChangeAppearanceNormal();
        HandManager.PickUpItem(oc);
        ///Update our overall quality, passing the data to the next object 
        var finalQuality =_previewItem.GetComponent<OverallQuality>();
        if(finalQuality)
        {
            foreach (var q in qualities)
            {
                finalQuality.ReadOutQuality(q); //how is this not null/missing if we destroyed obj above?
            }
        }

        ResetSelf();
    }

    private static void ResetSelf()
    {
        _previewedItems.Clear();
        _previewItem = null;
        _inPreview = false;
    }
  
}

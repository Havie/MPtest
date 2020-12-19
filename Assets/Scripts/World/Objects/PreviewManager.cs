using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PreviewManager 
{
    
    private static List<ObjectController> _previewedItems = new List<ObjectController>();
    private static GameObject _previewItem;
    public static bool _inPreview { get; private set; }

    private static bool _inMiddleOfClear = false;


    public static void ShowPreview(ObjectController controller, ObjectController otherController, int createdID)
    {
        if(_inPreview)
        {
            Debug.LogWarning("trying to preview again too fast ??");
            return;
        }

        if (UserInput.Instance._state != UserInput.eState.DISPLACEMENT)
            return; /// would feel cleaner to cache on the object, but extra work

        Debug.Log($"Show Preview heard for createID={createdID}:{(ObjectManager.eItemID)createdID}");

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

        if (_inMiddleOfClear)
        {
            Debug.LogWarning("trying to ConfirmCreation again too fast ??");
            return;
        }
        _inMiddleOfClear = true;

        List<ObjectQuality> qualities = new List<ObjectQuality>();

        foreach (var item in _previewedItems)
        {
            var overallQuality = item.GetComponent<OverallQuality>();
            if(overallQuality)
            {
                foreach (var quality in overallQuality.Qualities)
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
                finalQuality.ReadOutQuality(q);
            }
        }

        ResetSelf();
        _inMiddleOfClear = false;
    }

    private static void ResetSelf()
    {
        _previewedItems.Clear();
        _previewItem = null;
        _inPreview = false;
    }
  
}

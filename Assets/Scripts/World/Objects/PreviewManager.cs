using System.Collections;
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
        controller.ChangeAppearanceHidden();
        otherController.ChangeAppearanceHidden();
        ///Store for later to undo
        _previewedItems.Add(controller);
        _previewedItems.Add(otherController);
          //Spawn a new obj via CreatedID and set opacity to preview 
        //Debug.LogError("createdid=" + createdID);
        var obj = BuildableObject.Instance.SpawnObject(createdID);
        obj.GetComponent<ObjectController>().ChangeApperancePreview();
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
            item.ChangeApperanceNormal();
        }
        BuildableObject.Instance.DestroyObject(_previewItem);
        ResetSelf();
    }

    public static void ConfirmCreation()
    {
        //Debug.Log("....called Confirm Creation ");
        foreach (var item in _previewedItems)
        {
            HandManager.RemoveItem(item);
           // HandManager.PrintQueue();
            BuildableObject.Instance.DestroyObject(item.gameObject);
        }
        _previewItem.GetComponent<ObjectController>().ChangeApperanceNormal();
        ResetSelf();
    }

    private static void ResetSelf()
    {
        _previewedItems.Clear();
        _previewItem = null;
        _inPreview = false;
    }
  
}

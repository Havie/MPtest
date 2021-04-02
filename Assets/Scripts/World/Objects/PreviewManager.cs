using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UserInput;         ///TODO wish i cud clean up this dependency to UserInputManager

public static class PreviewManager
{

    private static List<ObjectController> _previewedItems = new List<ObjectController>();
    private static GameObject _previewItem;
    public static bool _inPreview { get; private set; }

    private static bool _inMiddleOfClear = false;

    private static GameObject _switchOGParent;


    public static void ShowPreview(ObjectController controller, ObjectController otherController, int createdID)
    {
        if (_inPreview)
        {
            Debug.LogWarning("trying to preview again too fast ??");
            return;
        }

        ///Hack , should abstract this into the UserInputManagerClass
        if (UserInputManager.Instance._currentState != UserInputManager.Instance._displacementState)
            return; /// would feel cleaner to cache on the object, but extra work

        //Debug.Log($"Show Preview heard for createID={createdID}:{(ObjectManager.eItemID)createdID} , controller={controller} otherController={otherController}");

        ///disable both items mesh renderers
        controller.ChangeAppearanceHidden(true);
        otherController.ChangeAppearanceHidden(true);

        ///Store for later to undo
        _previewedItems.Add(controller);
        _previewedItems.Add(otherController);
        //Spawn a new obj via CreatedID and set opacity to preview 
        //Debug.LogError("createdid=" + createdID);
        var obj = ObjectManager.Instance.SpawnObject(createdID);
        ///Set its orientation to match its female parent
        obj.transform.position = controller.gameObject.transform.position;
        obj.transform.rotation = controller.gameObject.transform.rotation;
        var newController = obj.GetComponent<ObjectController>();
        newController.ChangeAppearancePreview();
        FixRotationOnPreviewItem(newController);
        _previewItem = obj;
       // CheckForSwitch();
        _inPreview = true;
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

        List<QualityObject> qualities = new List<QualityObject>();

        foreach (var item in _previewedItems)
        {
            var overallQuality = item.GetComponent<QualityOverall>();
            if (overallQuality)
            {
                foreach (var quality in overallQuality.Qualities)
                {
                    qualities.Add(quality);
                }
            }

            HandManager.RemoveDeletedItem(item);
            // HandManager.PrintQueue();
            ObjectManager.Instance.DestroyObject(item.gameObject);
        }
        var oc = _previewItem.GetComponent<ObjectController>();
        HandManager.PickUpItem(oc);
        oc.ChangeAppearanceNormal(); ///do this after picking it up so we dont reset rotation
        ///Update our overall quality, passing the data to the next object 
        var finalQuality = _previewItem.GetComponent<QualityOverall>();
        if (finalQuality)
        {
            foreach (var q in qualities)
            {
                finalQuality.ReadOutQuality(q);
            }
        }

        ResetSelf();
        _inMiddleOfClear = false;
    }

    public static void UndoPreview()
    {
        foreach (var item in _previewedItems)
        {
            item.ChangeAppearanceNormal();
        }
        //CheckForSwitch();
        ObjectManager.Instance.DestroyObject(_previewItem);
        ResetSelf();
    }


    /************************************************************************************************************************/
    // Private/Helpers
    /************************************************************************************************************************/



    private static void FixRotationOnPreviewItem(ObjectController newItem)
    {
        foreach(var newQuality in newItem.GetComponentsInChildren<QualityObject>())
        {
            if(newQuality.QualityStep._qualityAction == QualityAction.eActionType.ROTATE)
            {
                ///check if this QualityStep Exists on either of the previewItems
                foreach (var controller in _previewedItems)
                {
                    foreach (var existingQuality in controller.GetComponentsInChildren<QualityObject>())
                    {
                        if (existingQuality.QualityStep == newQuality.QualityStep)
                        {
                            ///Transfer the rotation:
                            var oldRot = existingQuality.transform.rotation;
                            newQuality.transform.rotation = oldRot;
                            //Debug.Log($"Trans old {existingQuality.gameObject} to new {newQuality.gameObject}");
                        }
                    }
                }
            }
        }
    }

    private static void ResetSelf()
    {
        _previewedItems.Clear();
        _previewItem = null;
        _inPreview = false;
    }



    ///This was to copyTheSwitch over, but going to put on each prefab now
    /*
    private static void CheckForSwitch()
    {
        if (!_inPreview)
        {
            foreach (var item in _previewedItems)
            {
                Switch s = item.GetComponentInChildren<Switch>();
                if (s != null)
                {
                    var transform = s.transform;
                    Vector3 localPos = transform.localPosition;
                    Quaternion localRot = transform.localRotation;
                    transform.parent = _previewItem.transform;
                    transform.localPosition = localPos;
                    transform.localRotation = localRot;
                    s.ShowInPreview();
                    _switchOGParent = item.gameObject;
                    return;
                }

            }
        }
        else ///switch back via UndoPreview
        {
            Switch s = _previewItem.GetComponentInChildren<Switch>();
            if (s != null)
            {
                var transform = s.transform;
                Vector3 localPos = transform.localPosition;
                Quaternion localRot = transform.localRotation;
                s.transform.parent = _switchOGParent.transform;
                transform.localPosition = localPos;
                transform.localRotation = localRot;
                s.ShowNormal();
                _switchOGParent = null;

            }
        }
    }
    */
}

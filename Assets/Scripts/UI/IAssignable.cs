using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAssignable
{
    bool GetInUse();
    bool PreviewSlot(Sprite img);
    void UndoPreview();
    void RemoveItem();
    bool  AssignItem(ObjectController oc, int count);

    bool AssignItem(int id, int count, List<QualityData> qualities);

    bool RequiresCertainID();
}

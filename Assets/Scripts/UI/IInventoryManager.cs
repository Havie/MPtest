

using System.Collections.Generic;

public interface IInventoryManager
{
    void ItemAssigned(UIInventorySlot slot);

    bool TryAssignItem(int id, int count, List<QualityObject> qualities);
}


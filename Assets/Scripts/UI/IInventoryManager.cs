

using System.Collections.Generic;

public interface IInventoryManager
{
    void SlotStateChanged(UIInventorySlot slot);

    bool TryAssignItem(int id, int count, List<QualityData> qualities);
}


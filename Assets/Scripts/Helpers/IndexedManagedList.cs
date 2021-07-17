#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
	public class IndexedManagedList<T,E> : ManagedList<T,E> where T: MonoBehaviour
	{
	     
        /************************************************************************************************************************/
        public int GetLastIndex()
        {
            return _infiniteItemList.Count -1 ;
        }

        public int GetIndexOfManagedItem(T item)
        {
            for (int i = 0; i < _infiniteItemList.Count; i++)
            {
                if(_infiniteItemList[i]==item)
                {
                    return i;
                }
            }
            return -1;
        }

		public bool EnactOnManagedItemByIndex(int index, System.Action<T> action)
        {
            if(index>-1 && _infiniteItemList.Count > index)
            {
                action?.Invoke(_infiniteItemList[index]);
                return true;
            }
            return false;
        }
        public T GetManagedItemAtIndex(int index)
        {
            if (index < 0 || index > _infiniteItemList.Count - 1)
                return null;
            return _infiniteItemList[index];
        }

	}
}

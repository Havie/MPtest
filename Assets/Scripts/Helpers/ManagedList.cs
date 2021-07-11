#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;

namespace Helpers
{
    public class ManagedList<T, E> where T : MonoBehaviour
    {

        private Transform _transform;
        private static T _itemPREFAB;
        List<T> _infiniteItemList = new List<T>();
        System.Action<T> _initalizationAction;
        System.Action<int, T, E> _unboxingAction;
        private static bool _initalized = false;
        /************************************************************************************************************************/
        public void Init(Transform t, T itemPrefab)
        {
            _transform = t;
            _itemPREFAB = itemPrefab;
            _initalized = true;
        }
        public void Init(Transform t, T itemPrefab, System.Action<int, T, E> unboxingAction)
        {
            _unboxingAction = unboxingAction;
            Init(t, itemPrefab);
        }
        public void Init(Transform t, T itemPrefab, System.Action<T> initalizationAction)
        {
            _initalizationAction = initalizationAction;
            Init(t, itemPrefab);
        }
        public void Init(Transform t, T itemPrefab, System.Action<T> initalizationAction, System.Action<int, T, E> unboxingAction)
        {
            _initalizationAction = initalizationAction;
            Init(t, itemPrefab, unboxingAction);
        }
        /************************************************************************************************************************/

        public void DisplayList(List<E> listOfItemsToDisplay)
        {
            if (!_initalized)
                return;
            ///Lazy way, just turn them all off 
            TurnOffUnused();
            for (int i = 0; i < listOfItemsToDisplay.Count; ++i)
            {
                if (_infiniteItemList.Count <= i)
                {
                    CreateItem();
                }
                T item = _infiniteItemList[i];
                /// Action to cast If there is one
                _unboxingAction?.Invoke(i, item, listOfItemsToDisplay[i]);
                ///Turn back on the ones we need
                item.gameObject.SetActive(true);

            }

        }

        public void PerformActionOnAllItems(System.Action<T> action)
        {
            foreach (var item in _infiniteItemList)
            {
                action?.Invoke(item);
            }
        }
       
        public T GetFirstItemInList()
        {
            if (!_initalized)
                return null;
            return _infiniteItemList[0];
        }
        /************************************************************************************************************************/

        private void TurnOffUnused()
        {
            foreach (var item in _infiniteItemList)
            {
                item.gameObject.SetActive(false);
            }
        }

        private void CreateItem()
        {
            if (!_initalized)
            {
                return;
            }
            T newItem = GameObject.Instantiate(_itemPREFAB, _transform).GetComponent<T>();
            newItem.gameObject.name = "ManagedItem_" + _infiniteItemList.Count;
            /// perform our initialization action if there is one
            _initalizationAction?.Invoke(newItem);
            _infiniteItemList.Add(newItem);
        }
    }
}

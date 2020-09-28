using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/** Keeps Track of our sprites and game objects Associations  */
[CreateAssetMenu(fileName = "ObjectManager", menuName = "ObjectManager")]
public class ObjectManager : ScriptableObject
{


    [SerializeField] List<Sprite> _sprites;
    [SerializeField] List<GameObject> _objects;



    public Sprite GetSprite(int level)
    {
        return _sprites[level];
    }

    public GameObject GetObject(int level)
    {
       return _objects[level];

    }
}

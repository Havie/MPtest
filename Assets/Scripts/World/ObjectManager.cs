using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/** Keeps Track of our sprites and game objects Associations  */
[CreateAssetMenu(fileName = "ObjectManager", menuName = "ObjectManager")]
public class ObjectManager : ScriptableObject
{

    public enum eItemID { sHusk, sIron, sBladeHot, sBlade, sHandle, sSword, sHilt, sFinal,
                          aHusk, aIron, aBladeHot, aBlade, aHandle, aSword, aHilt, aFinal };

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

    public string getItemName(int level)
    {
       eItemID tmp= (eItemID)level;
        return tmp.ToString(); //figure something else out later
    }
}

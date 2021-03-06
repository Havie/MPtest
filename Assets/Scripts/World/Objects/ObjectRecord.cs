﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/** Keeps Track of our sprites and game objects Associations  */
[CreateAssetMenu(fileName = "ObjectRecord", menuName = "ObjectRecord")]
public class ObjectRecord : ScriptableObject
{

    public enum eItemID
    {
        GreenRect1 = 1, BlueBolt, PinkTop, RedBot, YellowPart, PurplePlug, PinkTopwYellow,
        RectwBolts1, RectwBolts2, RectwTopYellow, RectwBotTopYellow, RectwTopBotPurplePlug, finalPower,
        RectwBoltsAlt, RectwTopAltMissingBotBolt, RectwBotAlt,RectWTopBotAlt1, RectwRedBotMissingTopBolt, RectwTopAltBoltBot,
        RectwTopYellowMissingBotBolt, RectwTopPurplePlugBotBolt, RectwTopPurplePlugMissingBotBolt, YellowPurplePlug, PinkwPurplePlug, IGNORE
    };

    [SerializeField] List<Sprite> _sprites = default;
    [SerializeField] List<GameObject> _objects = default;

    public bool IsBasicItem(eItemID item)
    {
        return (int)item < 7;
    }

    public Sprite GetSprite(int level)
    {
        return _sprites[level];
    }

    public GameObject GetObject(int level)
    {
        if (level > _objects.Count-1 || level<0)
            Debug.LogWarning($"levelID {level}  greater than max count {_objects.Count} . Will error ");
        return _objects[level];
    }

    public string getItemName(int level)
    {
        eItemID tmp = (eItemID)level;
        return tmp.ToString(); //figure something else out later
    }

    public Array[] GetEnumList()
    {
        return (Array[])System.Enum.GetValues(typeof(eItemID));
    }
}

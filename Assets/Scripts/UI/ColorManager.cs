using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ColorManager", menuName = "ColorManager")]
public class ColorManager : ScriptableObject
{
    [SerializeField] Color _cBlack = default;
    [SerializeField] Color _cGood = default;
    [SerializeField] Color _cLow = default;
    [SerializeField] Color _cBad = default;

    [SerializeField] Color _tutorialTabOn = default;
    [SerializeField] Color _tutorialTabOff = default;

    public Color Black => _cBlack;
    public Color Good => _cGood;
    public Color Low => _cLow;
    public Color Bad => _cBad;

    public Color TutorialTabOn => _tutorialTabOn;
    public Color TutorialTabOff => _tutorialTabOff;

}

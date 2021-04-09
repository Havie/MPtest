#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class UIPreviewMovementIcon : MonoBehaviour
{
    [SerializeField] Image _myIcon = default;
    private Sprite _defaultIcon;


    public void Awake()
    {
        _defaultIcon = this.GetComponent<SpriteRenderer>().sprite;
    }

    public void ShowPreviewMovingIcon(bool cond, Vector3 pos, Sprite img)
    {
        if (_myIcon)
        {
            this.gameObject.SetActive(cond);
            if (cond)
            {
                this.gameObject.transform.position = pos;
                _myIcon.sprite = img;
            }
        }
    }
}


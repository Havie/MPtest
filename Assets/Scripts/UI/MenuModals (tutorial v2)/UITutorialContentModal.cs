#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class UITutorialContentModal : MonoBehaviour
{
    [Header("Default End Step")]
    [SerializeField] TutorialItem _OnFinishedItem = default;
    [Header("Self Components")]
    [SerializeField] TextMeshProUGUI _descriptionText = default;
    [SerializeField] VideoPlayer _video = default;
    [SerializeField] Button _leftButton = default;
    [SerializeField] Button _rightButton = default;

    /************************************************************************************************************************/

    public void DisplayInfo(TutorialItem item)
    {
        if (item)
        {
            _descriptionText.text = item.bodyTxt;
            _video.clip = item.VideoGif;
        }
        else
        {
            if (_OnFinishedItem) ///Prevent an infinite loop if unassigned
            {
                DisplayInfo(_OnFinishedItem); 
            }
        }
    }

    public void SetButtonNavigationCallBack(bool isLeftArrow, System.Action callback)
    {
        Button button = PickCorrectArrowButton(isLeftArrow);
        button.onClick.AddListener(delegate { callback(); });
    }

    public void DisableContentArrow(bool isLeftArrow)
    {
        Button button = PickCorrectArrowButton(isLeftArrow);
        button.gameObject.SetActive(false);
    }
    private Button PickCorrectArrowButton(bool isLeftArrow)
    {
        return isLeftArrow ? _leftButton : _rightButton;
    }
}


#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class UITutorialContentModal : MonoBehaviour
{
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
            Debug.Log($"<color=yellow> Missing TutorialItem Info </color>");
        }
    }

    public void SetButtonNavigationCallBack(bool isLeftArrow, System.Action callback)
    {
        Button button = isLeftArrow ? _leftButton : _rightButton;
        button.onClick.AddListener(delegate { callback(); });
    }
}


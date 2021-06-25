
using UnityEngine;

[RequireComponent(typeof(ClickToShow))]
public class PartBin : HighlightableObject
{
    [SerializeField] Animator _animator = default;
    [SerializeField] string _animTxt = "CompAdded";
    [SerializeField] BatchEvent _uiEvent = default;

    private ClickToShow _toggle;
    private bool _isSelected;


    protected override void Awake()
    {
        base.Awake();
        _toggle = this.GetComponent<ClickToShow>();
        if (_uiEvent)
            _uiEvent.OnEventRaised += PlayUIAnim;
    }

    private void Follow(Vector3 toLoc)
    {
        Vector3 onlyHoriz = new Vector3(toLoc.x, transform.position.y, toLoc.z);
        transform.position = Vector3.Lerp(transform.position, onlyHoriz, 0.5f);
    }


    public override void OnInteract()
    {
        ///Nothing really happens we click this object?
        _isSelected = !_isSelected;
        SetHighlighted(_isSelected);
        _toggle.ClickToShowObject();
        if(GameManager.Instance.IsTutorial)
        {
            TutorialEvents.CallOnInventoryOpened();
        }
    }

    /// <summary>
    /// Used to trigger an animation when items get added/removed from the bins via network
    /// Instead of adding a new void event for IN ( added 1 component at a time) and listening for 2 event types,
    /// decided to make the in-event a batchEvent full of dummy data. Will probably move to the network sending individual items
    /// as batches at some point soon 
    /// </summary>
    /// <param name="dummyData"></param>
    private void PlayUIAnim(BatchWrapper dummyData)
    {
        if(_animator)
        {
            _animator.SetTrigger(_animTxt);
        }
    }


}

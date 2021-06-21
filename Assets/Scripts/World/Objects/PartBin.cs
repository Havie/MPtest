
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

    private void PlayUIAnim(BatchWrapper dummyData)
    {
        Debug.Log($"Play anim");
        if(_animator)
        {
            _animator.SetTrigger(_animTxt);
        }
    }







}

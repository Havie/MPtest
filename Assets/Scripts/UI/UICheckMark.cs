#pragma warning disable CS0649 // Ignore : "Field is never assigned to, and will always have its default value"
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UICheckMark : MonoBehaviour
{
    [SerializeField] Animator _animator = default;

    ///NPE if done in awake for whatever reason
    private bool FindAnimator()
    {
        if (_animator == null)
            _animator = this.GetComponent<Animator>();

        return _animator != null;
    }

    public void ResetState()
    {

        if (FindAnimator() && _animator.GetBool("HasPlayed") == true)
        {
            _animator.SetBool("HasPlayed", false);
            _animator.SetTrigger("Reset");
        }
    }
}


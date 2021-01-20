using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class UIInstructions : MonoBehaviour
{

    [SerializeField] Animator _controller = default;

    private bool _isOnTable = true;
    private bool _isAnimating;


    public void InstructionsClicked()
    {
        Debug.Log($"Instructions Clicked _isAnimating={_isAnimating}");
        if (_isAnimating)
            return;

        if (_isOnTable)
            PlayEnlargeAnimation();
        else
            PlayShrinkAnimation();

        _isOnTable = !_isOnTable;
    }

    private void PlayEnlargeAnimation()
    {
        _isAnimating = true;
        _controller.SetTrigger("Open");
        StartCoroutine(AnimationFinished());
    }

    private void PlayShrinkAnimation()
    {
        _isAnimating = true;
        _controller.SetTrigger("Close");
        StartCoroutine(AnimationFinished());
    }

    IEnumerator AnimationFinished()
    {
        var time =_controller.GetCurrentAnimatorStateInfo(0).length;
        //Debug.Log("time is :" + time);
        yield return new WaitForSeconds(time);
        _isAnimating = false;
    }
}

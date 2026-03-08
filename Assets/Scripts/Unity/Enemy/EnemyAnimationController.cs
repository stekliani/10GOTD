using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator _animator;
    EnemyAnimations currentAnimation;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();

    }
    public void ChangeAnimation(EnemyAnimations animation)
    {
        if (currentAnimation == EnemyAnimations.dying || currentAnimation == animation) return;

        switch (animation)
        {
            case EnemyAnimations.dying:
                currentAnimation = animation;
                _animator.SetBool("isDying", true);
                break;
            case EnemyAnimations.atacking:
                currentAnimation = animation;
                _animator.SetBool("isAtacking", true);
                break;
            case EnemyAnimations.walking:
                currentAnimation = animation;
                _animator.SetBool("isWalking", true);
                break;
        }
    }


    public float GetCurrentAnimationDuration()
    {
        AnimatorStateInfo state =
            _animator.GetCurrentAnimatorStateInfo(0);

        return state.length;
    }

    public EnemyAnimations GetCurrentAnimation()
    {
        return currentAnimation;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public class EnemyAnimationController : MonoBehaviour
{
    private Animator _animator;
    private EnemyAnimations currentAnimation;

    // Cache default pose so pooled skeletal rigs reset cleanly.
    private Transform _animatorRoot;
    private Transform[] _cachedTransforms;
    private Vector3[] _defaultLocalPositions;
    private Quaternion[] _defaultLocalRotations;
    private Vector3[] _defaultLocalScales;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        if (_animator != null)
        {
            _animatorRoot = _animator.transform;
            _cachedTransforms = _animatorRoot.GetComponentsInChildren<Transform>(true);
            _defaultLocalPositions = new Vector3[_cachedTransforms.Length];
            _defaultLocalRotations = new Quaternion[_cachedTransforms.Length];
            _defaultLocalScales = new Vector3[_cachedTransforms.Length];

            for (int i = 0; i < _cachedTransforms.Length; i++)
            {
                Transform t = _cachedTransforms[i];
                _defaultLocalPositions[i] = t.localPosition;
                _defaultLocalRotations[i] = t.localRotation;
                _defaultLocalScales[i] = t.localScale;
            }
        }

    }

    public void ResetToDefaults()
    {
        currentAnimation = default;

        if (_cachedTransforms != null)
        {
            for (int i = 0; i < _cachedTransforms.Length; i++)
            {
                Transform t = _cachedTransforms[i];
                if (t == null) continue;
                t.localPosition = _defaultLocalPositions[i];
                t.localRotation = _defaultLocalRotations[i];
                t.localScale = _defaultLocalScales[i];
            }
        }

        if (_animator == null) return;

        _animator.Rebind();
        _animator.Update(0f);

        _animator.SetBool("isDying", false);
        _animator.SetBool("isWalking", false);
        _animator.SetBool("isAtacking", false);
    }

    public void ChangeAnimation(EnemyAnimations animation)
    {
        if (currentAnimation == animation) return;

        switch (animation)
        {
            case EnemyAnimations.dying:
                currentAnimation = animation;
                _animator.SetBool("isDying", true);
                _animator.SetBool("isWalking", false);
                _animator.SetBool("isAtacking", false);
                break;
            case EnemyAnimations.atacking:
                currentAnimation = animation;
                _animator.SetBool("isDying", false);
                _animator.SetBool("isWalking", false);
                _animator.SetBool("isAtacking", true);
                break;
            case EnemyAnimations.walking:
                currentAnimation = animation;
                _animator.SetBool("isDying", false);
                _animator.SetBool("isWalking", true);
                _animator.SetBool("isAtacking", false);
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

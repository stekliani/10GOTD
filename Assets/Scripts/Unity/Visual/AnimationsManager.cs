using System;
using UnityEngine;

public class AnimationsManager : MonoBehaviour, IAnimationObserver
{
    [SerializeField] private GameObject                animatedPrefab;
    [SerializeField] private RuntimeAnimatorController groundExplosionController;
    [SerializeField] private RuntimeAnimatorController midAirExplosionController;

    [Header("Healing Animation")]
    [SerializeField] private GameObject healingAnimationPrefab;
    [SerializeField] private Transform  healingAnimationAnchor;

    private float _lastHealTime;
    [SerializeField] private float healStopDelay = 0.2f;

    private GameObject _currentHealingFX;
    private Animator   _healingAnimator;

    public void OnNotify(AnimationActions action, Vector3 position)
    {
        RuntimeAnimatorController controller = action switch
        {
            AnimationActions.PlayGroundExplosion => groundExplosionController,
            AnimationActions.PlayMidAirExplosion => midAirExplosionController,
            _                                    => null
        };

        if (controller != null)
            SpawnAndPlay(position, Quaternion.identity, controller);
    }

    private void OnEnable()
    {
        AnimationSubject.OnAnimationGlobal += OnNotify;
        PlayerStats.OnHeal                 += PlayHealingAnimation;
    }

    private void OnDisable()
    {
        AnimationSubject.OnAnimationGlobal -= OnNotify;
        PlayerStats.OnHeal                 -= PlayHealingAnimation;
    }

    private void Update()
    {
        if (_currentHealingFX != null && Time.time - _lastHealTime > healStopDelay)
        {
            _currentHealingFX.SetActive(false);
        }
    }

    private void SpawnAndPlay(Vector3 position, Quaternion rotation, RuntimeAnimatorController controller)
    {
        GameObject instance = Instantiate(animatedPrefab, position, rotation);
        Animator   anim     = instance.GetComponentInChildren<Animator>();

        if (anim == null) { Debug.LogError("Animator not found on prefab!"); return; }

        anim.runtimeAnimatorController = controller;
        anim.Play(0, 0, 0f);
        Destroy(instance, GetClipLength(anim));
    }

    private float GetClipLength(Animator anim)
    {
        var clips = anim.runtimeAnimatorController?.animationClips;
        return (clips != null && clips.Length > 0) ? clips[0].length : 1f;
    }

    private void PlayHealingAnimation(object sender, EventArgs e)
    {
        _lastHealTime = Time.time;

        if (_currentHealingFX == null)
        {
            _currentHealingFX = Instantiate(
                healingAnimationPrefab,
                healingAnimationAnchor.position,
                Quaternion.identity,
                healingAnimationAnchor);

            _healingAnimator = _currentHealingFX.GetComponent<Animator>();

            // play once when first created
            _healingAnimator.Play(0, 0, 0f);
            return;
        }


        // if it was disabled and healing resumed
        if (!_currentHealingFX.activeSelf)
        {
            _currentHealingFX.SetActive(true);
            _healingAnimator.Play(0, 0, 0f); // restart only when re-enabled
        }
    }
}

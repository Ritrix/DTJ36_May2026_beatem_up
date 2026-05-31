using System.Collections;
using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Animation States")]
    [SerializeField] private string idleAnimationState = "Enemy_Idle";
    [SerializeField] private string damagedAnimationState = "Enemy_Damaged";

    [Header("Timing")]
    [SerializeField] private float damagedFrameTime = 0.12f;

    private Coroutine damagedRoutine;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    public void PlayIdle()
    {
        PlayAnimationState(idleAnimationState);
    }

    public void PlayDamaged()
    {
        if (damagedRoutine != null)
            StopCoroutine(damagedRoutine);

        damagedRoutine = StartCoroutine(DamagedRoutine());
    }

    private IEnumerator DamagedRoutine()
    {
        PlayAnimationState(damagedAnimationState);

        yield return new WaitForSeconds(damagedFrameTime);

        PlayAnimationState(idleAnimationState);
    }

    private void PlayAnimationState(string stateName)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(stateName)) return;

        animator.Play(stateName, 0, 0f);
    }
}
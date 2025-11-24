using System.Collections;
using UnityEngine;

public class DarkVisionController : MonoBehaviour
{
    [Header("Overlay Settings")]
    [Tooltip("화면을 어둡게 덮는 패널 (Canvas 아래 Image 같은 거)")]
    public CanvasGroup overlay;      // 없으면 전체 GameObject 기준으로 처리
    public bool autoDisableOnDuration = true;

    Coroutine routine;

    void Awake()
    {
        // overlay를 지정 안 해줬으면, 자기 자신에서 CanvasGroup 찾아서 사용
        if (overlay == null)
            overlay = GetComponent<CanvasGroup>();
    }

    /// <summary>
    /// 암흑 시야 켜기 (duration 동안 유지)
    /// </summary>
    public void Enable(float duration)
    {
        gameObject.SetActive(true);

        if (overlay != null)
        {
            overlay.alpha = 0f;
            overlay.blocksRaycasts = true;
            overlay.interactable = false;
        }

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FadeInAndWait(duration));

        /*
        if (autoDisableOnDuration)
        {
            if (routine != null)
                StopCoroutine(routine);
            routine = StartCoroutine(AutoDisableRoutine(duration));
        }
        */
    }

    IEnumerator FadeInAndWait(float duration)
    {
        // ----- 1) 페이드 인 (0 → 1) -----
        float fadeDuration = 2.0f; // 페이드인 시간 원하는 만큼 조절 가능
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            if (overlay != null)
                overlay.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }

        if (overlay != null) overlay.alpha = 1f;

        // ----- 2) duration 만큼 유지 -----
        float holdTime = duration;
        float timer = 0f;
        while (timer < holdTime)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        // ----- 3) 자동 종료 -----
        Disable();
    }

    /*
    IEnumerator AutoDisableRoutine(float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        Disable();
    }
    */

    /// <summary>
    /// 부드럽게 끌 필요 없으면 그냥 호출
    /// </summary>
    public void Disable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        if (overlay != null)
        {
            overlay.alpha = 0f;
            overlay.blocksRaycasts = false;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 보스가 죽었을 때 등, 즉시 끄고 싶을 때 사용
    /// </summary>
    public void DisableImmediately()
    {
        Disable();
    }
}

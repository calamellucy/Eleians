using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class CutsceneDialog
{
    [TextArea(3, 5)]
    public string text;
    public float waitTime = 2f;
}

[System.Serializable]
public class CutsceneStep
{
    public Sprite slideImage;
    public List<CutsceneDialog> dialogs;
}

public class IntroManager : MonoBehaviour
{
    [Header("UI Components")]
    public Image displayImage;
    public Text subtitleText;
    public AspectRatioFitter imageFitter;
    public Image fadePanel;

    [Header("Settings")]
    public float typingSpeed = 0.05f;
    public float fadeDuration = 1.0f;
    public string nextSceneName = "Battle_Main";

    [Header("Data")]
    public List<CutsceneStep> cutsceneSteps;

    private bool isSkippingTyping = false;

    void Start()
    {
        if (imageFitter == null && displayImage != null)
            imageFitter = displayImage.GetComponent<AspectRatioFitter>();

        if (fadePanel != null) fadePanel.gameObject.SetActive(true);

        StartCoroutine(PlayCutscene());
    }

    public void SkipAllCutscene()
    {
        StopAllCoroutines();
        SceneManager.LoadScene(nextSceneName);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isSkippingTyping = true;
        }
    }

    IEnumerator PlayCutscene()
    {
        // [수정 1] 시작하기 전에 '첫 번째 이미지'를 미리 세팅해둡니다.
        // 화면이 검은 상태(FadePanel이 덮인 상태)라 유저는 교체 과정을 못 봅니다.
        if (cutsceneSteps.Count > 0 && cutsceneSteps[0].slideImage != null)
        {
            SetImage(cutsceneSteps[0].slideImage);
        }

        // 1. 이제 이미지가 세팅된 상태에서 화면이 밝아집니다.
        yield return StartCoroutine(Fade(1, 0));

        foreach (CutsceneStep step in cutsceneSteps)
        {
            // 이미지가 바뀌어야 할 때 (첫 번째 이미지는 위에서 이미 세팅했으므로 건너뜀)
            if (step.slideImage != null && displayImage.sprite != step.slideImage)
            {
                // [수정 2] 이미지가 넘어가기 전에 자막을 싹 지웁니다.
                subtitleText.text = "";

                // 페이드 아웃 (어두워짐)
                yield return StartCoroutine(Fade(0, 1));

                // 이미지 교체
                SetImage(step.slideImage);

                // 페이드 인 (밝아짐)
                yield return StartCoroutine(Fade(1, 0));
            }

            foreach (CutsceneDialog dialog in step.dialogs)
            {
                isSkippingTyping = false;

                // 타자기 효과 실행
                yield return StartCoroutine(TypeEffect(dialog.text));

                // [수정 3] 여기서 1프레임을 쉬어줍니다! (매우 중요)
                // 이유: 타자기를 스킵하려고 누른 스페이스바가 아래 while문까지 영향을 주지 않게 하기 위해.
                isSkippingTyping = false; // 스킵 변수 확실히 초기화
                yield return null;

                float timer = 0;
                while (timer < dialog.waitTime)
                {
                    timer += Time.deltaTime;
                    // 이제 위에서 1프레임 쉬었으므로, 아까 누른 키는 여기서 감지되지 않습니다.
                    // 유저가 '새로' 눌러야만 감지됩니다.
                    if (Input.GetKeyDown(KeyCode.Space)) break;
                    yield return null;
                }
            }
        }

        // 끝날 때 페이드 아웃
        yield return StartCoroutine(Fade(0, 1));

        EndCutscene();
    }

    // [코드 정리] 이미지와 비율을 함께 바꾸는 함수를 따로 뺐습니다.
    void SetImage(Sprite sprite)
    {
        displayImage.sprite = sprite;
        float ratio = sprite.rect.width / sprite.rect.height;
        imageFitter.aspectRatio = ratio;
    }

    IEnumerator TypeEffect(string fullText)
    {
        subtitleText.text = "";

        foreach (char letter in fullText.ToCharArray())
        {
            if (isSkippingTyping)
            {
                subtitleText.text = fullText;
                yield break;
            }

            subtitleText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    IEnumerator Fade(float startAlpha, float endAlpha)
    {
        if (fadePanel == null) yield break;

        float timer = 0;
        Color color = fadePanel.color;
        color.a = startAlpha; // 확실하게 시작값 고정
        fadePanel.color = color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / fadeDuration);
            fadePanel.color = color;
            yield return null;
        }

        color.a = endAlpha;
        fadePanel.color = color;
    }

    void EndCutscene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
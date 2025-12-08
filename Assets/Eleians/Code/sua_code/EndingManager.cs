using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// (CutsceneDialog, CutsceneStep 클래스는 이미 정의되어 있다고 가정합니다. 
// 만약 IntroManager와 같은 파일에 있다면 이 부분은 지워도 됩니다.)
/*
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
*/

public class EndingManager : MonoBehaviour
{
    [Header("Phase 1: Cutscene UI")]
    public Image displayImage;
    public Text subtitleText;
    public AspectRatioFitter imageFitter;
    public Image fadePanel;

    [Header("Phase 2: Credits UI")]
    public GameObject creditsObject;   // 크레딧 배경 오브젝트 (검은 배경 추천)
    public Text creditText;            // ★ 가운데서 나타날 텍스트 컴포넌트

    [TextArea(1, 3)]
    public List<string> creditLines;   // ★ 크레딧 내용들 (Inspector에서 입력)

    public float creditDisplayTime = 2.0f; // 한 문구가 떠있는 시간
    public float textFadeDuration = 1.0f;  // 텍스트가 나타나/사라지는 시간

    [Header("Phase 3: Result UI")]
    public GameObject resultPanel; // 결과 화면 패널
    public Button toMainButton;    // 메인으로 가는 버튼

    [Header("Common Settings")]
    public float typingSpeed = 0.05f;
    public float fadeDuration = 1.0f;
    public string mainMenuSceneName = "MainScreen_jw";

    [Header("Data")]
    public List<CutsceneStep> cutsceneSteps;

    private bool isSkippingTyping = false;
    private bool isSkippingCredits = false;

    void Start()
    {
        // 1. 초기화
        if (imageFitter == null && displayImage != null)
            imageFitter = displayImage.GetComponent<AspectRatioFitter>();

        // UI 초기 상태 설정
        if (fadePanel != null) fadePanel.gameObject.SetActive(true);
        if (creditsObject != null) creditsObject.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);

        // 버튼 리스너 연결
        if (toMainButton != null)
            toMainButton.onClick.AddListener(GoToMainMenu);

        // 전체 엔딩 시퀀스 시작
        StartCoroutine(PlayEndingSequence());
    }

    void Update()
    {
        // 텍스트 타이핑 스킵
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isSkippingTyping = true;
            // 크레딧 스킵 (가속)
            isSkippingCredits = true;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            isSkippingCredits = false;
        }
    }

    // ★ 전체 흐름을 관리하는 메인 코루틴
    IEnumerator PlayEndingSequence()
    {
        // 1단계: 컷씬 재생 (IntroManager 로직 재사용)
        yield return StartCoroutine(PlayCutscenePhase());

        // 2단계: 엔딩 크레딧 재생
        yield return StartCoroutine(PlayCreditsPhase());

        // 3단계: 결과 화면 표시
        ShowResultPhase();
    }

    // =================================================================
    // Phase 1: 컷씬 로직 (IntroManager와 거의 동일)
    // =================================================================
    IEnumerator PlayCutscenePhase()
    {
        // 첫 이미지 세팅
        if (cutsceneSteps.Count > 0 && cutsceneSteps[0].slideImage != null)
        {
            SetImage(cutsceneSteps[0].slideImage);
        }

        // 화면 밝아짐
        yield return StartCoroutine(Fade(1, 0));

        foreach (CutsceneStep step in cutsceneSteps)
        {
            // 이미지 교체 로직
            if (step.slideImage != null && displayImage.sprite != step.slideImage)
            {
                subtitleText.text = "";
                yield return StartCoroutine(Fade(0, 1)); // 페이드 아웃
                SetImage(step.slideImage);
                yield return StartCoroutine(Fade(1, 0)); // 페이드 인
            }

            // 대사 출력 로직
            foreach (CutsceneDialog dialog in step.dialogs)
            {
                isSkippingTyping = false;
                yield return StartCoroutine(TypeEffect(dialog.text));

                isSkippingTyping = false;
                yield return null; // 1프레임 대기 (키 입력 중복 방지)

                float timer = 0;
                while (timer < dialog.waitTime)
                {
                    timer += Time.deltaTime;
                    if (Input.GetKeyDown(KeyCode.Space)) break;
                    yield return null;
                }
            }
        }

        // 컷씬 끝날 때 페이드 아웃 (화면 검게)
        yield return StartCoroutine(Fade(0, 1));

        // 자막 비우기
        subtitleText.text = "";
    }

    // =================================================================
    // Phase 2: 엔딩 크레딧 로직
    // =================================================================
    IEnumerator PlayCreditsPhase()
    {
        if (creditsObject == null || creditText == null) yield break;

        // 1. 크레딧 오브젝트 켜기
        creditsObject.SetActive(true);
        creditText.text = ""; // 처음엔 빈 칸

        // 텍스트 투명도 초기화 (안 보이게)
        Color c = creditText.color;
        c.a = 0f;
        creditText.color = c;

        // 2. 화면 페이드 인 (검은 화면 -> 크레딧 배경)
        yield return StartCoroutine(Fade(1, 0));

        // 3. 한 줄씩 나타났다 사라지기 반복
        foreach (string line in creditLines)
        {
            // 텍스트 교체
            creditText.text = line;

            // (1) 텍스트 페이드 인 (나타나기)
            yield return StartCoroutine(FadeText(0, 1));

            // (2) 대기 (스페이스바 누르면 빨리 넘어감)
            float timer = 0f;
            float wait = isSkippingCredits ? 0.3f : creditDisplayTime; // 스킵 시 짧게 대기

            while (timer < wait)
            {
                timer += Time.deltaTime;
                // 스킵 중이면 대기 시간 실시간 단축
                if (isSkippingCredits && timer > 0.3f) break;
                yield return null;
            }

            // (3) 텍스트 페이드 아웃 (사라지기)
            yield return StartCoroutine(FadeText(1, 0));
        }

        // 4. 모든 텍스트가 끝났으면 화면 다시 어둡게 (Phase 종료)
        yield return StartCoroutine(Fade(0, 1));

        creditsObject.SetActive(false);
    }

    // =================================================================
    // Phase 3: 결과 화면 로직
    // =================================================================
    void ShowResultPhase()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            // 결과 화면 나올 때 부드럽게 밝아지기
            StartCoroutine(Fade(1, 0));
        }
    }

    // =================================================================
    // Helper Functions (공통 기능)
    // =================================================================

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // 혹시 멈춰있을 경우 대비
        SceneManager.LoadScene(mainMenuSceneName);
    }

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
        color.a = startAlpha;
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

    // ★★★ [신규] 텍스트 전용 페이드 함수 ★★★
    IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        if (creditText == null) yield break;

        // 스킵 중이면 페이드 속도 5배
        float duration = isSkippingCredits ? textFadeDuration * 0.2f : textFadeDuration;
        float timer = 0;

        Color color = creditText.color;
        color.a = startAlpha;
        creditText.color = color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, timer / duration);
            creditText.color = color;
            yield return null;
        }
        color.a = endAlpha;
        creditText.color = color;
    }
}
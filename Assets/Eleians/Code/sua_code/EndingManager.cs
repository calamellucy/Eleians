using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [Header("Phase 1: Cutscene UI")]
    public Image displayImage;
    public Text subtitleText;
    public AspectRatioFitter imageFitter;
    public Image fadePanel;
    public Button skipButton;

    [Header("Phase 2: Credits UI")]
    public GameObject creditsObject;   // 크레딧 배경 오브젝트 (검은 배경 추천)
    public Text creditText;            // ★ 가운데서 나타날 텍스트 컴포넌트

    [TextArea(1, 3)]
    public List<string> creditLines;   // ★ 크레딧 내용들 (Inspector에서 입력)

    public float creditDisplayTime = 2.0f; // 한 문구가 떠있는 시간
    public float textFadeDuration = 1.0f;  // 텍스트가 나타나/사라지는 시간

    public GameObject returnPromptObject;

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
    private bool isSkippingCutscene = false;

    void Start()
    {
        // 1. 초기화
        if (imageFitter == null && displayImage != null)
            imageFitter = displayImage.GetComponent<AspectRatioFitter>();

        // UI 초기 상태 설정
        if (fadePanel != null) fadePanel.gameObject.SetActive(true);
        if (creditsObject != null) creditsObject.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);

        if (returnPromptObject != null) returnPromptObject.SetActive(false);

        // 버튼 리스너 연결
        if (toMainButton != null)
            toMainButton.onClick.AddListener(GoToMainMenu);

        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipCutscene);
            skipButton.gameObject.SetActive(true); // 컷씬 중엔 보여줌
        }

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

    // ★ [신규] 스킵 버튼이 눌리면 호출됨
    public void SkipCutscene()
    {
        isSkippingCutscene = true;
    }

    IEnumerator PlayEndingSequence()
    {
        // 1. 컷씬 재생
        yield return StartCoroutine(PlayCutscenePhase());

        // 컷씬 끝났으니 스킵 버튼 숨기기
        if (skipButton != null) skipButton.gameObject.SetActive(false);

        // 2. 크레딧 재생
        yield return StartCoroutine(PlayCreditsPhase());
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
            if (isSkippingCutscene) break;

            // 이미지 교체 로직
            if (step.slideImage != null && displayImage.sprite != step.slideImage)
            {
                subtitleText.text = "";
                yield return StartCoroutine(Fade(0, 1)); // 페이드 아웃

                if (isSkippingCutscene) break;

                SetImage(step.slideImage);
                yield return StartCoroutine(Fade(1, 0)); // 페이드 인
            }

            // 대사 출력 로직
            foreach (CutsceneDialog dialog in step.dialogs)
            {
                if (isSkippingTyping) isSkippingTyping = false; // 초기화
                if (isSkippingCutscene) break;
                yield return StartCoroutine(TypeEffect(dialog.text));

                isSkippingTyping = false;
                yield return null; // 1프레임 대기 (키 입력 중복 방지)

                float timer = 0;
                while (timer < dialog.waitTime)
                {
                    timer += Time.deltaTime;
                    if (Input.GetKeyDown(KeyCode.Space)) break;
                    if (isSkippingCutscene) break;
                    yield return null;
                }

                if (isSkippingCutscene) break; // 대사 루프 탈출
            }

            if (isSkippingCutscene) break; // 대사 루프 탈출
        }

        // 스킵으로 나왔든, 정상 종료든 화면을 어둡게 하고 자막을 지움
        if (isSkippingCutscene)
        {
            // 즉시 어둡게 (뚝 끊기는 느낌 방지하려면 짧은 페이드)
            yield return StartCoroutine(Fade(fadePanel.color.a, 1f)); // 현재 상태 -> 1
        }
        else
        {
            yield return StartCoroutine(Fade(0, 1));
        }

        // 자막 비우기
        subtitleText.text = "";
    }

    // =================================================================
    // Phase 2: 엔딩 크레딧 로직
    // =================================================================
    IEnumerator PlayCreditsPhase()
    {
        if (creditsObject == null || creditText == null) yield break;

        creditsObject.SetActive(true);
        creditText.text = "";

        Color c = creditText.color;
        c.a = 0f;
        creditText.color = c;

        yield return StartCoroutine(Fade(1, 0)); // 배경 밝아짐

        // ★ for문으로 변경 (마지막 인덱스 체크를 위해)
        for (int i = 0; i < creditLines.Count; i++)
        {
            string line = creditLines[i];
            creditText.text = line;

            // (1) 텍스트 나타나기
            yield return StartCoroutine(FadeText(0, 1));

            // ★★★ [핵심] 마지막 줄인가? ★★★
            if (i == creditLines.Count - 1)
            {
                // 마지막 줄이면 사라지지 않고 대기
                Debug.Log("엔딩 크레딧 끝. 입력 대기 중...");

                // 안내 문구 페이드 인 호출
                if (returnPromptObject != null)
                {
                    returnPromptObject.SetActive(true);
                    // 1초 동안 부드럽게 나타나게 함
                    StartCoroutine(FadeInObject(returnPromptObject, 1.0f));
                }

                // 클릭(터치) 대기
                // (스페이스바 스킵 방지를 위해 잠시 0.5초 텀을 둠)
                yield return new WaitForSeconds(0.5f);

                // 무한 대기: 클릭할 때까지
                while (!Input.GetMouseButtonDown(0) && !Input.anyKeyDown)
                {
                    yield return null;
                }

                // 클릭하면 메인으로 이동
                GoToMainMenu();
                yield break; // 코루틴 종료
            }

            // (2) 마지막 줄이 아니면 대기 후 사라짐
            float timer = 0f;
            float wait = isSkippingCredits ? 0.3f : creditDisplayTime;

            while (timer < wait)
            {
                timer += Time.deltaTime;
                if (isSkippingCredits && timer > 0.3f) break;
                yield return null;
            }

            // (3) 텍스트 사라지기
            yield return StartCoroutine(FadeText(1, 0));
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

    // ★★★ [신규] 안내 문구 오브젝트(Text 혹은 CanvasGroup) 페이드 인 함수 ★★★
    IEnumerator FadeInObject(GameObject obj, float duration)
    {
        // 1. CanvasGroup이 있다면 그걸로 조절 (추천 방식)
        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0f; // 시작은 투명
            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                cg.alpha = Mathf.Lerp(0f, 1f, timer / duration);
                yield return null;
            }
            cg.alpha = 1f;
            yield break;
        }

        // 2. CanvasGroup이 없고 Text만 있다면 Text 색상 조절
        Text txt = obj.GetComponent<Text>();
        if (txt != null)
        {
            Color c = txt.color;
            c.a = 0f;
            txt.color = c;

            float timer = 0f;
            while (timer < duration)
            {
                timer += Time.deltaTime;
                c.a = Mathf.Lerp(0f, 1f, timer / duration);
                txt.color = c;
                yield return null;
            }
            c.a = 1f;
            txt.color = c;
        }
    }
}
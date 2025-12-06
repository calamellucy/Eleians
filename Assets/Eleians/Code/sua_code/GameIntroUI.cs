using UnityEngine;
using UnityEngine.UI;

public class GameIntroUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject introPanel;     // 전체 패널 (배경 포함)
    public GameObject[] pages;        // 설명 페이지들 (Page1, Page2, Page3)
    public Text buttonText;// 버튼 텍스트 (Next -> Start로 변경용)

    private int currentPageIndex = 0;

    void Start()
    {
        // 게임 시작 시 무조건 실행
        ShowIntro();
    }

    public void ShowIntro()
    {
        // 1. 패널 켜기
        introPanel.SetActive(true);

        // 2. 게임 시간 정지 (중요!)
        Time.timeScale = 0f;

        // 3. 페이지 초기화 (0번만 켜고 나머지 끔)
        currentPageIndex = 0;
        UpdatePageDisplay();
    }

    // 버튼(OnClick)에 연결할 함수
    public void OnNextButtonClick()
    {
        // 현재 페이지 끄기
        pages[currentPageIndex].SetActive(false);

        // 다음 페이지로 인덱스 증가
        currentPageIndex++;

        // 마지막 페이지까지 다 봤다면? -> 게임 시작!
        if (currentPageIndex >= pages.Length)
        {
            GameStart();
            return;
        }

        // 다음 페이지 켜기
        UpdatePageDisplay();
    }

    void UpdatePageDisplay()
    {
        // 해당 순서 페이지만 켜기
        pages[currentPageIndex].SetActive(true);

        // 마지막 페이지면 버튼 텍스트를 "게임 시작"으로 변경
        if (currentPageIndex == pages.Length - 1)
        {
            if (buttonText != null) buttonText.text = "Start";
        }
        else
        {
            if (buttonText != null) buttonText.text = "Next";
        }
    }

    void GameStart()
    {
        // UI 끄기
        introPanel.SetActive(false);

        // ★ 시간 다시 흐르게 하기 (게임 시작)
        Time.timeScale = 1f;

        // (선택사항) 효과음 재생
        // AudioManager.instance.PlaySfx(AudioManager.Sfx.Click);
    }
}
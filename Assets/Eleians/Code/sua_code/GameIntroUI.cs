using UnityEngine;
using UnityEngine.UI;

public class GameIntroUI : MonoBehaviour
{
    [Header("UI Components")]
    public GameObject introPanel;     // 전체 패널 (배경 포함)
    public GameObject[] pages;        // 설명 페이지들 (Page1, Page2, Page3)
    public Text buttonText;           // 버튼 텍스트 (Next -> Start)

    private int currentPageIndex = 0;

    void Start()
    {
        // 게임 켜자마자 최초 1회 실행
        ShowIntro();
    }

    // ★ 외부(EscButton)에서 이 함수를 호출해서 다시 열 수 있음
    public void ShowIntro()
    {
        // 1. 패널 켜기
        introPanel.SetActive(true);

        // 2. 게임 시간 정지 (ESC 메뉴에서 넘어왔어도 확실히 정지)
        Time.timeScale = 0f;

        // 3. 페이지 초기화 (0번부터 다시 보여주기)
        currentPageIndex = 0;
        UpdatePageDisplay();
    }

    public void OnNextButtonClick()
    {
        // 현재 페이지 끄기
        pages[currentPageIndex].SetActive(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.click);
        // 다음 페이지로
        currentPageIndex++;

        // 마지막 페이지까지 다 봤다면? -> 닫고 게임 재개
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
        pages[currentPageIndex].SetActive(true);

        // 마지막 페이지면 텍스트 변경
        if (currentPageIndex == pages.Length - 1)
        {
            // 게임 도중 다시 열었을 때는 "Resume"이나 "Close"가 더 어울릴 수도 있지만
            // 일단 요청대로 "Start" 로직 그대로 유지 (혹은 텍스트만 조건부 변경 가능)
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

        // ★ 시간 다시 흐르게 하기 (게임 재개)
        Time.timeScale = 1f;

        AudioManager.instance.PlaySfx(AudioManager.Sfx.click);
        // (중요) 만약 ESC 메뉴가 '숨김' 상태로 남아있다면, 여기서 완전히 닫아주는 처리가 필요할 수도 있음.
        // 보통은 시간만 흐르면 되니까 이대로 OK.
    }
}
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // 클릭 감지용

public class SkillExplanationManager : MonoBehaviour
{
    [Header("연결: 스킬 UI 페이지")]
    public GameObject[] skillPages;

    [Header("연결: ESC 메뉴 관리자")]
    public EscMenu escMenu;

    private int currentPageIndex = 0;

    // ★ 누가 불렀는지 기억하는 변수 (true면 ESC메뉴에서 옴, false면 게임에서 옴)
    private bool isEscContext = false;

    void Update()
    {
        // UI가 켜져 있을 때만 작동
        if (!gameObject.activeSelf) return;

        // 1. ESC 키 입력 감지 -> 닫기 처리
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseProcess();
        }

        // 2. 마우스 좌클릭 감지 -> 닫기 처리
        // (단, 화살표 버튼 등을 누를 때는 닫히면 안 되므로 UI 위가 아닐 때 혹은 배경 버튼 활용 권장)
        // 여기서는 간단하게 '왼쪽 버튼' 클릭 시 닫히도록 구현
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            // 화살표 버튼을 클릭했을 때도 닫히는 걸 방지하려면 
            // EventSystem을 통해 버튼 위인지 체크해야 하는데, 
            // 가장 확실한 방법은 인스펙터에서 'explaination' 오브젝트 자체에 Button 컴포넌트를 추가하고
            // OnClick에 CloseProcess()를 연결하는 거야. (아래 설명 참고)

            // 만약 코드로만 배경 클릭을 처리하고 싶다면, 아래 주석을 풀어줘. 
            // 하지만 화살표 클릭과 겹칠 수 있어서 버튼 컴포넌트 추가 방식을 추천해!
            // CloseProcess(); 
        }

        // 3. 화살표 키 페이지 전환
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame) MovePage(1);
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame) MovePage(-1);
        }
    }

    // --- [중요] 외부에서 여는 함수들 ---

    // 1. 게임 화면(SkillUIManager)에서 호출
    public void OpenFromGame(int pageIndex)
    {
        isEscContext = false; // 게임에서 옴
        OpenUI(pageIndex);
    }

    // 2. ESC 메뉴(EscButton)에서 호출
    public void OpenFromEsc()
    {
        isEscContext = true; // ESC 메뉴에서 옴
        OpenUI(0); // 첫 페이지부터
    }

    // 공통 오픈 로직
    private void OpenUI(int pageIndex)
    {
        gameObject.SetActive(true);
        // 범위 체크
        if (pageIndex >= 0 && pageIndex < skillPages.Length) currentPageIndex = pageIndex;
        else currentPageIndex = 0;

        UpdatePageVisibility();
        Time.timeScale = 0f; // 시간 정지
    }

    // --- [핵심] 닫기 프로세스 ---
    public void CloseProcess()
    {
        gameObject.SetActive(false); // 설명창 끄기

        if (escMenu != null)
        {
            if (isEscContext)
            {
                // ESC 메뉴에서 왔다면 -> ESC 메뉴로 복귀
                escMenu.Show();
            }
            else
            {
                // 게임에서 왔다면 -> 게임 재개 (완전 닫기)
                escMenu.Hide();
            }
        }
        else
        {
            Time.timeScale = 1f; // 비상용
        }
    }

    // --- 내부 로직 (페이지 이동 등) ---
    private void MovePage(int direction)
    {
        currentPageIndex += direction;
        if (currentPageIndex >= skillPages.Length) currentPageIndex = 0;
        else if (currentPageIndex < 0) currentPageIndex = skillPages.Length - 1;

        UpdatePageVisibility();

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.mouse_on_button);
    }

    private void UpdatePageVisibility()
    {
        for (int i = 0; i < skillPages.Length; i++)
        {
            if (skillPages[i] != null) skillPages[i].SetActive(i == currentPageIndex);
        }
    }
}
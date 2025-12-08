using UnityEngine;
using UnityEngine.EventSystems;

public class EscButton : MonoBehaviour, IPointerEnterHandler
{
    [Header("버튼 타입 설정")]
    // 0:게임, 1:도전과제, 2:상태창, 3:설명서, 4:스킬상세, 5:메인
    public int type;

    [Header("연결할 오브젝트들")]
    public EscMenu escMenu;
    public GameObject achievementUI;

    // ★ [변경] GameObject statusUI 대신 매니저 스크립트를 직접 연결
    public StatusUIManager statusManager;

    // ★ [변경] 설명창 UI 게임오브젝트 대신 매니저 스크립트를 직접 연결
    public SkillExplanationManager skillManager;

    public GameIntroUI gameIntroUI;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.mouse_on_button);
    }

    public void OnClick()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySfx(AudioManager.Sfx.click);

        switch (type)
        {
            case 0: // 게임으로
                ResumeGame();
                break;
            case 1: // 도전과제
                OpenSubMenu(achievementUI);
                break;
            case 2: // ★ 상태창 (핵심 수정됨)
                OpenStatusUI();
                break;
            case 3: // 게임 설명
                OpenGameManual();
                break;
            case 4: // ★ 스킬 상세 (핵심 수정됨)
                OpenSkillDetail();
                break;
            case 5: // 메인으로
                GoToMain();
                break;
        }
    }

    // --- 기능 구현 ---

    void ResumeGame()
    {
        escMenu.Hide();
    }

    void OpenSubMenu(GameObject targetUI)
    {
        if (targetUI == null) return;
        targetUI.SetActive(true);
        escMenu.HideForSubMenu();
    }

    // ★ [수정] 상태창 열기 로직
    void OpenStatusUI()
    {
        if (statusManager != null)
        {
            escMenu.HideForSubMenu(); // ESC 메뉴 배경 숨기기

            // 상태창을 연다. (StatusUIManager가 닫힐 때 무조건 ESC 메뉴를 부르도록 설정됨)
            statusManager.OpenStats();
        }
        else
        {
            Debug.LogError("EscButton에 StatusUIManager가 연결되지 않았습니다!");
        }
    }

    void OpenGameManual()
    {
        if (gameIntroUI != null)
        {
            escMenu.HideForSubMenu();
            gameIntroUI.ShowIntro();
        }
    }

    // ★ [수정] 스킬 상세창 열기 로직
    void OpenSkillDetail()
    {
        if (skillManager != null)
        {
            escMenu.HideForSubMenu();
            // 스킬창은 '어디서 왔는지'가 중요하므로 명시적으로 호출
            skillManager.OpenFromEsc();
        }
        else
        {
            Debug.LogError("EscButton에 SkillExplanationManager가 연결되지 않았습니다!");
        }
    }

    void GoToMain() { ResumeGame(); }
}
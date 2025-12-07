using UnityEngine;
using UnityEngine.EventSystems;

public class EscButton : MonoBehaviour, IPointerEnterHandler
{
    [Header("버튼 타입 설정")]
    // 0: 게임으로, 1: 도전과제, 2: 상태창, 3: 게임 설명, 4: 스킬 상세, 5: 메인으로
    public int type;

    [Header("연결할 오브젝트들")]
    public EscMenu escMenu;
    public GameObject achievementUI;
    public GameObject statusUI;
    public GameObject explainationUI;

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
                OpenSubMenu(achievementUI); // ★ 변경됨
                break;
            case 2: // 상태창
                OpenSubMenu(statusUI);      // ★ 변경됨
                break;
            case 3: // 게임 설명
                OpenGameManual();
                break;
            case 4: // 스킬 상세
                OpenSubMenu(explainationUI); // ★ 변경됨
                break;
            case 5: // 메인 화면으로
                GoToMain();
                break;
        }
    }

    // --- 기능 구현 함수들 ---

    void ResumeGame()
    {
        escMenu.Hide(); // 완전히 끄고 게임 재개
    }

    // ★ [핵심] 서브 메뉴 여는 함수
    void OpenSubMenu(GameObject targetUI)
    {
        if (targetUI == null) return;

        targetUI.SetActive(true); // 1. 목표 창(도전과제 등) 켜기
        escMenu.HideForSubMenu(); // 2. ESC 메인 메뉴는 잠깐 숨기기 (시간 정지 유지)
    }

    void OpenGameManual() { ResumeGame(); }
    void GoToMain() { ResumeGame(); }
}
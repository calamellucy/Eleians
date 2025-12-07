using UnityEngine;

public class EscMenu : MonoBehaviour
{
    RectTransform rect;

    [Header("서브 메뉴들")]
    public GameObject achievementUI;
    public GameObject statusUI;
    public GameObject explainationUI;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    void Start()
    {
        rect.localScale = Vector3.zero;
        CloseAllSubMenus();
    }

    void Update()
    {
        // ESC 키 입력 감지
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 1. 서브 메뉴가 켜져 있다면? -> 서브 끄고, 메인 메뉴 다시 보이기
            if (IsAnySubMenuOpen())
            {
                CloseAllSubMenus();
                Show(); // ★ 다시 메인 메뉴 등판!
            }
            // 2. 서브 메뉴 없고, 메인 메뉴가 꺼져 있다면? -> 켜기
            else if (rect.localScale == Vector3.zero)
            {
                Show();
            }
            // 3. 서브 메뉴 없고, 메인 메뉴가 켜져 있다면? -> 끄고 게임 재개
            else
            {
                Hide();
            }
        }
    }

    public void Show()
    {
        rect.localScale = Vector3.one; // 보임
        GameManager.instance.Stop();   // 시간 정지
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero; // 안 보임
        GameManager.instance.Resume();  // 시간 흐름 (게임 재개)
        CloseAllSubMenus();
    }

    // ★ [추가된 함수] 서브 메뉴를 열 때, 메인 메뉴만 잠깐 숨기는 용도 (시간은 계속 정지)
    public void HideForSubMenu()
    {
        rect.localScale = Vector3.zero; // 안 보임
        // GameManager.Resume()을 호출하지 않음! (시간은 멈춰있어야 하니까)
    }

    // --- 헬퍼 함수들 ---
    bool IsAnySubMenuOpen()
    {
        if (achievementUI != null && achievementUI.activeSelf) return true;
        if (statusUI != null && statusUI.activeSelf) return true;
        if (explainationUI != null && explainationUI.activeSelf) return true;
        return false;
    }

    void CloseAllSubMenus()
    {
        if (achievementUI != null) achievementUI.SetActive(false);
        if (statusUI != null) statusUI.SetActive(false);
        if (explainationUI != null) explainationUI.SetActive(false);
    }
}
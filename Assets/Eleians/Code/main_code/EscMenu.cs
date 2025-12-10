using UnityEngine;

public class EscMenu : MonoBehaviour
{
    RectTransform rect;

    [Header("서브 메뉴들")]
    public GameObject achievementUI;
    public GameObject statusUI;
    public GameObject explainationUI;

    [Header("★ 매니저 연결")]
    public StatusUIManager statusManager;        // 상태창 매니저
    public SkillExplanationManager skillManager; // 설명창 매니저
    public GameObject gameRuleUI;

    void Awake() { rect = GetComponent<RectTransform>(); }

    void Start()
    {
        rect.localScale = Vector3.zero;
        CloseAllSubMenus();
    }

    void Update()
    {
        if (gameRuleUI != null && gameRuleUI.activeSelf)
        {
            return;
        }

        // ESC 키 입력
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // 1. 상태창이 켜져 있다면? -> 상태창 닫기 함수 호출
            if (statusUI != null && statusUI.activeSelf)
            {
                if (statusManager != null)
                {
                    statusManager.CloseStats(); // -> 이게 실행되면 상태창 꺼지고 ESC메뉴 켜짐
                }
                return; // 여기서 멈춤
            }

            // 2. 스킬 설명창이 켜져 있다면?
            if (explainationUI != null && explainationUI.activeSelf)
            {
                if (skillManager != null) skillManager.CloseProcess();
                return;
            }

            // 3. 나머지 로직 (기존 유지)
            if (IsAnySubMenuOpen())
            {
                CloseAllSubMenus();
                Show();
            }
            else if (rect.localScale == Vector3.zero)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }
    }

    // ... (Show, Hide 등 기존 함수 그대로) ...
    public void Show()
    {
        rect.localScale = Vector3.one;
        GameManager.instance.Stop();
    }

    public void Hide()
    {
        rect.localScale = Vector3.zero;
        GameManager.instance.Resume();
        CloseAllSubMenus();
    }

    public void HideForSubMenu()
    {
        rect.localScale = Vector3.zero;
    }

    bool IsAnySubMenuOpen()
    {
        if (achievementUI != null && achievementUI.activeSelf) return true;
        // 설명창, 상태창도 서브 메뉴로 간주
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
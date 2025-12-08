using UnityEngine;
using UnityEngine.UI;

public class StatusUIManager : MonoBehaviour
{
    [Header("▼ BG 안의 텍스트들을 여기에 연결해줘")]
    public Text hpText;
    public Text atkText;
    public Text atkSpdText;
    public Text critRateText;
    public Text critDmgText;

    [Header("▼ 시스템 연결")]
    public EscMenu escMenu; // ★ 무조건 여기로 돌아가기 위해 필요

    void OnEnable()
    {
        UpdateStats();
    }

    // --- [여는 함수] ---
    // 어디서 호출하든 똑같이 엽니다.
    public void OpenStats()
    {
        gameObject.SetActive(true);
        UpdateStats();
        Time.timeScale = 0f; // 시간 정지
    }

    // --- [닫는 함수] ---
    // ★ 핵심 수정: 조건문 없이 무조건 ESC 메뉴를 엽니다.
    public void CloseStats()
    {
        gameObject.SetActive(false); // 상태창 끄기

        if (escMenu != null)
        {
            // 게임 재개(Hide)가 아니라, 무조건 메뉴 오픈(Show)!
            escMenu.Show();
        }
        else
        {
            // 혹시 연결 안 됐을 때 갇히지 않게 비상용
            Time.timeScale = 1f;
        }
    }

    // --- [값 갱신 로직] ---
    public void UpdateStats()
    {
        if (StatsManager.instance == null || GameManager.instance == null) return;
        StatsManager stats = StatsManager.instance;
        GameManager gm = GameManager.instance;

        if (hpText != null) hpText.text = string.Format("{0} / {1}", Mathf.RoundToInt(gm.health), Mathf.RoundToInt(stats.MaxHP));
        if (atkText != null) atkText.text = stats.Attack.ToString("F1");
        if (atkSpdText != null) atkSpdText.text = stats.AttackSpeed.ToString("F2");
        if (critRateText != null) critRateText.text = string.Format("{0:F0}%", stats.CritChance * 100f);
        if (critDmgText != null) critDmgText.text = string.Format("{0:F0}%", stats.CritDamage * 100f);
    }
}
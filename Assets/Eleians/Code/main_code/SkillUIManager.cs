using UnityEngine;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour
{
    public static SkillUIManager instance;

    [Header("1. Tooltip Objects (툴팁 이미지 오브젝트 연결)")]
    public GameObject fireTooltip;
    public GameObject iceTooltip;
    public GameObject electricTooltip;
    public GameObject earthTooltip;

    [Header("2. Tooltip Texts (툴팁 내부 Legacy Text 연결)")]
    public Text fireText;
    public Text iceText;
    public Text electricText;
    public Text earthText;

    [Header("3. Detail Manager (설명창 매니저 연결)")]
    // ★ [변경] 단순 GameObject 대신 기능을 가진 매니저를 연결합니다.
    public SkillExplanationManager explanationManager;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        HideTooltip();

        // 시작할 때 설명창 매니저가 있다면 꺼둡니다.
        if (explanationManager != null)
            explanationManager.gameObject.SetActive(false);
    }

    // ====================================================
    // 툴팁 관련 로직 (기존과 동일)
    // ====================================================
    public void ShowTooltip(string skillType)
    {
        HideTooltip(); // 겹침 방지
        if (StatsManager.instance == null) return;

        int currentCount = 0;
        string msg = "";

        switch (skillType)
        {
            case "Fire":
                fireTooltip.SetActive(true);
                currentCount = StatsManager.instance.FireCnt;
                if (fireText != null) fireText.text = GetMessageByCount(currentCount);
                break;
            case "Ice":
                iceTooltip.SetActive(true);
                currentCount = StatsManager.instance.IceCnt;
                if (iceText != null) iceText.text = GetMessageByCount(currentCount);
                break;
            case "Electric":
                electricTooltip.SetActive(true);
                currentCount = StatsManager.instance.ElectricCnt;
                if (electricText != null) electricText.text = GetMessageByCount(currentCount);
                break;
            case "Earth":
                earthTooltip.SetActive(true);
                currentCount = StatsManager.instance.EarthCnt;
                if (earthText != null) earthText.text = GetMessageByCount(currentCount);
                break;
        }
    }

    public void HideTooltip()
    {
        if (fireTooltip) fireTooltip.SetActive(false);
        if (iceTooltip) iceTooltip.SetActive(false);
        if (electricTooltip) electricTooltip.SetActive(false);
        if (earthTooltip) earthTooltip.SetActive(false);
    }

    private string GetMessageByCount(int count)
    {
        if (count >= 0 && count <= 4) return "5 : 스킬 강화!";
        else if (count >= 5 && count <= 9) return "10 : 1차 각성!";
        else if (count >= 10 && count <= 14) return "15 : 액티브 해방!";
        else if (count >= 15 && count <= 19) return "20 : 2차 각성!";
        else return "SKILL MAX!";
    }

    // ====================================================
    // ★ [수정] 상세창(일시정지) 관련 로직
    // ====================================================

    // 버튼 클릭 시 이 함수를 호출할 거야.
    // index -> 0:전기, 1:불, 2:얼음, 3:흙 (매니저 배열 순서와 맞춰야 해!)
    public void OpenDetailPopup(int skillIndex)
    {
        HideTooltip();
        if (explanationManager != null)
        {
            // ★ [변경] 게임에서 열었다고 명시하는 함수 호출
            explanationManager.OpenFromGame(skillIndex);
        }
    }
}
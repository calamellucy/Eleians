using UnityEngine;
using UnityEngine.UI;

public class SkillUIManager : MonoBehaviour
{
    // 어디서든 접근할 수 있게 싱글톤 처리
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

    [Header("3. Detail Popup (상세 설명창 연결)")]
    public GameObject detailPopupPanel;

    private void Awake()
    {
        // 싱글톤 초기화
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        // 게임 시작 시 모든 툴팁과 팝업 숨기기
        HideTooltip();
        if (detailPopupPanel != null) detailPopupPanel.SetActive(false);
    }

    // ====================================================
    // 툴팁 관련 로직
    // ====================================================

    // 마우스 올렸을 때 호출 (스킬 타입에 따라 다른 툴팁 표시)
    public void ShowTooltip(string skillType)
    {
        // 겹침 방지를 위해 일단 모두 끔
        HideTooltip();

        int currentCount = 0;
        string msg = "";

        // StatsManager가 없으면 에러가 날 수 있으니 체크
        if (StatsManager.instance == null)
        {
            Debug.LogWarning("StatsManager가 씬에 없습니다!");
            return;
        }

        switch (skillType)
        {
            case "Fire":
                fireTooltip.SetActive(true);               // 툴팁 켜기
                currentCount = StatsManager.instance.FireCnt; // 현재 불 개수 가져오기
                msg = GetMessageByCount(currentCount);     // 개수에 맞는 멘트 가져오기
                if (fireText != null) fireText.text = msg; // 텍스트 적용
                break;

            case "Ice":
                iceTooltip.SetActive(true);
                currentCount = StatsManager.instance.IceCnt;
                msg = GetMessageByCount(currentCount);
                if (iceText != null) iceText.text = msg;
                break;

            case "Electric":
                electricTooltip.SetActive(true);
                currentCount = StatsManager.instance.ElectricCnt;
                msg = GetMessageByCount(currentCount);
                if (electricText != null) electricText.text = msg;
                break;

            case "Earth":
                earthTooltip.SetActive(true);
                currentCount = StatsManager.instance.EarthCnt;
                msg = GetMessageByCount(currentCount);
                if (earthText != null) earthText.text = msg;
                break;
        }
    }

    // 마우스 나갔을 때 호출 (모두 숨기기)
    public void HideTooltip()
    {
        if (fireTooltip) fireTooltip.SetActive(false);
        if (iceTooltip) iceTooltip.SetActive(false);
        if (electricTooltip) electricTooltip.SetActive(false);
        if (earthTooltip) earthTooltip.SetActive(false);
    }

    // 개수에 따른 텍스트 반환 로직 (요청한 조건)
    private string GetMessageByCount(int count)
    {
        if (count >= 0 && count <= 4)
        {
            return "5 : 스킬 강화!";
        }
        else if (count >= 5 && count <= 9)
        {
            return "10 : 1차 각성!";
        }
        else if (count >= 10 && count <= 14)
        {
            return "15 : 액티브 해방!";
        }
        else if (count >= 15 && count <= 19)
        {
            return "20 : 2차 각성!";
        }
        else
        {
            return "SKILL MAX!";
        }
    }

    // ====================================================
    // 상세창(일시정지) 관련 로직
    // ====================================================

    // 클릭 시 호출
    public void OpenDetailPopup(string skillName)
    {
        if (detailPopupPanel != null)
        {
            detailPopupPanel.SetActive(true);

            // 나중에 여기에 상세창 내용(이미지/설명)을 skillName에 따라 바꾸는 코드를 추가하면 돼.
            // 예: detailTitleText.text = skillName; 

            Time.timeScale = 0f; // 게임 일시정지
        }
    }

    // 닫기 버튼(X버튼)에 연결할 함수
    public void CloseDetailPopup()
    {
        if (detailPopupPanel != null)
        {
            detailPopupPanel.SetActive(false);
            Time.timeScale = 1f; // 게임 재개 (1배속)
        }
    }
}
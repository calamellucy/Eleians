using UnityEngine;
using UnityEngine.UI;

public class AchievementSlot : MonoBehaviour
{
    [Header("설정")]
    public int id; // 이 업적의 번호 (0~15)

    // 이제 인스펙터에서 안 넣어도 되니까 숨겨도 되지만, 
    // 잘 찾아졌는지 확인하고 싶다면 public으로 둬도 됨
    private GameObject lockedPanel;

    void Awake()
    {
        /*
        // 1. 내 자식 오브젝트 중에서 이름이 "Locked"인 녀석을 찾는다.
        Transform lockedTransform = transform.Find("Locked");

        // 2. 찾았다면 그 게임오브젝트를 변수에 넣는다.
        if (lockedTransform != null)
        {
            lockedPanel = lockedTransform.gameObject;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} 슬롯 안에 'Locked'라는 이름의 자식 오브젝트가 없습니다!");
        }
        */
        FindLockedPanel();
    }

    // ★ 패널 찾는 함수 분리
    void FindLockedPanel()
    {
        if (lockedPanel != null) return; // 이미 찾았으면 패스

        Transform lockedTransform = transform.Find("Locked");
        if (lockedTransform != null)
        {
            lockedPanel = lockedTransform.gameObject;
        }
        else
        {
            // 혹시 이름이 달라서 못 찾을 경우를 대비해 첫 번째 자식을 가져오는 꼼수 (선택사항)
            // if (transform.childCount > 0) lockedPanel = transform.GetChild(0).gameObject;
        }
    }

    public void UpdateSlotState()
    {
        // AchievementManager가 없거나 싱글톤 초기화 전이면 에러 날 수 있으니 체크
        if (AchievementManager.instance == null) return;

        // ★★★ [핵심 수정] 갱신하려는데 패널 변수가 비어있다면? 지금 당장 찾는다! ★★★
        if (lockedPanel == null)
        {
            FindLockedPanel();
        }

        bool isUnlocked = AchievementManager.instance.CheckUnlocked(id);

        // lockedPanel을 Awake에서 찾았으니 null 체크 후 사용
        if (lockedPanel != null)
        {
            if (isUnlocked)
            {
                // 해금됨 -> 검은 패널 끄기
                lockedPanel.SetActive(false);
            }
            else
            {
                // 잠김 -> 검은 패널 켜기
                lockedPanel.SetActive(true);
            }
        }
    }

    // (선택) 창이 켜질 때 스스로 갱신하도록 보험 들어두기
    void OnEnable()
    {
        UpdateSlotState();
    }
}
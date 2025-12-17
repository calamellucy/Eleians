using UnityEngine;
using UnityEngine.UI;

public class AchievementSlot : MonoBehaviour
{
    [Header("설정")]
    public int achievementID; // ★ 이 슬롯이 몇 번 도전과제인지 인스펙터에서 적어주세요 (0, 1, 2...)

    // 이제 인스펙터에서 안 넣어도 되니까 숨겨도 되지만, 
    // 잘 찾아졌는지 확인하고 싶다면 public으로 둬도 됨
    public GameObject darkCover;


    public void UpdateSlotState()
    {
        // 1. 데이터 매니저 확인
        if (DataManager.Instance == null) return;

        // 2. ID 유효성 체크
        if (achievementID < 0 || achievementID >= DataManager.Instance.achievementUnlocks.Length)
        {
            return;
        }

        // 3. 해금 여부 확인
        bool isUnlocked = DataManager.Instance.achievementUnlocks[achievementID];

        // 4. 패널 상태 결정
        if (darkCover != null)
        {
            // 잠겨있으면(!isUnlocked) -> 어두운 패널을 켠다(Active True)
            // 해금됐으면(isUnlocked) -> 어두운 패널을 끈다(Active False) -> 밝아짐!
            darkCover.SetActive(!isUnlocked);
        }
    }
}
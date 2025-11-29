using UnityEngine;

public class IceActive : ActiveSkillBase
{
    // 위에서 만든 스킬 로직 스크립트 연결
    public IceShieldSkill iceSkill;

    // 1. 활성화 조건 (얼음 속성 15 이상)
    protected override bool IsSkillUnlocked()
    {
        if (StatsManager.instance == null) return false;

        // [주의] StatsManager 변수명 확인 (IceCnt)
        bool canUse = StatsManager.instance.IceCnt >= 15;

        // 조건 안 되면 아이콘 숨기기 (원치 않으면 삭제 가능)
        // transform.localScale = canUse ? Vector3.one : Vector3.zero; // 필요 시 주석 해제

        return canUse;
    }

    // 2. 입력 키 (E키 - 불 스킬과 겹친다면 키 변경 필요, 예: R)
    protected override bool CheckInput()
    {
        // 원래 코드에 E키라고 되어 있어서 E로 했습니다.
        // 만약 불 스킬도 E라면 키를 바꿔야 합니다!
        return Input.GetKeyDown(KeyCode.R);
    }

    // 3. 스킬 발동
    protected override void ActivateSkill()
    {
        // 플레이어에 있는 스킬 로직 실행
        iceSkill.ActiveIceShield();
    }
}
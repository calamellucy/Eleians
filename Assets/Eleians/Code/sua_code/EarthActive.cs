using UnityEngine;

public class EarthActive : ActiveSkillBase
{
    // [수정] 기존 sk4 변수는 필요 없다면 제거해도 됩니다.
    // public Skill4 sk4; 

    public EarthBumpSkill earthbump; // 실제 스킬 스크립트

    // 1. 활성화 조건 (StatsManager 체크 + 아이콘 숨김/표시)
    protected override bool IsSkillUnlocked()
    {
        // 매니저가 없으면 비활성
        if (StatsManager.instance == null) return false;

        // [중요] 흙 속성 수치가 15 이상인지 확인
        return StatsManager.instance.EarthCnt >= 15;
    }

    // 2. 입력 키 (Q키)
    protected override bool CheckInput()
    {
        return Input.GetKeyDown(KeyCode.Alpha4);
    }

    // 3. 스킬 발동
    protected override void ActivateSkill()
    {
        earthbump.ActiveEarthBump();
    }
}
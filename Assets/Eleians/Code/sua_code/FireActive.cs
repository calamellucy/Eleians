using UnityEngine;

// [중요] ActiveSkillBase를 상속받습니다.
public class FireActive : ActiveSkillBase
{
    // 연결할 실제 스킬 스크립트
    public FireExplosionSkill fireSkill;

    // [참고] 쿨타임 변수(coolTime)와 테두리 이미지(borderImage)는 
    // 부모 클래스(ActiveSkillBase)에 이미 있으므로 여기서 선언 안 해도 됩니다!
    // 유니티 Inspector에서 'Common Settings' 부분에서 설정하세요.

    // 1. 활성화 조건 (StatsManager 체크 + 아이콘 숨김/표시)
    protected override bool IsSkillUnlocked()
    {
        // 매니저가 없으면 비활성
        if (StatsManager.instance == null) return false;

        // 불 속성 15 이상인지 확인
        return StatsManager.instance.FireCnt >= 15;
    }

    // 2. 입력 키 설정 (E키)
    protected override bool CheckInput()
    {
        return Input.GetKeyDown(KeyCode.E);
    }

    // 3. 실제 스킬 발동
    protected override void ActivateSkill()
    {
        // 기존 코드에 있던 스킬 발동 함수 호출
        fireSkill.ActiveChainExplosion();
    }
}
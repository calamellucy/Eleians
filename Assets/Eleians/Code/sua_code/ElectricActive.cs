using UnityEngine;

public class ElectricActive : ActiveSkillBase
{
    // [핵심] 기존 스크립트(Skill1_Re)를 변수로 받습니다.
    public Skill1_Re skillScript;
    public KeyCode triggerKey = KeyCode.Alpha1;

    // 1. 활성화 조건
    protected override bool IsSkillUnlocked()
    {
        if (StatsManager.instance == null) return false;

        // 전기 속성 15 이상 확인
        bool canUse = StatsManager.instance.ElectricCnt >= 15;

        // 아이콘 숨김/표시 (필요 없으면 삭제)
        // if (canUse) transform.localScale = Vector3.one;
        // else transform.localScale = Vector3.zero;

        return canUse;
    }

    // 2. 입력 키 (흙 스킬과 키 겹치지 않게 주의!)
    protected override bool CheckInput()
    {
        return Input.GetKeyDown(triggerKey);
    }

    // 3. 스킬 발동
    protected override void ActivateSkill()
    {
        // Skill1_Re에 뚫어놓은 public 함수 호출
        skillScript.CastActiveSkill();
    }
}
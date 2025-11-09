using UnityEngine;

public class TowerMonster : NormalMonster
{
    // NormalMonster의 OnEnable() 대신 Tower를 타겟으로 지정
    protected override void OnEnable()
    {
        base.OnEnable(); // NormalMonster의 초기화 과정 유지

        // target을 Tower로 변경
        if (GameManager.instance.tower != null)
            target = GameManager.instance.tower.GetComponent<Rigidbody2D>();
    }
}

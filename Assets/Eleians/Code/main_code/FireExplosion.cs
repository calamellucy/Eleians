using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExplosion : MonoBehaviour
{
    public float damageCoefficient = 3.0f; // 공격력 계수 (300%)

    CapsuleCollider2D col;
    HashSet<Collider2D> hitTargets = new HashSet<Collider2D>(); // 중복 피격 방지

    void Awake()
    {
        col = GetComponent<CapsuleCollider2D>();
    }

    void OnEnable()
    {
        // 1. 활성화 시 콜라이더 끄고, 타겟 리스트 초기화
        col.enabled = false;
        hitTargets.Clear();

        // 2. 2프레임 뒤에 콜라이더 켜는 코루틴 실행
        StartCoroutine(EnableColliderDelay());
    }

    IEnumerator EnableColliderDelay()
    {
        // 2 프레임 대기 (물리 업데이트 기준)
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        col.enabled = true;
    }

    // ★ 애니메이션의 마지막 프레임에 Add Event로 이 함수를 연결해줘!
    public void Delete()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 태그 체크 & 중복 체크
        if (!other.CompareTag("Enemy") || hitTargets.Contains(other)) return;

        hitTargets.Add(other);

        // 데미지 계산 (StatsManager 의존)
        float finalDamage = StatsManager.instance.Attack * damageCoefficient;
        // 불 속성 스택에 따른 추가 데미지 공식이 있다면 여기에 추가
        // finalDamage *= (1 + StatsManager.instance.FireCnt * 0.1f);

        // 크리티컬 적용
        finalDamage = StatsManager.instance.ApplyCrit(finalDamage);

        // 몬스터에게 데미지 전달
        NormalMonster monster = other.GetComponent<NormalMonster>();
        if (monster != null)
        {
            monster.ApplyDamage(finalDamage);
        }
    }
}
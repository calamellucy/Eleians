using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireExplosion : MonoBehaviour
{
    public float damageCoefficient = 3.0f; // 공격력 계수 (300%)

    // DamageReceiver가 가져갈 최종 데미지값
    [HideInInspector] public float finalDamage;

    CapsuleCollider2D col;

    void Awake()
    {
        col = GetComponent<CapsuleCollider2D>();
    }

    void OnEnable()
    {
        col.enabled = false;

        // 1. 활성화되는 순간 스탯 + 계수 + 크리티컬까지 모두 계산해서 'finalDamage'에 저장
        CalculateFinalDamage();

        StartCoroutine(EnableColliderDelay());
    }

    void CalculateFinalDamage()
    {
        finalDamage = StatsManager.instance.Attack * damageCoefficient;
    }

    IEnumerator EnableColliderDelay()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        col.enabled = true;
    }

    public void Delete()
    {
        gameObject.SetActive(false);
    }
}
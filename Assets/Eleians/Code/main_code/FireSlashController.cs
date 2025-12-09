using System.Collections;
using UnityEngine;

public class FlameSwordController : MonoBehaviour
{
    [Header("프리팹 ID (검 본체:13 / 검기:8)")]
    public int swordPrefabId = 13;

    [Header("기본 스펙")]
    public float baseInterval = 1.5f;
    public float baseScale = 3f;
    public float subAngle = 55f;        // 기본 각도 보정
    public float returnAngle = 7f;      // 복귀 시 추가 각도 보정

    [Header("투사체 설정")]
    public float projectileBaseScale = 1.0f; // 검기 기본 크기

    private float timer;

    // 불 20 스택
    private int fireStack = 0;
    private const int MaxFireStack = 7;

    void Update()
    {
        timer += Time.deltaTime;

        // ■ 흙 시너지: 공격속도 감소 (최대 75%까지만 감소하도록 안전장치)
        float earthSpeedMod = Mathf.Min(0.75f, StatsManager.instance.EarthCnt * 0.03f);
        float finalAttackSpeed = StatsManager.instance.AttackSpeed * (1 + earthSpeedMod);

        float currentInterval = baseInterval / finalAttackSpeed;

        if (timer >= currentInterval)
        {
            timer = 0f;
            UseSkill();
        }
    }

    void UseSkill()
    {
        // ==========================================
        // 1. 검(Sword) 스펙 계산
        // ==========================================
        float baseDmg = StatsManager.instance.Attack * 1.5f;
        float iceMod = 1f + (StatsManager.instance.IceCnt * 0.04f);
        float swordFinalDamage = baseDmg * iceMod;

        float fireScaleMod = 1f + (StatsManager.instance.FireCnt * 0.04f);

        // [불 5] 검 크기 증가 (GDD 기준 0.35f)
        if (StatsManager.instance.FireCnt >= 5)
            fireScaleMod += 0.1f;

        // [불 20] 스택 시스템 (GDD: 7% 중첩)
        bool isTriggerAttack = false;
        if (StatsManager.instance.FireCnt >= 20)
        {
            fireStack++;
            float stackBonus = fireStack * 0.07f;

            fireScaleMod += stackBonus;
            swordFinalDamage *= (1f + stackBonus);

            if (fireStack >= MaxFireStack)
            {
                isTriggerAttack = true;
                fireStack = 0;
            }
        }

        float swordFinalScale = baseScale * fireScaleMod;

        // ==========================================
        // 2. 투사체(Projectile) 스펙 계산
        // ==========================================
        // 데미지: 공격력 * 0.6
        float projDamage = StatsManager.instance.Attack * 0.6f;

        // 크기: (1 + 불 개수 * 0.04) 배율
        float projScaleMultiplier = 1f + (StatsManager.instance.FireCnt * 0.04f);
        float projFinalScale = projectileBaseScale * projScaleMultiplier;


        // ==========================================
        // 3. 스킬 실행
        // ==========================================
        Vector2 dir = GetTargetDirection();

        // 첫 번째 베기 (원본 크기)
        SpawnSword(dir, swordFinalDamage, swordFinalScale, projDamage, projFinalScale, false, isTriggerAttack);

        // [불 5] 되돌아오는 베기
        if (StatsManager.instance.FireCnt >= 5)
        {
            StartCoroutine(SpawnReturnSlash(dir, swordFinalDamage, swordFinalScale, projDamage, projFinalScale, isTriggerAttack));
        }
    }

    IEnumerator SpawnReturnSlash(Vector2 dir, float sDmg, float sScale, float pDmg, float pScale, bool isTrigger)
    {
        // 딜레이 0.45초
        yield return new WaitForSeconds(0.45f);
        SpawnSword(dir, sDmg * 0.75f, sScale * 0.93f, pDmg, pScale * 0.93f, true, isTrigger);
    }

    void SpawnSword(Vector2 dir, float sDmg, float sScale, float pDmg, float pScale, bool isReverse, bool isTriggerAttack)
    {
        Transform swordT = GameManager.instance.pool.Get(swordPrefabId).transform;

        // 각도 계산
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 각도 보정 (돌아올 땐 returnAngle 만큼 더 꺾음)
        float finalAngle = isReverse ? (angle + subAngle - returnAngle) : (angle - subAngle);

        Quaternion rotation = Quaternion.AngleAxis(finalAngle, Vector3.forward);
        swordT.rotation = rotation;

        FlameSword swordScript = swordT.GetComponent<FlameSword>();
        if (swordScript != null)
        {
            // 투사체 정보(pDmg, pScale) 전달 -> pScale은 이미 줄어든 상태로 전달됨
            swordScript.Init(sDmg, sScale, pDmg, pScale, isReverse, isTriggerAttack, dir, this.transform);
        }
    }

    Vector2 GetTargetDirection()
    {
        Transform[] targets = GameManager.instance.player.scans.GetNearest(1);
        Transform target = (targets != null && targets.Length > 0) ? targets[0] : null;

        if (target != null)
            return (target.position - transform.position).normalized;
        else
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            return (dir == Vector2.zero) ? Vector2.right : dir;
        }
    }
}
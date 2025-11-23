using System.Collections;
using UnityEngine;

public class BossMonster : NormalMonster
{
    [Header("Boss Settings")]
    public Transform player;           // 플레이어 위치
    public float patternInterval = 1f; // 패턴 사이 짧은 숨 고르기 시간

    [Header("Shadow Rain")]
    public GameObject shadowSpearPrefab;  // 그림자 창 프리팹
    public float shadowRainDuration = 7f; // 패턴 지속 시간
    public float shadowRainSpawnRate = 3f; // 초당 몇 개 (3개)

    [Header("Binding Field")]
    public GameObject bindingFieldPrefab; // 속박 장판 (보스 주변 원)
    public float bindingFieldDuration = 8f;

    [Header("Dark Sigil")]
    public DarkSigilController sigilController; // 어둠의 인장 제어용(마법진 8조각)

    [Header("Dark Vision")]
    public DarkVisionController darkVision; // 화면 어둡게 만드는 UI 컨트롤러
    public float darkVisionDuration = 6f;

    Coroutine patternRoutine;

    protected override void OnEnable()
    {
        base.OnEnable();

        // Player를 타겟으로
        target = GameManager.instance.player.GetComponent<Rigidbody2D>();

        if (patternRoutine != null)
            StopCoroutine(patternRoutine);

        patternRoutine = StartCoroutine(PatternLoop());
    }

    public override void Die(bool giveReward)
    {
        base.Die(giveReward);

        // 보스 죽으면 패턴 중지
        if (patternRoutine != null)
            StopCoroutine(patternRoutine);

        // 암흑 시야 켜져 있으면 끄기
        if (darkVision != null)
            darkVision.DisableImmediately();
    }

    public void BossInit()
    {
        speed = 1;
        maxHealth = 10000;
        damage = 50;
        health = maxHealth;
    }

    public void BossSpawn()
    {
        BossInit();
        gameObject.SetActive(true);
    }

    public override void ApplyDamage(float dmg, int skillType)
    {
        base.ApplyDamage(dmg, skillType);
    }

    // 보스 패턴
    IEnumerator PatternLoop()
    {
        // 필요하면 등장 모션/인트로 대기
        yield return new WaitForSeconds(1f);

        while (isLive)
        {
            // 1. 암흑 시야
            yield return DarkVisionPattern();
            yield return new WaitForSeconds(patternInterval);

            // 2. 그림자 낙하
            yield return ShadowRainPattern();
            yield return new WaitForSeconds(patternInterval);

            // 3. 어둠의 인장
            yield return DarkSigilPattern();
            yield return new WaitForSeconds(patternInterval);

            // 나중에 Phase 나누고 싶으면 여기서 HP 비율에 따라 패턴 변경 가능
        }
    }

    // ---------------- 패턴별 코루틴 골격 ----------------

    IEnumerator DarkVisionPattern()
    {
        if (!isLive) yield break;

        // 모션: 암흑 시야 캐스팅 애니메이션
        if (anim != null)
            anim.SetTrigger("castDarkVision");

        // 화면 어둡게
        if (darkVision != null)
            darkVision.Enable(darkVisionDuration);

        float t = 0f;
        while (t < darkVisionDuration && isLive)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // darkVision 쪽에서 duration 끝나면 자동으로 꺼지게 해도 되고,
        // 여기서 꺼도 됩니다. (중복 방지용 안전 체크)
        if (darkVision != null)
            darkVision.Disable();
    }

    IEnumerator ShadowRainPattern()
    {
        if (!isLive || player == null) yield break;

        if (anim != null)
            anim.SetTrigger("castShadowRain"); // 지팡이 들어올리는 모션

        float elapsed = 0f;
        float spawnInterval = 1f / shadowRainSpawnRate; // ex) 3개/초 → 0.333초마다 스폰
        float spawnTimer = 0f;

        while (elapsed < shadowRainDuration && isLive)
        {
            elapsed += Time.deltaTime;
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                spawnTimer -= spawnInterval;
                SpawnShadowSpear();
            }

            yield return null;
        }
    }

    void SpawnShadowSpear()
    {
        if (shadowSpearPrefab == null || player == null) return;

        // 플레이어 주변 랜덤 위치
        float radius = 3f; // 원하는 범위
        Vector2 rand = Random.insideUnitCircle * radius;
        Vector3 pos = player.position + new Vector3(rand.x, rand.y, 0f);

        // 스폰
        GameObject spear = Instantiate(shadowSpearPrefab, pos, Quaternion.identity);

        // spear 안에서:
        // 0.6초 동안 보라색 경고 원 → 이후 창 떨어지고,
        // Player 맞으면 HP 20% 감소 처리
        // 이 로직은 spear 스크립트에 넣으시면 됩니다.
    }


    IEnumerator DarkSigilPattern()
    {
        if (!isLive) yield break;
        if (sigilController == null)
        {
            // 아직 마법진 구현 안 했으면 그냥 대기만
            yield return new WaitForSeconds(7f);
            yield break;
        }

        // 어둠의 인장 시작 (sigilController가 1,2차 폭발까지 내부에서 처리)
        sigilController.StartSigilPattern();

        float duration = 7f; // 전체 패턴 시간 대략
        float t = 0f;
        while (t < duration && isLive)
        {
            t += Time.deltaTime;
            yield return null;
        }

        sigilController.StopSigilPattern();
    }
}

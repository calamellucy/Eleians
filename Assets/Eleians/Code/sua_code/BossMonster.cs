using System.Collections;
using UnityEngine;

public class BossMonster : NormalMonster
{
    [Header("Boss Settings")]
    public Transform player;           // 플레이어 위치
    public float patternInterval = 1f; // 패턴 사이 짧은 숨 고르기 시간
    public bool isBattleReady = false;

    [Header("Shadow Rain")]
    public GameObject shadowSpearPrefab;  // 그림자 창 프리팹
    public float shadowRainDuration = 7f; // 패턴 지속 시간
    public float shadowRainSpawnRate = 3f; // 초당 몇 개 (3개)

    [Header("Dark Vision")]
    public DarkVisionController darkVision; // 화면 어둡게 만드는 UI 컨트롤러
    public float darkVisionDuration = 6f;

    [Header("Ghost Swarm")]
    public GameObject ghostPrefab;      // ★ 유령 프리팹 (GhostMonster 스크립트 붙은 거)
    public int ghostSpawnCount = 10;    // 한 번에 몇 마리?
    public float ghostSpawnInterval = 0.2f; // 다다다다 간격
    public float ghostSpawnRadius = 2.0f;   // 보스 주변 몇 미터?

    Coroutine patternRoutine;

    protected override void OnEnable()
    {
        base.OnEnable();

        isBattleReady = false;

        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            player = GameManager.instance.player.transform;
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        }

        // if (patternRoutine != null) StopCoroutine(patternRoutine);

        // patternRoutine = StartCoroutine(PatternLoop());
    }

    // ★ [추가] 부모(NormalMonster)의 이동 로직을 멈추기 위해 추가
    protected override void FixedUpdate()
    {
        // 전투 준비가 안 됐으면 움직이지 마라 (물리엔진 정지)
        if (!isBattleReady)
        {
            rigid.linearVelocity = Vector2.zero; // 제자리 고정
            return;
        }

        base.FixedUpdate();
    }

    // NormalMonster의 LateUpdate를 덮어씌웁니다.
    protected new void LateUpdate()
    {
        if (!isLive) return;

        // 원본 그림이 반대라서, 로직도 반대로 뒤집어 줌
        spriter.flipX = target.position.x < rigid.position.x;
    }

    // ★★★ [핵심] GameManager가 호출해줄 "전투 시작" 함수 ★★★
    public void StartBattle()
    {
        isBattleReady = true;

        Debug.Log("보스 전투 시작!");

        // 패턴 코루틴 시작
        if (patternRoutine != null) StopCoroutine(patternRoutine);
        patternRoutine = StartCoroutine(PatternLoop());
    }

    public override void Die(bool giveReward)
    {
        // 1. 모든 패턴 코루틴(창 던지기, 암흑 시야 대기 등) 즉시 중단
        StopAllCoroutines();

        // 2. [암흑 시야] 켜져 있다면 즉시 끄기 (화면 밝아짐)
        if (darkVision != null)
            darkVision.DisableImmediately();

        // 3. [유령 군단] 살아있는 유령 모두 찾아서 죽이기
        GhostMonster[] ghosts = FindObjectsByType<GhostMonster>(FindObjectsSortMode.None);
        foreach (var ghost in ghosts)
        {
            if (ghost.gameObject.activeSelf)
                ghost.Die(false); // 보상 없이 즉사
        }

        base.Die(giveReward);

        GameManager.instance.GameClear();

        /*
        // 보스 죽으면 패턴 중지
        if (patternRoutine != null)
            StopCoroutine(patternRoutine);

        // 암흑 시야 켜져 있으면 끄기
        if (darkVision != null)
            darkVision.DisableImmediately();
        */
    }

    public void BossInit()
    {
        speed = 1f; // 
        originalSpeed = speed;
        maxHealth = 10000;
        health = maxHealth;
        damage = 50;
    }

    public void BossSpawn()
    {
        BossInit();
        gameObject.SetActive(true);
    }

    public override void ApplyDamage(float dmg, ElementType element = ElementType.None)
    {
        base.ApplyDamage(dmg, ElementType.None);

        // 2. ★ 체력바 UI 갱신 (GameManager에게 현재 체력 전달)
        // (죽었어도 0으로 갱신하기 위해 isLive 체크 뒤보다는 여기가 나음)
        GameManager.instance.UpdateBossHealthUI(health, maxHealth);
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

            // ★ 3. [신규] 유령 군단 소환
            yield return GhostSwarmPattern();
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

    // ----------------------------------------------------
    // ★★★ [신규] 유령 소환 패턴 (다다다다!) ★★★
    // ----------------------------------------------------
    IEnumerator GhostSwarmPattern()
    {
        if (!isLive) yield break;

        Debug.Log("패턴: 유령 군단 소환");

        // 1. 소환 모션 (있다면)
        if (anim != null)
            anim.SetTrigger("castSummon"); // 소환 애니메이션 트리거 이름

        // 모션 선딜레이 (잠깐 폼 잡는 시간)
        yield return new WaitForSeconds(0.5f);

        // 2. 다다다다 소환 시작
        for (int i = 0; i < ghostSpawnCount; i++)
        {
            if (!isLive) yield break;

            SpawnGhost();

            // 다음 유령 나올 때까지 잠깐 대기 (다다다다 효과)
            yield return new WaitForSeconds(ghostSpawnInterval);
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

        Debug.Log($"창 생성됨! 위치: {pos}, 활성상태: {spear.activeSelf}");

        // spear 안에서:
        // 0.6초 동안 보라색 경고 원 → 이후 창 떨어지고,
        // Player 맞으면 HP 20% 감소 처리
        // 이 로직은 spear 스크립트에 넣으시면 됩니다.
    }

    void SpawnGhost()
    {
        if (ghostPrefab == null) return;

        // 보스 주변 랜덤 위치 계산 (원형)
        Vector2 randPos = Random.insideUnitCircle.normalized * ghostSpawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randPos.x, randPos.y, 0);

        // 생성
        GameObject ghostObj = Instantiate(ghostPrefab, spawnPos, Quaternion.identity);

        // 스탯 설정
        GhostMonster ghostScript = ghostObj.GetComponent<GhostMonster>();
        if (ghostScript != null)
        {
            // 체력 100, 공격력 10, 속도 2.5 (원하는 대로 조절)
            ghostScript.InitGhost(100f, 10f, 2f);
        }
    }
}

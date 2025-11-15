using System.Collections;
using UnityEngine;

/// <summary>
/// Skill4
/// - 플레이어 주변(0.3~1.0)에서 스폰
/// - 0.8s 페이드인 → 0.2s 대기 후 발사
/// - 발사 방향: 입력이 있으면 입력 방향, 없으면 '마지막 비영 입력 방향' 유지
/// - 버스트: 45발/1.5s, 4초 간격
/// - 발사 후 2초 뒤 비활성화
/// </summary>
public class Skill4 : MonoBehaviour
{
    [Header("Pool / Prefab")]
    public int prefabId = 1;

    [Header("Move / Damage")]
    public float speed = 8f;
    public float damage = 10f;
    public float lifeTime = 2f;          // 발사 "후" 생존시간

    [Header("Burst")]
    public float burstInterval = 4f;     // 버스트 간격
    public int shotsPerBurst = 45;       // 버스트당 발사 수
    public float burstDuration = 1.5f;   // 버스트 진행 시간

    [Header("Spawn Range (ring)")]
    public float minSpawnRadius = 0.3f;
    public float maxSpawnRadius = 1.0f;

    [Header("Aiming")]
    public bool useLastAimingWhenIdle = true; // 입력이 0일 때 마지막 조준 방향 유지 여부
    private Vector2 lastAimDir = Vector2.right; // 마지막 비영(非0) 입력 방향

    private Coroutine loopCo;

    // 🔹 스킬이 처음 생성될 때, 전체 스킬 루프(지속적으로 버스트를 반복하는 코루틴)를 시작하는 함수
    void Start()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = StartCoroutine(Loop());
    }
    void Update()
    {
        var player = GameManager.instance.player;
        if (player == null) return;

        // 입력이 있으면 그 방향으로 갱신
        if (player.inputVec.sqrMagnitude > 0.0001f)
        {
            lastAimDir = player.inputVec.normalized;
        }
        // 입력이 없을 때, 유지 모드가 아니면 바라보는 좌/우로 덮어씀
        else if (!useLastAimingWhenIdle)
        {
            lastAimDir = player.IsFacingRight ? Vector2.right : Vector2.left;
        }
    }

    // 🔹 스킬의 ‘버스트 패턴’을 무한 반복하는 메인 루프
    //    - GameManager/Pool/Player 참조 체크
    //    - Burst() 한 번 실행 → burstInterval 만큼 대기 → 다시 Burst()
    IEnumerator Loop()
    {
        // 필수 참조 검사
        if (GameManager.instance == null || GameManager.instance.pool == null || GameManager.instance.player == null)
        {
            Debug.LogError("GameManager / PoolManager / Player reference missing");
            yield break;
        }
        if (prefabId < 0 || prefabId >= GameManager.instance.pool.prefabs.Length)
        {
            Debug.LogError($"prefabId {prefabId} out of range");
            yield break;
        }

        while (true)
        {
            // 한 번의 ‘탄막 버스트’를 실행
            yield return StartCoroutine(Burst());
            // 버스트 간 대기 시간
            yield return new WaitForSeconds(burstInterval);
        }
    }

    // 🔹 한 번의 버스트(탄막 쏟아붓기)를 담당하는 함수
    //    - shotsPerBurst만큼 반복해서 SpawnAndPrepare() 호출
    //    - burstDuration 동안 균등하게 나눠서 발사(간격 gap)
    IEnumerator Burst()
    {
        float gap = burstDuration / Mathf.Max(1, shotsPerBurst);
        for (int i = 0; i < shotsPerBurst; i++)
        {
            // 각 탄환을 생성하고, 페이드인/발사를 담당하는 코루틴을 내부적으로 시작
            SpawnAndPrepare();
            yield return new WaitForSeconds(gap);
        }
    }

    // 🔹 개별 탄환 1발을 실제로 스폰하고,
    //    - 플레이어 주변 랜덤 위치에 배치
    //    - 조준 방향 계산(입력 or 마지막 입력 or 바라보는 방향)
    //    - Rigidbody 및 Bullet 세팅
    //    - 페이드인 후 발사하는 코루틴(FadeAndFire)을 시작
    void SpawnAndPrepare()
    {
        var player = GameManager.instance.player;

        // 플레이어 주변 0.3~1.0 반경 랜덤 스폰 (면적 균등)
        Vector3 origin = player.transform.position;
        Vector2 randDir = Random.insideUnitCircle.normalized;
        float r = Mathf.Sqrt(Random.Range(minSpawnRadius * minSpawnRadius,
                                          maxSpawnRadius * maxSpawnRadius));
        Vector3 spawnPos = origin + (Vector3)(randDir * r);

        // 풀에서 프리팹 가져오기
        GameObject go = GameManager.instance.pool.Get(prefabId);
        if (!go) return;

        Transform t = go.transform;
        t.SetParent(transform, false);
        t.position = spawnPos;

        // 🔹 조준 방향은 Update에서 계속 갱신된 lastAimDir 사용
        Vector2 fireDir = lastAimDir;

        // 혹시라도 0벡터면(초기 상황 등) 바라보는 방향으로 한 번 더 보정
        if (fireDir.sqrMagnitude < 0.0001f)
            fireDir = player.IsFacingRight ? Vector2.right : Vector2.left;

        // 탄환 스프라이트 회전
        float angle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
        t.rotation = Quaternion.Euler(0, 0, angle);

        // Rigidbody2D 세팅: 발사 전까지는 정지 상태로 두기
        var rb = go.GetComponent<Rigidbody2D>();
        if (!rb) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;

        // (선택) 탄환 데미지, 관통 등 초기화
        var b = go.GetComponent<Bullet>();
        if (b) b.Init(damage, 3, Vector3.zero);

        // 페이드인 → 짧은 대기 → 발사를 처리하는 코루틴 시작
        StartCoroutine(FadeAndFire(go, fireDir));
    }

    // 🔹 한 탄환에 대해:
    //    1) 완전 투명 상태로 시작
    //    2) 0.8초 동안 서서히 나타나는 페이드인
    //    3) 0.2초 정지 상태 유지
    //    4) 지정된 방향/속도로 발사
    //    5) lifeTime 이후 비활성화
    IEnumerator FadeAndFire(GameObject go, Vector2 dir)
    {
        if (!go) yield break;

        // 프리팹 및 자식의 모든 SpriteRenderer 수집
        var renderers = go.GetComponentsInChildren<SpriteRenderer>(true);

        // 0) 완전 투명으로 시작
        SetAlpha(renderers, 0f);

        // 1) 0.8초 페이드인
        float fadeTime = 0.5f;
        float t = 0f;
        float stoppedTime = 0.8f - fadeTime;
        while (t < fadeTime && go.activeInHierarchy)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeTime);
            SetAlpha(renderers, a);
            yield return null;
        }

        // 2) 0.2초 대기(정지)
        yield return new WaitForSeconds(stoppedTime);

        // 3) 발사(속도 부여)
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = dir * speed;

        // 4) lifeTime 후 비활성화
        yield return new WaitForSeconds(lifeTime);
        if (go) go.SetActive(false);
    }

    // 🔹 전달받은 SpriteRenderer 배열의 알파값(투명도)을 일괄 변경하는 유틸리티 함수
    //    - 페이드인/페이드아웃 같은 이펙트용으로 사용
    void SetAlpha(SpriteRenderer[] srs, float a)
    {
        for (int i = 0; i < srs.Length; i++)
        {
            var c = srs[i].color;
            c.a = a;
            srs[i].color = c;
        }
    }
}

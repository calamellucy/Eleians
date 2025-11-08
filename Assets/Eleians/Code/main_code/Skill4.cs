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
    public int prefabId;

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

    void Start()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = StartCoroutine(Loop());
    }

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
            yield return StartCoroutine(Burst());
            yield return new WaitForSeconds(burstInterval);
        }
    }

    IEnumerator Burst()
    {
        float gap = burstDuration / Mathf.Max(1, shotsPerBurst);
        for (int i = 0; i < shotsPerBurst; i++)
        {
            SpawnAndPrepare();                // 각 탄은 내부 코루틴에서 페이드→발사
            yield return new WaitForSeconds(gap);
        }
    }

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

        // 발사 방향 결정: 입력 있으면 그 방향(캐싱), 없으면 캐싱 또는 좌/우
        Vector2 fireDir;
        if (player.inputVec.sqrMagnitude > 0.0001f)
        {
            fireDir = player.inputVec.normalized;
            if (useLastAimingWhenIdle) lastAimDir = fireDir; // 마지막 유효 방향 캐싱
        }
        else
        {
            fireDir = useLastAimingWhenIdle
                ? lastAimDir
                : (player.IsFacingRight ? Vector2.right : Vector2.left);
        }

        // 보이는 방향 회전(필요 시 스프라이트 오프셋은 angle += offset;)
        float angle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
        t.rotation = Quaternion.Euler(0, 0, angle);

        // Rigidbody2D 세팅: 발사 전까지는 정지
        var rb = go.GetComponent<Rigidbody2D>();
        if (!rb) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;

        // (선택) 데미지 초기화
        var b = go.GetComponent<Bullet>();
        if (b) b.Init(damage, 3, Vector3.zero);

        // 페이드인 → 대기 → 발사
        StartCoroutine(FadeAndFire(go, fireDir));
    }

    IEnumerator FadeAndFire(GameObject go, Vector2 dir)
    {
        if (!go) yield break;

        // 프리팹 및 자식의 모든 SpriteRenderer 수집
        var renderers = go.GetComponentsInChildren<SpriteRenderer>(true);

        // 0) 완전 투명으로 시작
        SetAlpha(renderers, 0f);

        // 1) 0.8초 페이드인
        float fadeTime = 0.8f;
        float t = 0f;
        while (t < fadeTime && go.activeInHierarchy)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeTime);
            SetAlpha(renderers, a);
            yield return null;
        }

        // 2) 0.2초 대기(정지)
        yield return new WaitForSeconds(0.2f);

        // 3) 발사
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = dir * speed;

        // 4) lifeTime 후 비활성화
        yield return new WaitForSeconds(lifeTime);
        if (go) go.SetActive(false);
    }

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

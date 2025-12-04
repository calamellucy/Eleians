using System.Collections;
using UnityEngine;

public class Skill4 : MonoBehaviour
{
    [Header("References")]
    public ScanALot scans;
    public StoneDust stoneDustComp;

    [Header("Pool / Prefab")]
    public int prefabId = 1;

    [Header("Move / Damage")]
    public float speed = 8f;
    public float damage = 30f;
    public float lifeTime = 2f;

    [Header("Burst")]
    public float burstInterval = 3.33f;
    public int shotsPerBurst = 30;
    public float burstDuration = 1.3f;
    public int per = 0;
    public Vector3 baseBulletScale = Vector3.one;

    [Header("Spawn Range")]
    public float minSpawnRadius = 0.3f;
    public float maxSpawnRadius = 1.0f;

    [Header("Aiming")]
    public bool useLastAimingWhenIdle = true;
    private Vector2 lastAimDir = Vector2.left;

    [Header("Triggers")]
    public bool StoneDust = false;
    public bool StoneActive = false;
    public bool VibrationalWave = false;

    private Coroutine loopCo;
    private Coroutine spikeCo;

    void Awake()
    {
        if (scans == null) scans = GetComponentInParent<ScanALot>();
        if (stoneDustComp == null) stoneDustComp = GetComponentInParent<StoneDust>();
    }

    /*
    void Start()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = StartCoroutine(Loop());

        if (spikeCo != null) StopCoroutine(spikeCo);
        spikeCo = StartCoroutine(SpikeLoop());
    }
    */
    
    //여기부터 수정
    void OnEnable()
    {
        // 1. 혹시 돌고 있던 게 있다면 끄고 (안전장치)
        if (loopCo != null) StopCoroutine(loopCo);
        if (spikeCo != null) StopCoroutine(spikeCo);

        // 2. 다시 코루틴 시작!
        loopCo = StartCoroutine(Loop());
        spikeCo = StartCoroutine(SpikeLoop());
    }

    // (보너스) 꺼질 때 코루틴 변수 정리
    void OnDisable()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        if (spikeCo != null) StopCoroutine(spikeCo);
    }
    // 여기까지 수정

    void Update()
    {
        var player = GameManager.instance.player;
        if (player == null) return;

        if (player.inputVec.sqrMagnitude > 0.0001f)
            lastAimDir = player.inputVec.normalized;
        else if (!useLastAimingWhenIdle)
            lastAimDir = player.IsFacingRight ? Vector2.right : Vector2.left;
    }

    public void GiveLevelSystemToSkill4()
    {
        baseBulletScale = Vector3.one * (1f + StatsManager.instance.FireCnt * 0.04f) * 1.4f;
        damage = StatsManager.instance.Attack * (1f + 0.04f * StatsManager.instance.IceCnt) * 0.5f; burstInterval = 1f / (StatsManager.instance.AttackSpeed * 0.3f);
        float baseInterval = 1f / (StatsManager.instance.AttackSpeed * 0.3f);
        burstInterval = baseInterval * Mathf.Max(0.1f, (1f - StatsManager.instance.ElectricCnt * 0.05f));
        shotsPerBurst = 30 + StatsManager.instance.EarthCnt;
        if (StatsManager.instance.EarthCnt >= 5) { shotsPerBurst += 10; per = 4; }
        if (StatsManager.instance.EarthCnt >= 10) StoneDust = true;
        if (StatsManager.instance.EarthCnt >= 15) StoneActive = true;
        if (StatsManager.instance.EarthCnt >= 20) VibrationalWave = true;
    }

    IEnumerator Loop()
    {
        if (GameManager.instance == null || GameManager.instance.pool == null) yield break;
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
            SpawnAndPrepare();
            yield return new WaitForSeconds(gap);
        }
    }

    // 기본 공격
    void SpawnAndPrepare()
    {
        var player = GameManager.instance.player;
        Vector2 fireDir = lastAimDir;
        if (fireDir.sqrMagnitude < 0.0001f)
            fireDir = player.IsFacingRight ? Vector2.right : Vector2.left;

        // 기본 공격은 각성 아님 (false)
        CreateBullet(fireDir, false); 
    }

    // 20레벨 대지의 송곳
    IEnumerator SpikeLoop()
    {
        while (true)
        {
            // [밸런스] 0.5초 대기 (서브딜 속도)
            yield return new WaitForSeconds(0.5f);

            if (VibrationalWave && scans != null && GameManager.instance.player != null)
            {
                // [밸런스] 한 번에 5명 타격 (넓게 뿌리기)
                Transform[] targets = scans.GetNearest(5);

                foreach (Transform target in targets)
                {
                    if (target == null || !target.gameObject.activeInHierarchy) continue;

                    Vector3 spawnPos = GetRandomSpawnPos(GameManager.instance.player.transform.position);
                    Vector3 targetDir = (target.position - spawnPos).normalized;

                    // true를 넘겨서 작고 갈색인 탄알 발사
                    CreateBullet(targetDir, true);
                }
            }
        }
    }

    void CreateBullet(Vector2 dir, bool isAwakening = false)
    {
        var player = GameManager.instance.player;
        Vector3 spawnPos = GetRandomSpawnPos(player.transform.position);

        GameObject go = GameManager.instance.pool.Get(prefabId);
        if (!go) return;

        Transform t = go.transform;
        if (GameManager.instance.pool != null) t.SetParent(GameManager.instance.pool.transform);
        else t.SetParent(null);

        t.position = spawnPos;

        // --- [수정] 서브딜 컨셉 조정 ---
        float currentScale = 1f;
        Color bulletColor = Color.white;
        int finalPer = per;

        if (isAwakening)
        {
            // 크기는 0.7배로 작게 (자잘한 공격 느낌)
            currentScale = 0.7f;

            // 색상은 붉은기 뺀 '진한 갈색' (RGB: 0.55, 0.4, 0.25)
            // 흙이나 암석 느낌이 나도록 조정했습니다.
            bulletColor = new Color(0.55f, 0.4f, 0.25f, 1f);

            // 작지만 관통력은 +1 유지 (선택사항, 필요 없으면 빼도 됨)
            finalPer += 1;
        }

        // 최종 크기 적용
        t.localScale = baseBulletScale * currentScale;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        t.rotation = Quaternion.Euler(0, 0, angle);

        Bullet b = go.GetComponent<Bullet>();
        if (b)
        {
            // 데미지는 서브딜이니까 기본 데미지 그대로 (배율 삭제)
            float finalDamage = damage;

            b.Init(StatsManager.instance.ApplyCrit(finalDamage), finalPer, dir, speed, lifeTime, stoneDustComp);
            b.SetColor(bulletColor);
        }
    }

    Vector3 GetRandomSpawnPos(Vector3 origin)
    {
        Vector2 randDir = Random.insideUnitCircle.normalized;
        float r = Mathf.Sqrt(Random.Range(minSpawnRadius * minSpawnRadius, maxSpawnRadius * maxSpawnRadius));
        return origin + (Vector3)(randDir * r);
    }
}
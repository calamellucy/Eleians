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
    public int shotsPerBurst = 15;
    public float burstDuration = 1.3f;
    public int per = 0;
    public Vector3 baseBulletScale = Vector3.one;

    [Header("Spawn Range")]
    public float minSpawnRadius = 0.3f;
    public float maxSpawnRadius = 1.0f;

    // [추가] 처음에 인스펙터에서 설정한 값을 기억할 변수
    private float originMaxRadius;

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

        // [추가] 게임 시작 시 설정된 기본 최대 반경을 저장해둡니다.
        originMaxRadius = maxSpawnRadius;
    }

    void OnEnable()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        if (spikeCo != null) StopCoroutine(spikeCo);

        loopCo = StartCoroutine(Loop());
        spikeCo = StartCoroutine(SpikeLoop());
    }

    void OnDisable()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        if (spikeCo != null) StopCoroutine(spikeCo);
    }

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
        // 1. 크기(Scale) 계산
        baseBulletScale = Vector3.one * (1f + StatsManager.instance.FireCnt * 0.04f) * 1.4f;

        // [추가/수정] 탄알 크기(x축 기준)에 비례해서 최대 스폰 반경을 넓혀줍니다.
        // 탄알이 커지면 더 멀리서 생성되어 겹침 방지 + 웅장함 연출
        maxSpawnRadius = originMaxRadius * baseBulletScale.x;

        // 2. 데미지 계산
        damage = StatsManager.instance.Attack * (1f + 0.04f * StatsManager.instance.IceCnt) * 0.5f;

        // 3. 공속(Interval) 계산
        float baseInterval = 1f / (StatsManager.instance.AttackSpeed * 0.3f);
        burstInterval = baseInterval * Mathf.Max(0.1f, (1f - StatsManager.instance.ElectricCnt * 0.05f));

        // 4. 발사 수 및 특수 효과 해금
        shotsPerBurst = 15 + StatsManager.instance.EarthCnt;
        if (StatsManager.instance.EarthCnt >= 5) { shotsPerBurst += 6; per = 4; }
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

    void SpawnAndPrepare()
    {
        var player = GameManager.instance.player;
        Vector2 fireDir = lastAimDir;
        if (fireDir.sqrMagnitude < 0.0001f)
            fireDir = player.IsFacingRight ? Vector2.right : Vector2.left;

        CreateBullet(fireDir, false);

        if (StatsManager.instance.EarthCnt >= 5)
        {
            Vector2 reverseDir = -fireDir;
            CreateBullet(reverseDir, false);
        }
    }

    IEnumerator SpikeLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (VibrationalWave && scans != null && GameManager.instance.player != null)
            {
                Transform[] targets = scans.GetNearest(5);

                foreach (Transform target in targets)
                {
                    if (target == null || !target.gameObject.activeInHierarchy) continue;

                    Vector3 spawnPos = GetRandomSpawnPos(GameManager.instance.player.transform.position);
                    Vector3 targetDir = (target.position - spawnPos).normalized;

                    CreateBullet(targetDir, true);
                }
            }
        }
    }

    void CreateBullet(Vector2 dir, bool isAwakening = false)
    {
        var player = GameManager.instance.player;

        // 여기서 maxSpawnRadius가 늘어난 상태로 계산됩니다.
        Vector3 spawnPos = GetRandomSpawnPos(player.transform.position);

        GameObject go = GameManager.instance.pool.Get(prefabId);
        if (!go) return;

        Transform t = go.transform;
        if (GameManager.instance.pool != null) t.SetParent(GameManager.instance.pool.transform);
        else t.SetParent(null);

        t.position = spawnPos;

        float currentScale = 1f;
        Color bulletColor = Color.white;
        int finalPer = per;

        if (isAwakening)
        {
            currentScale = 0.7f;
            bulletColor = new Color(0.55f, 0.4f, 0.25f, 1f);
            finalPer += 1;
        }

        t.localScale = baseBulletScale * currentScale;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        t.rotation = Quaternion.Euler(0, 0, angle);

        Bullet b = go.GetComponent<Bullet>();
        if (b)
        {
            float finalDamage = damage;
            b.Init(StatsManager.instance.ApplyCrit(finalDamage), finalPer, dir, speed, lifeTime, stoneDustComp);
            b.SetColor(bulletColor);
        }
    }

    Vector3 GetRandomSpawnPos(Vector3 origin)
    {
        Vector2 randDir = Random.insideUnitCircle.normalized;

        // [참고] maxSpawnRadius는 GiveLevelSystemToSkill4에서 이미 크기에 맞춰 늘어나 있음
        float r = Mathf.Sqrt(Random.Range(minSpawnRadius * minSpawnRadius, maxSpawnRadius * maxSpawnRadius));
        return origin + (Vector3)(randDir * r);
    }
}
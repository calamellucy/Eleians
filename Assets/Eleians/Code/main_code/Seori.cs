using System.Collections;
using UnityEngine;

public class Seori : MonoBehaviour
{
    public int id;
    public int prefabId = 10;
    public float damage;
    public int count = 1;
    public float speed;
    public float range;
    public float slowRate = 0.4f;

    // 원소 수
    public int electricCount = 0;
    public int fireCount = 0;
    public int iceCount = 0;
    public int earthCount = 0;

    // 이전 값 저장 → 변경 감지용
    int prevElectric, prevFire, prevIce, prevEarth;

    // 둔화 영역
    public GameObject dhwyyPrefab;
    private GameObject dhwyyInstance;
    public float dhwyyBaseScale = 1.0f;

    // 얼음 영역 (풀 프리팹 id)
    // public GameObject iceZonePrefab; // 풀 쓰면 사실 필요 없음
    public int iceZonePrefabId = 14;

    // 15진화 즉시 발동 플래그
    float iceZoneCooldown = 20f;
    float iceZoneTimer = 0f;
    bool iceZoneFirstTriggered = false;

    Transform player;

    void Start()
    {
        player = GameManager.instance.player.transform;

        SyncWithStats();
        Init();
    }

    void Update()
    {
        // 회전 중심 = 플레이어
        transform.position = player.position;
        transform.Rotate(Vector3.back * speed * Time.deltaTime);

        // 원소 변화 감지
        if (StatsChanged())
        {
            SyncWithStats();
            Init();

            // 10개 달성 순간 → 즉시 1회 발동
            if (iceCount >= 10 && !iceZoneFirstTriggered)
            {
                ActivateIceZoneRandom();
                iceZoneTimer = 0f;               // 쿨타임 초기화
                iceZoneFirstTriggered = true;
            }
        }

        // 10개 이상 유지 → 쿨타임 발동
        if (iceZoneFirstTriggered && iceCount >= 10)
        {
            iceZoneTimer += Time.deltaTime;

            if (iceZoneTimer >= iceZoneCooldown)
            {
                ActivateIceZoneRandom();
                iceZoneTimer = 0f;
            }
        }
    }

    // 원소 변화 감지
    bool StatsChanged()
    {
        return
            prevElectric != StatsManager.instance.ElectricCnt ||
            prevFire != StatsManager.instance.FireCnt ||
            prevIce != StatsManager.instance.IceCnt ||
            prevEarth != StatsManager.instance.EarthCnt;
    }

    // 스탯 동기화
    void SyncWithStats()
    {
        electricCount = StatsManager.instance.ElectricCnt;
        fireCount = StatsManager.instance.FireCnt;
        iceCount = StatsManager.instance.IceCnt;
        earthCount = StatsManager.instance.EarthCnt;

        prevElectric = electricCount;
        prevFire = fireCount;
        prevIce = iceCount;
        prevEarth = earthCount;
    }

    // 스킬 재등록
    public void Init()
    {
        float atk = StatsManager.instance.Attack;

        // 기존 표창 비활성화
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Seori_Shuri>() != null)
                child.gameObject.SetActive(false);
        }

        // 진화 옵션 계산
        speed = 240f;
        damage = (atk * 0.4f) * Mathf.Pow(1.04f, earthCount);
        count = 1 + (int)(iceCount * 0.2f);
        range = 1 + fireCount * 0.05f;
        slowRate = 0.4f + (electricCount * 0.04f);

        if (iceCount >= 5)
        {
            speed *= 1.2f;
            count += 2;
            range += 0.75f;
        }

        // 얼음 20 진화
        if (iceCount >= 20)
            ActivateDhwyy();
        else if (dhwyyInstance != null)
            dhwyyInstance.SetActive(false);

        // 표창 재배치
        Batch();
    }

    // 표창 생성
    void Batch()
    {
        for (int i = 0; i < count; i++)
        {
            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;

            bullet.gameObject.SetActive(true);
            bullet.SetParent(transform);

            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            // 각도 배치
            Vector3 rot = Vector3.forward * (360f * i / count);
            bullet.Rotate(rot);

            // 바깥으로 보내기
            bullet.Translate(bullet.up * range, Space.World);

            // 데이터 전달
            bullet.GetComponent<Seori_Shuri>().Init(damage, -1, slowRate);
        }
    }

    // ============================
    // ★ 10진화: 즉시 발동 얼음장판 (풀 사용, 랜덤 위치)
    void ActivateIceZoneRandom()
    {
        if (player == null)
            return;

        Vector3 basePos = player.position;
        Vector2 offset = Random.insideUnitCircle * 3f;   // 반지름 3 안에서 랜덤
        Vector3 spawnPos = basePos + (Vector3)offset;
        spawnPos.z = 0f;

        GameObject zone = GameManager.instance.pool.Get(iceZonePrefabId);
        zone.transform.position = spawnPos;
        zone.transform.rotation = Quaternion.identity;
        zone.SetActive(true);
        Debug.Log("IceZone 생성!");
    }

    // 20진화: dhwyy 생성 / 유지
    void ActivateDhwyy()
    {
        if (dhwyyInstance == null)
        {
            dhwyyInstance = Instantiate(dhwyyPrefab, player);
            dhwyyInstance.transform.localPosition = Vector3.zero;
        }

        float sizeMultiplier = 1f + (iceCount * 0.025f);
        dhwyyInstance.transform.localScale = Vector3.one * sizeMultiplier * dhwyyBaseScale;

        dhwyyInstance.SetActive(true);
    }
}

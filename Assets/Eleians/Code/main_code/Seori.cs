using System.Collections;
using UnityEngine;

public class Seori : MonoBehaviour
{
    public int id;
    public int prefabId = 5;
    public float damage;
    public int count = 1;
    public float speed;
    public float range;
    public float slowRate = 0.3f;

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


    void Start()
    {
        SyncWithStats();    // 처음 원소값 동기화
        Init();             // 표창 생성
    }

    void Update()
    {
        // 표창 회전
        transform.Rotate(Vector3.back * speed * Time.deltaTime);

        // 원소 변경 감지 → 스킬 재등록
        if (StatsChanged())
        {
            SyncWithStats();   // 먼저 업데이트
            Init();            // 그 다음 표창 재생성
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


    // 🔥 스킬 재등록
    public void Init()
    {
        // ============================
        // 1) 표창만 끄기 (dhwyy 영향 X)
        // ============================
        foreach (Transform child in transform)
        {
            if (child.GetComponent<Seori_Shuri>() != null)
                child.gameObject.SetActive(false);
        }

        // ============================
        // 2) 각종 진화 계산
        // ============================
        speed = 270f;
        damage = (fireCount * 1.5f) * earthCount * 1.08f;
        count = 1 + (int)(iceCount * 0.25f);
        range = 1 + iceCount * 0.025f;
        slowRate = 0.3f + (iceCount * 0.04f);

        if (iceCount >= 5)
        {
            speed *= 1.35f;
            slowRate += 0.15f;
        }

        // ============================
        // 3) 얼음 20 진화 → dhwyy 생성
        // ============================
        if (iceCount >= 20)
            ActivateDhwyy();
        else if (dhwyyInstance != null)
            dhwyyInstance.SetActive(false);

        // ============================
        // 4) 표창 재배치
        // ============================
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

            // 바깥으로 내보내기
            bullet.Translate(bullet.up * range, Space.World);

            // 표창 데이터 전달
            bullet.GetComponent<Seori_Shuri>().Init(damage, -1, slowRate);
        }
    }


    // 🔥 dhwyy 생성 / 유지
    void ActivateDhwyy()
    {
        Transform player = GameManager.instance.player.transform;

        if (dhwyyInstance == null)
        {
            dhwyyInstance = Instantiate(dhwyyPrefab, player);
            dhwyyInstance.transform.localPosition = Vector3.zero;
        }

        // 🔥 크기 조절 (전체 스케일)
        float size = 1f + (iceCount * 0.025f);  // 예: 얼음 1개당 10% 증가
        dhwyyInstance.transform.localScale = Vector3.one * size;

        dhwyyInstance.SetActive(true);
    }

}

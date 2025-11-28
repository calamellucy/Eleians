using UnityEngine;

public class BlizzardArea : MonoBehaviour
{
    [Header("Base Settings")]
    public float tickInterval = 1f;     // 1초마다 적용
    public float baseDamage = 5f;       // 기본 눈보라 데미지
    public float slowRate = 0.3f;       // 기본 둔화율(30%)
    public int electricCount = 0;       // ���� ���� ���� (0~20)
    public int fireCount = 0;
    public int iceCount = 0;
    public int earthCount = 0;

    float timer = 0f;

    int prevElectric, prevFire, prevIce, prevEarth;


    private void OnEnable()
    {
        // 활성화될 때마다 타이머 초기화
        timer = 0f;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        timer += Time.deltaTime;

        if (timer < tickInterval)
            return;

        // 1초 지남 → 효과 적용
        timer = 0f;

        if (collision.CompareTag("Monster"))
        {
            MonsterBase m = collision.GetComponent<MonsterBase>();
            if (m != null)
            {
                // StatsManager의 공격력 적용 + 크리티컬 적용
                float damage = StatsManager.instance.ApplyCrit(
                    StatsManager.instance.Attack * 0.25f   // 공격력 25% 비율 데미지 예시
                );

                // iceCount에 비례해 눈보라 강화 (원하면 조정 가능)
                damage += baseDamage + (StatsManager.instance.IceCnt * 0.4f);

                m.ApplyDamage(damage);
                m.ApplySlow(slowRate);
            }
        }
    }

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
}

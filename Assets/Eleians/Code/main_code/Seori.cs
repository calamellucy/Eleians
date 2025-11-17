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
    public int electricCount = 0;       // 전기 원소 개수 (0~20)
    public int fireCount = 0;
    public int iceCount = 0;
    public int earthCount = 0;


    void Start()
    {
        Init();
    }
    private void Update()
    {
        transform.Rotate(Vector3.back * speed * Time.deltaTime);
        SyncWithStats();
    }

    public void Init()
    {
        speed = 270f;
        damage = (fireCount * 1.5f) * earthCount * 1.08f;
        count = 1 + (int)(iceCount * 0.25f) / 1;
        range = 1 + iceCount * 0.25f;
        slowRate = 0.3f + (iceCount * 0.04f);



        if (iceCount >= 5)
        {
            speed *= 1.35f;
            slowRate += 0.15f;
        }
        Batch();

    }

    void Batch()
    {
        for (int i = 0; i < count; i++)
        {
            Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
            bullet.gameObject.SetActive(true);   // ← 이것이 없어서 안 보임!

            bullet.parent = transform;

            Vector3 rotVec = Vector3.forward * 360 * i / count;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.0f, Space.World);
            bullet.GetComponent<Seori_Shuri>().Init(damage, -1, slowRate);
        }
    }

    void SyncWithStats()
    {
        electricCount = StatsManager.instance.ElectricCnt;
        fireCount = StatsManager.instance.FireCnt;
        iceCount = StatsManager.instance.IceCnt;
        earthCount = StatsManager.instance.EarthCnt;
        Init();
    }
}

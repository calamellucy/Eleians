using UnityEngine;
using System.Collections;

public class Skill1_Re : MonoBehaviour
{
    public int prefabId = 2;           // 프리펩 아이디: 2
    public float damage;               // 기본 대미지
    public int count;                  // 관통 횟수
    public float attackRate;           // 공격 속도 (초당 발사 횟수)
    public float projectileSize;       // 투사체 크기
    public int projectileCount;        // 한 번에 발사하는 투사체 수

    [Header("Evolution State")]
    public int electricCount = 0;       // 전기 원소 개수 (0~20)

    float timer;
    Player player;

    void Awake()
    {
        player = GetComponentInParent<Player>();
        if (player == null)
            Debug.LogError("Skill1_Re Player regerence is NULL"); // 오류 검사
    }

    void Start()
    {
       // Init();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 1f / attackRate)
        {
            timer = 0f;
            TryFire();
            Debug.Log("Update"); // 오류 검사
        }
        Init();
    }

    public void Init()
    {
        prefabId = 2;
        attackRate = 1f + 0.08f * electricCount;        
        damage = 10f + 0.6f * electricCount/* ( * 불 원소 개수) */;             
        count = 0 /* ( + (불원소 개수 * 0.25)/1)*/;      
        projectileSize = 0.2f /* ( * 흙 원소 개수 * 0.08f) */;   
        projectileCount = 1 /* ( + (얼음 원소 개수 * 0.33f)/1) */;

        // attakRate: 초당 발사 횟수 + 전기 개수에 비례한 공격 속도 증가
        // damage: 기본 대미지 + 전기 개수에 비례한 대미지 증가 + 불 개수에 비례한 대미지 증가
        // count: 기본 관통 횟수 + 불 개수에 비례한 관통 횟수 증가
        // projectileSize: 기본 크기 + 흙 개수에 비례한 투사체 크기 증가
        // projectileCount: 기본 투사체 수 + 얼음 개수에 비례한 투사체 수 증가

        // 각 속성 옆의 주석은 진화 상태에 따른 변화 예시입니다.

        /* !! 투사체 속도는 Bullet_Re 스크립트에서 처리 !! */

        if (electricCount >= 5)
        {
            projectileCount += 2;
            count += 1;
        }
    }

    //public void RefreshStats() => Init();

    void TryFire()
    {
        if (!player.scanner.nearestTarget)
            return;

        StartCoroutine(FireBurst());
    }

    IEnumerator FireBurst()
    {
        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = (targetPos - transform.position).normalized;

        int projectile_Count = projectileCount;
        int per = count;
        

        for (int i = 0; i < projectile_Count; i++)
        {
            FireSingle(dir, per);

            
            yield return new WaitForSeconds(0.08f);
        }
    }

    void FireSingle(Vector3 dir, int per)
    {
        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        bullet.localScale = Vector3.one * projectileSize;

        BulletEvolution evo = bullet.GetComponent<BulletEvolution>();
        if (evo != null)
            evo.Setup(this);

        bullet.GetComponent<Bullet_Re>().Init(damage, per, dir, electricCount);
        Debug.Log("Fire");
    }


}

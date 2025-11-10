using System.Collections;
using UnityEngine;

public class BulletEvolution : MonoBehaviour
{
    Skill1 skill;
    BulletHoming bullet;
    bool hasTriggered = false; // ⚡ 한 번만 실행하도록 플래그 추가

    public void Setup(Skill1 skillRef)
    {
        skill = skillRef;
        bullet = GetComponent<BulletHoming>();
        hasTriggered = false; // 새 총알 생성 시 초기화
    }

    // Bullet이 비활성화될 때 (관통력 다 소모 시)
    void OnDisable()
    {
        if (hasTriggered || skill == null || bullet == null)
            return;

        hasTriggered = true; // 다시 호출되지 않게 잠금

        int electric = skill.electricCount;

        // ⚡ 10개 이상 → 폭발 생성
        if (electric >= 10)
        {
            CreateExplosion();
        }

        // ⚡ 20개 이상 → 분열탄 발사
        if (electric >= 20)
        {
            SpawnSplitBullets();
        }
    }

    void CreateExplosion()
    {
        // explosion은 PoolManager에서 Element 4에 있음!
        GameObject explosion = GameManager.instance.pool.Get(4); // ✅ 1 → 4로 수정
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;
        explosion.transform.localScale = Vector3.one; // 안전하게 초기화

        Explosion exp = explosion.GetComponent<Explosion>();
        if (exp != null)
        {
            exp.damage = skill.damage * 0.25f;
            exp.radius = 1.5f;
            exp.duration = 0.3f;
        }

        explosion.SetActive(true);
    }



    void SpawnSplitBullets()
    {
        for (int i = 0; i < 3; i++)
        {
            float randAngle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(randAngle * Mathf.Deg2Rad), Mathf.Sin(randAngle * Mathf.Deg2Rad));

            GameObject split = GameManager.instance.pool.Get(skill.prefabId);
            split.transform.position = transform.position;
            split.transform.rotation = Quaternion.AngleAxis(randAngle, Vector3.forward);

            BulletHoming b = split.GetComponent<BulletHoming>();
            if (b != null)
            {
                b.damage = skill.damage * 0.5f;
                b.per = Mathf.Max(0, skill.per / 2);
                b.speed = skill.speed;
                b.SetTarget(null); // 분열탄은 직진
            }

            skill.StartCoroutine(DisableAfter(split, skill.lifetime * 0.5f));
        }
    }

    IEnumerator DisableAfter(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go != null)
            go.SetActive(false);
    }
}

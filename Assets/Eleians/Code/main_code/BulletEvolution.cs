using System.Collections;
using UnityEngine;

public class BulletEvolution : MonoBehaviour
{
    Skill1_Re skill;
    Bullet_Re bullet;
    bool hasTriggered = false;

    public void Setup(Skill1_Re skillRef)
    {
        skill = skillRef;
        bullet = GetComponent<Bullet_Re>();
        hasTriggered = false;
    }

    void OnEnable()
    {
        hasTriggered = false;
    }

    // Bullet_Re에서 적에게 맞았을 때 호출
    public void TriggerEvolution()
    {
        if (hasTriggered || skill == null)
            return;

        hasTriggered = true;
        int electric = skill.electricCount;

        // 10개 이상 → 폭발
        if (electric >= 10)
            CreateExplosion();

        // 20개 이상 → 분열 (관통을 모두 소비한 경우)
        if (electric >= 20)
            SpawnSplitBullets();
    }

    void CreateExplosion()
    {
        GameObject explosion = GameManager.instance.pool.Get(4);
        explosion.transform.position = transform.position;
        explosion.transform.rotation = Quaternion.identity;
        explosion.transform.localScale = Vector3.one;

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
        // 20전기: 3발 분열, 대미지/크기/관통 절반, 재분열 불가
        for (int i = 0; i < 3; i++)
        {
            float randAngle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(randAngle * Mathf.Deg2Rad), Mathf.Sin(randAngle * Mathf.Deg2Rad));

            GameObject split = GameManager.instance.pool.Get(skill.prefabId);
            split.transform.position = transform.position;
            split.transform.rotation = Quaternion.AngleAxis(randAngle, Vector3.forward);
            split.transform.localScale = Vector3.one * (skill.projectileSize * 0.7f);
            split.SetActive(true);

            Bullet_Re b = split.GetComponent<Bullet_Re>();
            if (b != null)
            {
                b.damage = skill.damage * 0.5f;
                b.per = Mathf.Max(0, skill.count / 2);
                b.GetComponent<BulletEvolution>().hasTriggered = true; // 재분열 방지
                b.Init(b.damage, b.per, dir, b.elecCount);
            }

            skill.StartCoroutine(DisableAfter(split, 1.5f));
        }
    }

    IEnumerator DisableAfter(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go != null && go.activeSelf)
            go.SetActive(false);
    }
}

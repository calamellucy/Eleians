using System.Collections;
using UnityEngine;

public class BulletEvolution : MonoBehaviour
{
    Skill1 skill;
    BulletHoming bullet;
    bool hasTriggered = false;

    public void Setup(Skill1 skillRef)
    {
        skill = skillRef;
        bullet = GetComponent<BulletHoming>();
        hasTriggered = false;
    }

    void OnEnable()
    {
        hasTriggered = false;
    }

    // ⚡ BulletHoming이 직접 호출하는 진화 트리거
    public void TriggerEvolution()
    {
        if (hasTriggered || skill == null || bullet == null)
            return;

        hasTriggered = true;

        int electric = skill.electricCount;

        // 10개 이상 → 폭발
        if (electric >= 10)
            CreateExplosion();

        // 20개 이상 → 분열
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
        for (int i = 0; i < 3; i++)
        {
            float randAngle = Random.Range(0f, 360f);
            Vector2 dir = new Vector2(Mathf.Cos(randAngle * Mathf.Deg2Rad), Mathf.Sin(randAngle * Mathf.Deg2Rad));

            GameObject split = GameManager.instance.pool.Get(skill.prefabId);
            split.transform.position = transform.position;
            split.transform.rotation = Quaternion.AngleAxis(randAngle, Vector3.forward);
            split.SetActive(true);

            BulletHoming b = split.GetComponent<BulletHoming>();
            if (b != null)
            {
                b.damage = skill.damage * 0.5f;
                b.per = Mathf.Max(0, skill.per / 2);
                b.speed = skill.speed;
                b.SetTarget(null);
            }

            skill.StartCoroutine(DisableAfter(split, skill.lifetime * 0.5f));
        }
    }

    IEnumerator DisableAfter(GameObject go, float t)
    {
        yield return new WaitForSeconds(t);
        if (go != null && go.activeSelf)
            go.SetActive(false);
    }
}

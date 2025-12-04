using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceZone : MonoBehaviour
{
    [Header("Duration")]
    public float duration = 5f;

    [Header("Heal Settings")]
    public float healTickInterval = 1f;
    public float healBase = 10f;
    public float healMissingHpRatio = 0.10f;

    [Header("Damage Settings")]
    public float damageInterval = 0.2f;
    public float damageBase = 10f;
    public float damageMaxHpRatio = 0.04f;

    // 플레이어 캐시
    private Player playerCache;
    private bool playerInside = false;
    private float healTimer = 0f;

    // 몬스터 리스트
    private List<MonsterBase> monsterList = new List<MonsterBase>();
    private float damageTimer = 0f;

    void OnEnable()
    {
        // 타이머 초기화
        healTimer = healTickInterval;
        damageTimer = damageInterval;

        // 상태 초기화
        playerInside = false;
        playerCache = null;

        monsterList.Clear();

        // 수명 코루틴 실행
        StartCoroutine(ZoneLife());
    }

    IEnumerator ZoneLife()
    {
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false); // 풀로 복귀
    }

    void Update()
    {
        // =========================
        // 1) 플레이어 힐 처리
        // =========================
        if (playerInside && playerCache != null)
        {
            healTimer -= Time.deltaTime;
            if (healTimer <= 0f)
            {
                float maxHp = GameManager.instance.maxHealth;
                float curHp = GameManager.instance.health;

                float missingHp = maxHp - curHp;
                float healAmount = healBase + (missingHp * healMissingHpRatio);

                playerCache.Heal(healAmount);
                Debug.Log($"ICEZONE HEAL: {healAmount}");

                healTimer = healTickInterval;
            }
        }

        // =========================
        // 2) 몬스터 피해 처리
        // =========================
        if (monsterList.Count > 0)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                for (int i = monsterList.Count - 1; i >= 0; i--)
                {
                    MonsterBase monster = monsterList[i];

                    // 죽었거나 비활성화된 몬스터 정리
                    if (monster == null || !monster.gameObject.activeSelf || !monster.isLive)
                    {
                        monsterList.RemoveAt(i);
                        continue;
                    }

                    float monsterMaxHp = monster.maxHealth;
                    float dmg = damageBase + (monsterMaxHp * damageMaxHpRatio);

                    monster.ApplyDamageWithoutKonckback(dmg);
                    // 필요하면 여기서도 로그 출력 가능
                    // Debug.Log($"ICEZONE DAMAGE: {dmg}");
                }

                damageTimer = damageInterval;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
            playerCache = collision.GetComponent<Player>();
        }

        if (collision.CompareTag("Enemy"))
        {
            MonsterBase monster = collision.GetComponent<MonsterBase>();
            if (monster != null && !monsterList.Contains(monster))
            {
                monsterList.Add(monster);
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
            playerCache = null;
        }

        if (collision.CompareTag("Enemy"))
        {
            MonsterBase monster = collision.GetComponent<MonsterBase>();
            if (monster != null && monsterList.Contains(monster))
            {
                monsterList.Remove(monster);
            }
        }
    }
}

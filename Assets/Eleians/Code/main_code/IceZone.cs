using UnityEngine;
using System.Collections;

public class IceZone : MonoBehaviour
{
    float duration = 5f;
    bool isRunning = false;

    bool playerInside = false;
    bool monsterInside = false;

    float healTimer = 1f;
    float damageTimer = 1f;

    Player playerCache;
    MonsterBase monsterCache;

    void OnEnable()
    {
        healTimer = 1f;
        damageTimer = 1f;

        playerInside = false;
        monsterInside = false;

        if (!isRunning)
            StartCoroutine(ZoneLife());
    }

    IEnumerator ZoneLife()
    {
        isRunning = true;
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
        isRunning = false;
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
                float healAmount = 10f + (missingHp * 0.10f);

                playerCache.Heal(healAmount);
                Debug.Log($"HEAL: {healAmount}");

                healTimer = 1f;
            }
        }

        // =========================
        // 2) 몬스터 피해 처리
        // =========================
        if (monsterInside && monsterCache != null && monsterCache.isLive)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                float monsterMaxHp = monsterCache.maxHealth;
                float dmg = 10f + (monsterMaxHp * 0.04f);

                monsterCache.ApplyDamageWithoutKonckback(dmg);
                Debug.Log($"DAMAGE: {dmg}");

                damageTimer = 1f;
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
            monsterInside = true;
            monsterCache = collision.GetComponent<MonsterBase>();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
            playerInside = false;

        if (collision.CompareTag("Enemy"))
            monsterInside = false;
    }
}

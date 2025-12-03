using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceZone : MonoBehaviour
{
    public float damagePerSecond = 5f;
    public float healPerSecond = 10f;
    float duration = 5f;

    // bool isRunning = false;
    //bool playerInside = false;
    //bool monsterInside = false;

    // 플레이어는 1명이니 변수 하나로 충분
    private Player playerCache;
    private bool playerInside = false;
    private float healTimer = 1f;

    //float healTimer = 1f;
    // ★ 몬스터는 여러 마리일 수 있으니 리스트로 관리
    private List<MonsterBase> monsterList = new List<MonsterBase>();
    float damageTimer = 0f;
    public float damageInterval = 0.2f;

    // Player playerCache;
    // MonsterBase monsterCache;

    void OnEnable()
    {
        healTimer = 1f;
        damageTimer = 0f;

        playerInside = false;
        //monsterInside = false;
        monsterList.Clear(); // 켜질 때 리스트 초기화

        //if (!isRunning)
        StartCoroutine(ZoneLife());
    }

    IEnumerator ZoneLife()
    {
        //isRunning = true;
        yield return new WaitForSeconds(duration);
        gameObject.SetActive(false);
        //isRunning = false;
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
        if (monsterList.Count > 0)
        {
            /*
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                float monsterMaxHp = monsterCache.maxHealth;
                float dmg = 10f + (monsterMaxHp * 0.04f);

                monsterCache.ApplyDamageWithoutKonckback(dmg);
                Debug.Log($"DAMAGE: {dmg}");

                damageTimer = 1f;
            }
            */
            // 리스트를 돌면서 모든 몬스터에게 데미지
            // (역순으로 도는 이유: 중간에 죽어서 리스트에서 빠질 경우 오류 방지)
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                for (int i = monsterList.Count - 1; i >= 0; i--)
                {
                    MonsterBase monster = monsterList[i];

                    if (monster == null || !monster.gameObject.activeSelf || !monster.isLive)
                    {
                        monsterList.RemoveAt(i);
                        continue;
                    }

                    float monsterMaxHp = monster.maxHealth;
                    float dmg = 10f + (monsterMaxHp * 0.04f);

                    monster.ApplyDamageWithoutKonckback(dmg);
                }

                damageTimer = damageInterval;
            }


            // Debug.Log(monsterList.Count + "마리 몬스터 피격!");
            
        }
    }

    // Player playerCache;
    // MonsterBase monsterCache;

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
            // 리스트에 없을 때만 추가 (중복 방지)
            if (monster != null && !monsterList.Contains(monster))
            {
                monsterList.Add(monster);
            }
            //monsterInside = true;
            //monsterCache = collision.GetComponent<MonsterBase>();
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
            // 나간 몬스터만 리스트에서 제거
            if (monster != null && monsterList.Contains(monster))
            {
                monsterList.Remove(monster);
            }
            //monsterInside = false;
        }

    }
}

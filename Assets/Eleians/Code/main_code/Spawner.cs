using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Spawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public Transform[] spawnPoint;
    public SpawnData[] spawnData;
    [Header("Battle Area (Manual)")]
    [Tooltip("전투 가능한 내부 구역의 좌측하단 (x, y)")]
    public Vector2 innerMin = new Vector2(-7.5f, -4.5f);
    [Tooltip("전투 가능한 내부 구역의 우측상단 (x, y)")]
    public Vector2 innerMax = new Vector2(7.5f, 4.5f);

    float timer;

    int level;

    float spawnTime;

    private void Awake()
    {
        spawnPoint = GetComponentsInChildren<Transform>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / 10f), spawnData.Length - 1);
        
        if (timer > spawnData[level].spawnTime)
        {
            timer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        // 스폰 가능한 스폰 포인트 계산

        List<Transform> validPoints = new List<Transform>();

        foreach (Transform point in spawnPoint)
        {
            if (point == transform) continue;

            // 벽 내부에 있는 포인트만 허용
            if (IsInsideBattleArea(point.position))
            {
                validPoints.Add(point);
            }
        }

        if (validPoints.Count == 0)
        {
            Debug.Log("스폰 가능한 포인트 없음 (모두 벽 밖)");
            return;
        }

        Transform spawnPos = validPoints[Random.Range(0, validPoints.Count)];


        // 페이즈 별 몬스터 스폰 결정
        // 현재 페이즈 구분 (거점 페이즈 여부)
        bool isTowerPhase = GameManager.instance.isTowerPhase;
        // 몬스터 종류 비율 결정
        float rand = Random.value;

        MonsterType selectedType;
        if (!isTowerPhase) // 일반 페이즈
        {
            selectedType = MonsterType.Normal; 
        }
        else // 거점 페이즈일 때 (예: 일반 0.6, 거점 0.4)
        {
            if (rand < 0.6f)
                selectedType = MonsterType.Normal;
            else 
                selectedType = MonsterType.Tower;
            // else
                // selectedType = MonsterType.Elite;
        }

        // Spawn Data 선택
        SpawnData data = GetSpawnData(selectedType);
        int spriteIndex = Random.Range(0, data.spriteCount);

        // 프리펩 선택
        int prefabIndex = GetPoolIndexByType(selectedType);
        GameObject enemy = GameManager.instance.pool.Get(prefabIndex);
        enemy.transform.position = spawnPos.position;

        // 몬스터 설정
        var monster = enemy.GetComponent<NormalMonster>();
        monster.Init(data, spriteIndex); // spriteIndex 함께 전달

        /*
        GameObject enemy = GameManager.instance.pool.Get(0);
        // enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        enemy.transform.position = spawnPos.position;
        // enemy.GetComponent<NormalMonster>().Init(spawnData[level]);
        enemy.GetComponent<NormalMonster>().Init(spawnData[Random.Range(1, spawnData.Length)]);
        */
    }

    // 수동 경계 체크 함수
    bool IsInsideBattleArea(Vector3 pos)
    {
        return (pos.x >= innerMin.x && pos.x <= innerMax.x &&
                pos.y >= innerMin.y && pos.y <= innerMax.y);
    }

    SpawnData GetSpawnData(MonsterType type)
    {
        foreach (var data in spawnData)
        {
            if (data.monsterType == type)
                return data;
        }
        // 기본값 (안전장치)
        return spawnData[0];
    }

    int GetPoolIndexByType(MonsterType type)
    {
        switch (type)
        {
            case MonsterType.Normal: return 0;
            case MonsterType.Tower: return 3;
            case MonsterType.Elite: return 0;
            default: return 0;
        }
    }
}
public enum MonsterType
{
    Normal,
    Tower,
    Elite
}

[System.Serializable]
public class SpawnData
{
    public MonsterType monsterType;
    // public int spriteType;
    public int spriteCount;
    public float spawnTime;
    public int health;
    public float speed;
}
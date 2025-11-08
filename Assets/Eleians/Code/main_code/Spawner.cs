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


        GameObject enemy = GameManager.instance.pool.Get(0);
        enemy.transform.position = spawnPoint[Random.Range(1, spawnPoint.Length)].position;
        enemy.GetComponent<NormalMonster>().Init(spawnData[level]);
    }

    // 수동 경계 체크 함수
    bool IsInsideBattleArea(Vector3 pos)
    {
        return (pos.x >= innerMin.x && pos.x <= innerMax.x &&
                pos.y >= innerMin.y && pos.y <= innerMax.y);
    }
}

[System.Serializable]
public class SpawnData
{
    public int spriteType;
    public float spawnTime;
    public int health;
    public float speed;
}
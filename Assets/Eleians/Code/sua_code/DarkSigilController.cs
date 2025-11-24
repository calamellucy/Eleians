using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarkSigilController : MonoBehaviour
{
    [Header("Arena Bounds (제한된 필드 범위)")]
    public Vector2 arenaMin;   // 왼쪽-아래
    public Vector2 arenaMax;   // 오른쪽-위

    [Header("Grid Settings")]
    public int cols = 4;       // 가로 칸 수 (4 x 2 = 8칸)
    public int rows = 2;       // 세로 칸 수

    [Header("VFX Prefabs")]
    public GameObject warningPrefab;   // 경고 표시 이펙트 (보라색 원 or 사각형)
    public GameObject explosionPrefab; // 폭발 이펙트

    [Header("Pattern Timing")]
    public float warningDuration = 0.4f;   // 경고 표시 시간
    public float explodeInterval = 0.5f;   // 타일 간 폭발 간격
    public float damagePercent = 0.4f;     // 맞을 때 플레이어 HP의 몇 % 깎을지 (TODO)

    Coroutine patternRoutine;

    // 내부용: 각 타일의 중심 위치와 경고 이펙트 참조
    List<Vector3> tileCenters = new List<Vector3>();
    List<GameObject> warningObjects = new List<GameObject>();

    void Awake()
    {
        PrecomputeTiles();
    }

    void PrecomputeTiles()
    {
        tileCenters.Clear();

        float width = arenaMax.x - arenaMin.x;
        float height = arenaMax.y - arenaMin.y;

        float tileW = width / cols;
        float tileH = height / rows;

        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < cols; c++)
            {
                float x = arenaMin.x + tileW * (c + 0.5f);
                float y = arenaMin.y + tileH * (r + 0.5f);
                tileCenters.Add(new Vector3(x, y, 0f));
            }
        }
    }

    public void StartSigilPattern()
    {
        if (patternRoutine != null)
            StopCoroutine(patternRoutine);
        patternRoutine = StartCoroutine(SigilRoutine());
    }

    public void StopSigilPattern()
    {
        if (patternRoutine != null)
        {
            StopCoroutine(patternRoutine);
            patternRoutine = null;
        }

        ClearWarnings();
        // 폭발 이펙트는 각자 AutoDestroy 처리해도 되고,
        // 여기서 따로 관리해도 됨.
    }

    IEnumerator SigilRoutine()
    {
        if (tileCenters.Count == 0)
            PrecomputeTiles();

        int totalTiles = tileCenters.Count;
        if (totalTiles == 0) yield break;

        // 8칸 기준: 절반은 4칸
        int halfCount = totalTiles / 2;

        // 인덱스 리스트 생성
        List<int> indices = new List<int>();
        for (int i = 0; i < totalTiles; i++)
            indices.Add(i);

        // 1차 폭발용으로 랜덤하게 halfCount개 선택
        Shuffle(indices);
        List<int> firstExplosionTiles = indices.GetRange(0, halfCount);
        List<int> secondExplosionTiles = indices.GetRange(halfCount, totalTiles - halfCount);

        // 1차: 선택된 4칸 경고 → 순차 폭발
        yield return FirstOrSecondWave(firstExplosionTiles);

        // 2차: 남은 4칸 경고 → 순차 폭발
        yield return FirstOrSecondWave(secondExplosionTiles);
    }

    IEnumerator FirstOrSecondWave(List<int> tileList)
    {
        // 경고 이펙트 뿌리기
        ShowWarnings(tileList);

        // 0.4초 경고 유지
        yield return new WaitForSeconds(warningDuration);

        ClearWarnings();

        // 0.5초 텀으로 순차 폭발
        foreach (int idx in tileList)
        {
            SpawnExplosion(idx);

            // TODO: 여기에서 플레이어가 이 타일 안에 있으면
            // Player에게 HP 40% 데미지 주는 로직 호출
            // ex) GameManager.instance.player.TakeDamageByPercent(damagePercent);

            yield return new WaitForSeconds(explodeInterval);
        }
    }

    void ShowWarnings(List<int> tileList)
    {
        ClearWarnings();
        if (warningPrefab == null) return;

        warningObjects = new List<GameObject>();

        foreach (int idx in tileList)
        {
            Vector3 pos = tileCenters[idx];
            var obj = Instantiate(warningPrefab, pos, Quaternion.identity);
            warningObjects.Add(obj);
        }
    }

    void ClearWarnings()
    {
        if (warningObjects == null) return;

        foreach (var obj in warningObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        warningObjects.Clear();
    }

    void SpawnExplosion(int tileIndex)
    {
        if (explosionPrefab == null) return;

        Vector3 pos = tileCenters[tileIndex];
        Instantiate(explosionPrefab, pos, Quaternion.identity);
    }

    // 단순 셔플
    void Shuffle(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}

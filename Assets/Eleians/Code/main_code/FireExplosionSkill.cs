using System.Collections;
using UnityEngine;

public class FireExplosionSkill : MonoBehaviour
{
    [Header("Settings")]
    public int poolIndex = 9;        // Fire Explosion 프리팹 인덱스
    public float startDistance = 3f; // 플레이어 뒤에서 시작할 거리
    public float endDistance = 8f;   // 플레이어 앞으로 나갈 거리
    public float spawnInterval = 1.0f; // 폭발 간 간격
    public float runnerSpeed = 0.05f;  // 다음 폭발까지 걸리는 시간

    [Header("Size Settings")]
    // 아까 스크린샷 보니까 프리팹 기본 크기가 (3,3,3) 이더라구. 그걸 기준으로 잡았어.
    public Vector3 defaultScale = new Vector3(3, 3, 3);
    public float sizeGrowthRate = 0.05f; // 폭발당 커질 크기 (0.05)

    // 마지막 이동 방향을 기억할 변수
    private Vector3 lastMoveDir = Vector3.right;

    void Update()
    {
        // 플레이어가 이동 중이라면 방향 기억 (탑뷰 전방향 대응)
        if (GameManager.instance.player.inputVec != Vector2.zero)
        {
            lastMoveDir = GameManager.instance.player.inputVec.normalized;
        }
    }

    public void ActiveChainExplosion()
    {
        Player player = GameManager.instance.player;
        if (player == null) return;

        // 기억해둔 마지막 방향으로 발사
        StartCoroutine(RunExplosionRunner(player.transform.position, lastMoveDir));
    }

    IEnumerator RunExplosionRunner(Vector3 centerPos, Vector3 dir)
    {
        Vector3 currentPos = centerPos - (dir * startDistance);
        float travelled = -startDistance;

        // ★ 추가된 부분: 현재 커진 정도를 저장할 변수 (처음엔 0)
        float currentGrowth = 0f;

        while (travelled < endDistance)
        {
            // Spawn함수에 현재 커진 정도(currentGrowth)를 같이 전달
            SpawnExplosion(currentPos, currentGrowth);

            // 다음 폭발을 위해 크기 증가
            currentGrowth += sizeGrowthRate;

            // 다음 위치로 이동
            currentPos += dir * spawnInterval;
            travelled += spawnInterval;

            yield return new WaitForSeconds(runnerSpeed);
        }
    }

    // ★ 수정된 부분: 추가 크기(extraScale)를 매개변수로 받음
    void SpawnExplosion(Vector3 pos, float extraScale)
    {
        GameObject exp = PoolManager.instance.Get(poolIndex);
        exp.transform.position = pos;

        // ★ 핵심: 기본 크기에 추가된 크기를 더해서 적용
        // (Vector3.one * extraScale)은 (0.05, 0.05, 0.05) 같은 값을 만들어줌
        exp.transform.localScale = defaultScale + (Vector3.one * extraScale);

        exp.SetActive(true);
    }
}
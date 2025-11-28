using System.Collections;
using UnityEngine;

public class ShadowSpear : MonoBehaviour
{
    [Header("Target Objects")]
    public Transform spearVisual;   // 하늘에서 떨어질 창 (자식 오브젝트)
    public GameObject warningObject; // 바닥에 깔릴 경고 장판 (자식 오브젝트)
    public Animator spearAnim;      // 창 애니메이션 (SpearVisual에 있는 것)
    public Animator warningAnim;    // 경고 장판 애니메이션 (WarningArea에 있는 것, 없으면 비워도 됨)

    [Header("Distance Settings")]
    public float dropHeight = 15f; // 창이 생성될 높이

    [Header("Time Settings")]
    public float warningDuration = 0.6f; // 경고 장판 지속 시간 (창 떨어지기 전 대기)
    public float fallDuration = 0.2f;    // 창이 떨어지는 시간
    public float deleteDuration = 0.5f;  // 꽂힌 후 유지 시간

    [Header("Stats")]
    public float damage = 40f;
    public GameObject shockwavePrefab;

    private bool hasHitPlayer = false;
    private bool isFalling = false; // 공격 판정 활성화 여부

    void OnEnable()
    {
        hasHitPlayer = false;
        isFalling = false;

        // 1. 초기화
        // 부모(this)는 이미 BossMonster가 바닥 위치에 생성해줬으므로 건드리지 않음.

        // 경고 장판 켜기
        if (warningObject != null) warningObject.SetActive(true);
        if (warningAnim != null) warningAnim.Play("boss_spear_warning"); // 페이드인 애니메이션이 있다면 재생

        // 창(Visual)은 안 보이게 하거나 하늘 위로 올려둠
        if (spearVisual != null)
        {
            spearVisual.gameObject.SetActive(true);
            // 로컬 좌표 기준으로 Y축만 위로 올림 (부모가 바닥에 있으므로)
            spearVisual.localPosition = Vector3.up * dropHeight;

            if (spearAnim != null) spearAnim.Play("boss_spear_create"); // 창 생성 모션(공중)
        }

        StartCoroutine(SpearSequenceRoutine());
    }

    IEnumerator SpearSequenceRoutine()
    {
        // ----------------------------------------
        // 1단계: 경고 (Warning)
        // ----------------------------------------
        // 부모 오브젝트는 바닥에 고정, 경고 장판만 보이는 상태
        // 창은 하늘 위에 대기 중

        yield return new WaitForSeconds(warningDuration);

        // ----------------------------------------
        // 2단계: 낙하 (Fall)
        // ----------------------------------------
        isFalling = true; // ★ 이제부터 데미지 판정 시작
        if (spearAnim != null) spearAnim.Play("boss_spear_fall");

        float elapsed = 0f;
        Vector3 startLocalPos = Vector3.up * dropHeight;
        Vector3 endLocalPos = Vector3.zero; // 부모의 위치(바닥)가 (0,0,0)임

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fallDuration;

            // ★ 중요: 창(자식)만 움직임 (LocalPosition 사용)
            if (spearVisual != null)
            {
                spearVisual.localPosition = Vector3.Lerp(startLocalPos, endLocalPos, t);
            }

            yield return null;
        }

        // 바닥 도착 확정
        if (spearVisual != null) spearVisual.localPosition = endLocalPos;

        // ----------------------------------------
        // 3단계: 착지 및 소멸 (Impact)
        // ----------------------------------------
        // 바닥에 닿음
        if (!hasHitPlayer)
        {
            SpawnShockwave();
        }

        

        isFalling = false; // 판정 끝

        if (spearAnim != null) spearAnim.Play("boss_spear_delete");
        yield return new WaitForSeconds(deleteDuration);

        // 경고 장판은 이제 꺼도 됨 (창이 꽂혔으니까)
        if (warningObject != null) warningObject.SetActive(false);
        gameObject.SetActive(false);
    }

    void SpawnShockwave()
    {
        // 충격파는 부모의 위치(바닥)에 생성
        if (shockwavePrefab != null)
            Instantiate(shockwavePrefab, transform.position, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        // 떨어지는 중이 아니면(경고 시간 등) 데미지 없음
        if (hasHitPlayer || !isFalling) return;

        if (col.CompareTag("Player"))
        {
            hasHitPlayer = true;
            col.GetComponent<Player>()?.ApplyDamage(damage);

            // 플레이어 타격 시 처리 (여기선 그냥 끄기)
            gameObject.SetActive(false);
        }
    }
}

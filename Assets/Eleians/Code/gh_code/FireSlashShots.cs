using UnityEngine;

public class FireSlashShots : MonoBehaviour
{
    public float speed = 10f; // 투사체 날아가는 속도
    public float lifeTime = 3f; // 3초 뒤 자동 소멸 (무한 비행 방지)
    public float damage = 20f;
    private Vector3 moveDir;
    private float timer;

    // 활성화될 때마다 초기화
    private void OnEnable()
    {
        timer = 0f;
    }

    // FlameSword에서 이 함수를 호출해서 발사 방향을 정해줄 거야
    public void Launch(Vector3 direction)
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.flame_sword);

        moveDir = direction.normalized;

        // 투사체가 날아가는 방향을 바라보게 회전
        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void Update()
    {
        // 1. 방향대로 이동 (Rigidbody Simulated가 꺼져 있어도 작동)
        transform.Translate(moveDir * speed * Time.deltaTime, Space.World);

        // 2. 수명 관리 (화면 밖으로 나가면 풀로 반환)
        timer += Time.deltaTime;
        if (timer > lifeTime)
        {
            gameObject.SetActive(false);
        }
        damage = StatsManager.instance.Attack * 0.4f * (1 + 0.07f * StatsManager.instance.FireCnt);
    }
}
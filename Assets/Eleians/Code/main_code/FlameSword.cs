using System.Collections;
using UnityEngine;

public class FlameSword : MonoBehaviour
{
    [Header("투사체 ID")]
    public int projectileId = 8;

    // 검 데미지 (외부 공개용)
    public float damage;

    // 투사체 전용 스펙 (내부 저장용)
    private float projDamage;
    private float projScale;

    // ★ 안전장치: 애니메이션 길이보다 넉넉한 시간 (예: 1초)
    private float lifeTimeSafety = 0.5f;

    Animator anim;
    Collider2D col;
    SpriteRenderer sr;

    private bool isTriggerAttack;
    private Vector2 attackDir;
    private Transform owner;

    void Awake()
    {
        anim = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        // 주인 따라다니기
        if (owner != null)
        {
            transform.position = owner.position;
        }
        else
        {
            // 주인이 없으면(사망 등) 즉시 비활성화
            Unact();
        }
    }

    public void Init(float sDmg, float sScale, float pDmg, float pScale, bool _isReverse, bool _isTrigger, Vector2 _dir, Transform _owner)
    {
        damage = sDmg;
        projDamage = pDmg;
        projScale = pScale;

        isTriggerAttack = _isTrigger;
        attackDir = _dir;
        owner = _owner;

        // 크기 및 방향 설정
        transform.localScale = Vector3.one * sScale;
        sr.flipY = _isReverse;

        col.enabled = false;
        sr.enabled = true;

        AudioManager.instance.PlaySfx(AudioManager.Sfx.flame_sword);

        // ★ 중요: 이전 실행 때 남아있을지 모르는 예약 취소
        CancelInvoke(nameof(Unact));

        // 애니메이션 재생
        anim.Play("fire slash", -1, 0f);

        // ★ 안전장치 가동: 
        // 애니메이션 이벤트가 실패하더라도 lifeTimeSafety초 뒤에는 무조건 꺼짐.
        // (fire slash 애니메이션 길이보다 조금 더 길게 잡아줘, 보통 0.6~1.0초면 충분)
        Invoke(nameof(Unact), lifeTimeSafety);
    }

    // --- 애니메이션 이벤트 ---

    void MakeHitBox()
    {
        col.enabled = true;
        CheckAndFireProjectile();
    }

    void DeleteHitbox()
    {
        col.enabled = false;
    }

    void Unact()
    {
        // 혹시 Invoke로 불렸을 때를 대비해 중복 실행 방지
        if (!gameObject.activeSelf) return;

        gameObject.SetActive(false);
    }

    void CheckAndFireProjectile()
    {
        // [불 20] 3방향 발사
        if (isTriggerAttack)
        {
            FireProjectile(attackDir);
            Vector2 dirUp = Quaternion.AngleAxis(30f, Vector3.forward) * attackDir;
            FireProjectile(dirUp);
            Vector2 dirDown = Quaternion.AngleAxis(-30f, Vector3.forward) * attackDir;
            FireProjectile(dirDown);
            isTriggerAttack = false;
        }
        // [불 10] 기본 1방향 발사
        else if (StatsManager.instance.FireCnt >= 10)
        {
            FireProjectile(attackDir);
        }
    }

    void FireProjectile(Vector2 direction)
    {
        GameObject shotObj = GameManager.instance.pool.Get(projectileId);

        shotObj.transform.position = transform.position;
        shotObj.transform.rotation = Quaternion.identity;

        FireSlashShots shotScript = shotObj.GetComponent<FireSlashShots>();
        if (shotScript != null)
        {
            shotObj.transform.localScale = Vector3.one * projScale;
            shotScript.damage = projDamage;
            shotScript.Launch(direction);
        }
    }
}
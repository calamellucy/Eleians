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
        if (owner != null)
        {
            transform.position = owner.position;
        }
    }

    // ★ Init 함수가 검 정보(s~)와 투사체 정보(p~)를 모두 받음
    public void Init(float sDmg, float sScale, float pDmg, float pScale, bool _isReverse, bool _isTrigger, Vector2 _dir, Transform _owner)
    {
        damage = sDmg;       // 검 데미지

        projDamage = pDmg;   // 투사체 데미지
        projScale = pScale;  // 투사체 크기

        isTriggerAttack = _isTrigger;
        attackDir = _dir;
        owner = _owner;

        // 검 크기 적용
        transform.localScale = Vector3.one * sScale;

        // 방향 반전 (돌아올 때 뒤집힘)
        sr.flipY = _isReverse;

        col.enabled = false;
        sr.enabled = true;

        AudioManager.instance.PlaySfx(AudioManager.Sfx.flame_sword);
        anim.Play("fire slash", -1, 0f);
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
            // ★ 여기서 아까 받아온 투사체 전용 크기와 데미지를 씀
            shotObj.transform.localScale = Vector3.one * projScale;
            shotScript.damage = projDamage;

            shotScript.Launch(direction);
        }
    }
}
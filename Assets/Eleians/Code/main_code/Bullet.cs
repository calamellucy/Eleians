using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float damage;
    public int per;
    public StoneDust stoneDust;

    private Rigidbody2D rigid;
    private SpriteRenderer[] renderers;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
    }

    // ★ [추가] 풀에서 꺼내지자마자 실행됨 (Init보다 먼저 실행됨)
    // 여기서 잔상과 이전 속도를 즉시 제거해야 함!
    void OnEnable()
    {
        // 1. 혹시 모를 이전 코루틴 종료
        StopAllCoroutines();

        // 2. 일단 투명하게 (깜빡임 방지 핵심)
        if (renderers == null) renderers = GetComponentsInChildren<SpriteRenderer>();
        SetAlpha(0f);

        // 3. 물리 잔상 제거
        if (rigid == null) rigid = GetComponent<Rigidbody2D>();
        rigid.simulated = false; // 물리 끄기
        rigid.linearVelocity = Vector2.zero; // 속도 0
        rigid.angularVelocity = 0f; // 회전 0

        // 태그 초기화
        gameObject.tag = "Untagged";
    }

    public void Init(float damage, int per, Vector3 dir, float speed, float lifeTime, StoneDust dustComp)
    {
        this.damage = damage;
        this.per = per;
        this.stoneDust = dustComp;

        // 회전 고정 (필요 시)
        if (rigid.constraints == RigidbodyConstraints2D.None)
            rigid.constraints = RigidbodyConstraints2D.FreezeRotation;

        // OnEnable에서 이미 초기화했지만, 확실하게 한 번 더 세팅하고 발사 로직 시작
        StartCoroutine(SpawnAndFireRoutine(dir, speed, lifeTime));
    }

    public void SetColor(Color color)
    {
        if (renderers == null) renderers = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = color;
    }

    public void OnHit()
    {
        if (StatsManager.instance != null && StatsManager.instance.EarthCnt >= 10)
        {
            if (stoneDust != null)
            {
                Vector2 dir = rigid.linearVelocity.normalized;
                stoneDust.SpawnExplosion(transform.position, dir);
            }
        }

        per--;
        if (per < 0)
        {
            rigid.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
        }
    }

    IEnumerator SpawnAndFireRoutine(Vector3 dir, float speed, float lifeTime)
    {
        AudioManager.instance.PlaySfx(AudioManager.Sfx.stoneSummon);

        // --- [1] 대기 (페이드 인) ---
        float fadeTime = 0.4f;
        float waitTime = 0.8f;
        float t = 0f;

        // 루프 돌면서 페이드인
        while (t < waitTime)
        {
            t += Time.deltaTime;
            if (t <= fadeTime) SetAlpha(t / fadeTime);
            else SetAlpha(1f);
            yield return null;
        }

        SetAlpha(1f);


        // --- [2] 발사 ---
        gameObject.tag = "Bullet";
        AudioManager.instance.PlaySfx(AudioManager.Sfx.stoneShot);

        // ★ 출발 직전에 물리 켜기
        rigid.simulated = true;
        rigid.linearVelocity = dir * speed;

        // --- [3] 수명 ---
        yield return new WaitForSeconds(lifeTime);
        gameObject.SetActive(false);
    }

    void SetAlpha(float a)
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            Color c = renderers[i].color;
            c.a = a;
            renderers[i].color = c;
        }
    }

    void OnDisable()
    {
        rigid.linearVelocity = Vector2.zero;
    }
}
using System.Collections;
using UnityEngine;

public class skill4Gh : MonoBehaviour
{
    public int prefabId;
    public float speed = 5f;
    public float damage = 10f;
    public float lifeTime = 2f;          // ← 발사 "후" 생존시간

    public float burstInterval = 4f;
    public int shotsPerBurst = 45;
    public float burstDuration = 1.5f;

    public float minSpawnRadius = 0.3f;
    public float maxSpawnRadius = 1.0f;

    Coroutine loopCo;

    void Start()
    {
        if (loopCo != null) StopCoroutine(loopCo);
        loopCo = StartCoroutine(Loop());
    }

    IEnumerator Loop()
    {
        if (GameManagerGh.instance == null || GameManagerGh.instance.pool == null || GameManagerGh.instance.player == null)
        {
            Debug.LogError("GameManagerGh / PoolManagerGh / PlayerGh reference missing");
            yield break;
        }
        if (prefabId < 0 || prefabId >= GameManagerGh.instance.pool.prefabs.Length)
        {
            Debug.LogError($"prefabId {prefabId} out of range");
            yield break;
        }

        while (true)
        {
            yield return StartCoroutine(Burst());
            yield return new WaitForSeconds(burstInterval);
        }
    }

    IEnumerator Burst()
    {
        float gap = burstDuration / Mathf.Max(1, shotsPerBurst);
        for (int i = 0; i < shotsPerBurst; i++)
        {
            SpawnAndPrepare(); 
            yield return new WaitForSeconds(gap);
        }
    }

    void SpawnAndPrepare()
    {
        var player = GameManagerGh.instance.player;

        
        Vector3 origin = player.transform.position;
        Vector2 randDir = Random.insideUnitCircle.normalized;
        float r = Mathf.Sqrt(Random.Range(minSpawnRadius * minSpawnRadius,
                                          maxSpawnRadius * maxSpawnRadius));
        Vector3 spawnPos = origin + (Vector3)(randDir * r);

        GameObject go = GameManagerGh.instance.pool.Get(prefabId);
        if (!go) return;

        Transform t = go.transform;
        t.SetParent(transform, false);
        t.position = spawnPos;

        Vector2 fireDir = (player.inputVec.sqrMagnitude > 0.0001f)
                        ? player.inputVec.normalized
                        : (player.IsFacingRight ? Vector2.right : Vector2.left);

        float angle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;
        t.rotation = Quaternion.Euler(0, 0, angle);


        var rb = go.GetComponent<Rigidbody2D>();
        if (!rb) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;



        var b = go.GetComponent<BulletGh>();
        if (b) b.Init(damage, 3, Vector3.zero);


        StartCoroutine(FadeAndFire(go, fireDir));
    }

    IEnumerator FadeAndFire(GameObject go, Vector2 dir)
    {
        if (!go) yield break;

        // 스프라이트들 모으기(식 포함)
        var renderers = go.GetComponentsInChildren<SpriteRenderer>(true);

        // 시작 알파 0으로
        SetAlpha(renderers, 0f);

        // 0.8초 동안 서서히 나타남
        float fadeTime = 0.8f;
        float t = 0f;
        while (t < fadeTime && go.activeInHierarchy)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeTime);
            SetAlpha(renderers, a);
            yield return null;
        }

        // 0.2초 대기(완전히 보이지만 정지)
        yield return new WaitForSeconds(0.2f);

        // 발사!
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb) rb.linearVelocity = dir * speed;

        // 발사 후 lifeTime 경과 시 비활성화
        yield return new WaitForSeconds(lifeTime);
        if (go) go.SetActive(false);
    }

    void SetAlpha(SpriteRenderer[] srs, float a)
    {
        for (int i = 0; i < srs.Length; i++)
        {
            var c = srs[i].color;
            c.a = a;
            srs[i].color = c;
        }
    }
}

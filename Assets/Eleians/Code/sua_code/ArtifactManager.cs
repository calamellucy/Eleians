using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AudioManager;

/// <summary>
/// 아티팩트 전체 관리 시스템 (최종 확정 버전)
/// </summary>
public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager instance;
    private void Awake() { instance = this; }

    [Header("Artifact Flags")]
    public bool hasElixir = false;          // 생명의 묘약 (Elixir of Life)
    public bool hasGuardiansVow = false;    // 수호자의 결의 (Guardian's Vow)
    public bool hasShadowClone = false;     // 그림자 분신 (Illusion Totem)
    public bool hasOverload = false;        // 과부하 (Overload Crystal)
    public bool hasThunderSoul = false;     // 뇌신의 영혼석 (Thunder Soul)
    public bool hasFrostArmor = false;      // 서리 갑옷 (Frost Armor)
    public bool hasMirror = false;          // 복수의 거울 (Mirror of Vengeance)
    public bool hasSolarCloak = false;      // 태양의 망토 (Solar Cloak)
    public bool hasMagmaCore = false;       // 마그마 코어 (Magma Core)
    public bool hasEarthResonance = false;  // 대지의 공명 (Earth Resonance)
    public bool hasObsidianEdge = false;    // 흑요석 칼날 (Obsidian Edge)

    // --- 내부 변수들 ---
    [Header("Runtime Variables")]
    public int overloadStacks = 0;          // 과부하 스택 (UI 표시용 public)
    public int earthResonanceStacks = 0;    // 대지의 공명 스택 (UI 표시용 public)

    private Coroutine earthResonanceResetCoroutine;
    private float frostArmorCooldown = 0f;
    private float solarCloakTimer = 0f;

    [Header("Visual Effects & Prefabs (Inspector 연결 필수!)")]
    public GameObject shadowClonePrefab;    // [그림자 분신] 소환할 프리팹
    public GameObject reviveEffectPrefab;   // [뇌신의 영혼석] 부활 이펙트
    public GameObject overloadExplosionPrefab; // [과부하] 폭발 이펙트
    public GameObject frostArmorEffectPrefab;  // [서리 갑옷] 무적 이펙트
    public GameObject solarCloakEffectPrefab; // 태양의 망토 이펙트 프리팹
    public Vector2 solarCloakOffset = new Vector2(0f, 0.5f);

    // 아티팩트 데이터 목록 & 획득 리스트
    public List<ArtifactData> allArtifacts;
    private List<ArtifactData> ownedArtifacts = new List<ArtifactData>();

    [Header("DEBUG")]
    public int debugArtifactIndex = 0; // 인스펙터에서 원하는 번호 입력

    [ContextMenu("테스트: 아티팩트 획득하기")] // 컴포넌트 우클릭 메뉴
    public void DebugAcquire()
    {
        if (allArtifacts != null && allArtifacts.Count > debugArtifactIndex)
        {
            Debug.Log($"[치트] {allArtifacts[debugArtifactIndex].artifactName} 획득!");
            AcquireArtifact(allArtifacts[debugArtifactIndex]);
        }
    }

    // ... (GetRandomArtifacts, AcquireArtifact 등 기존 함수 동일 유지) ...
    public List<ArtifactData> GetRandomArtifacts(int count = 3)
    {
        List<ArtifactData> result = new List<ArtifactData>();
        List<ArtifactData> pool = new List<ArtifactData>(allArtifacts);
        pool.RemoveAll(a => ownedArtifacts.Contains(a));
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }
        return result;
    }

    public void AcquireArtifact(ArtifactData data)
    {
        if (ownedArtifacts.Contains(data)) return;
        ownedArtifacts.Add(data);
        ArtifactSlotUI.instance.AddArtifact(data); // UI 있으면 주석 해제
        ApplyArtifact(data);
    }

    // ========================================================================
    //  아티팩트 적용 로직
    // ========================================================================
    private void ApplyArtifact(ArtifactData data)
    {
        switch (data.id)
        {
            case ArtifactID.ElixirOfLife: // [생명의 묘약]
                hasElixir = true;
                StatsManager.instance.artifactSpeedMult += 0.5f; // 이속 10%
                StartCoroutine(ElixirHealRoutine()); // 자동 회복 시작
                break;

            case ArtifactID.GuardiansVow: // [수호자의 결의]
                hasGuardiansVow = true;
                break;

            case ArtifactID.IllusionTotem: // [그림자 분신]
                hasShadowClone = true;
                break;

            case ArtifactID.OverloadCrystal: // [과부하]
                hasOverload = true;
                StartCoroutine(OverloadExplosionRoutine());
                break;

            case ArtifactID.ThunderSoul: // [뇌신의 영혼석]
                hasThunderSoul = true;
                break;

            case ArtifactID.FrostArmor: // [서리 갑옷]
                hasFrostArmor = true;
                break;

            case ArtifactID.MirrorOfVengeance: // [복수의 거울]
                hasMirror = true;
                StatsManager.instance.artifactAtkSpdMult -= 0.4f;
                StatsManager.instance.artifactCritChanceAdd -= 0.4f;
                StatsManager.instance.artifactDmgTakenMult -= 0.4f; // 받는 피해 40% 감소
                StatsManager.instance.ReflectDamage += 1.5f;        // 반사 150%
                break;

            case ArtifactID.SolarCloak: // [태양의 망토]
                hasSolarCloak = true;
                break;

            case ArtifactID.MagmaCore: // [마그마 코어]
                hasMagmaCore = true;
                break;

            case ArtifactID.EarthResonance: // [대지의 공명]
                hasEarthResonance = true;
                break;

            case ArtifactID.ObsidianEdge: // [흑요석 칼날]
                hasObsidianEdge = true;
                StatsManager.instance.artifactCritDmgAdd += 0.6f;
                StatsManager.instance.artifactCritChanceAdd += 0.1f;
                break;
        }

        StatsManager.instance.RecalculateStats();
    }

    // ========================================================================
    //  주기적 효과 (Update / Coroutine)
    // ========================================================================

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        // [태양의 망토] 매초 주변 적 데미지
        if (hasSolarCloak)
        {
            solarCloakTimer += Time.deltaTime;
            if (solarCloakTimer >= 1f)
            {
                solarCloakTimer = 0f;
                SolarCloakDamage();
            }
        }
    }

    // [생명의 묘약] 5초마다 체력 회복 + 초록색 글씨
    IEnumerator ElixirHealRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (GameManager.instance.isLive)
            {
                float healAmount = StatsManager.instance.MaxHP * 0.02f; // 최대 체력 2%
                GameManager.instance.player.Heal(healAmount);

            }
        }
    }

    // [과부하] 5초마다 폭발 (공격속도 비례)
    IEnumerator OverloadExplosionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (overloadStacks > 0)
            {
                // 1. [로직] 스택에 비례해서 범위(Radius)와 데미지 결정
                // 기본 3m + (스택당 0.2m 증가) -> 스택 50이면 13m (화면 전체급)
                float currentRadius = 3.0f + (overloadStacks * 0.2f);
                // 데미지 조정 (스택당 10 데미지)
                float dmg = overloadStacks * 5f;

                // 2. [로직] 계산된 범위 내의 적 타격
                // (이제 이펙트 범위와 이 판정 범위가 일치하게 됩니다)
                Collider2D[] hits = Physics2D.OverlapCircleAll(GameManager.instance.player.transform.position, currentRadius);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Enemy"))
                    {
                        MonsterBase m = hit.GetComponent<MonsterBase>();
                        if (m) m.ApplyDamage(dmg, ElementType.Lightning);
                    }
                }

                // 3. [시각] 균일한 패턴으로 폭발 생성
                if (overloadExplosionPrefab != null)
                {
                    // 스택이 많을수록 이펙트 개수도 늘려서 꽉 차게 보이게 함
                    // (최소 5개 ~ 최대 40개 제한으로 렉 방지)
                    int effectCount = Mathf.Clamp(5 + overloadStacks, 5, 40);

                    StartCoroutine(SpawnUniformExplosion(effectCount, currentRadius));
                }

                // 4. 초기화
                StatsManager.instance.artifactAtkSpdMult -= (overloadStacks * 0.01f);
                overloadStacks = 0;
                StatsManager.instance.RecalculateStats();
                ArtifactSlotUI.instance.UpdateArtifactStack(ArtifactID.OverloadCrystal, 0);
            }
        }
    }

    // ★ [추가] 수학적으로 균일하게(나선형) 퍼지는 폭발 연출
    IEnumerator SpawnUniformExplosion(int count, float radius)
    {
        Vector3 center = GameManager.instance.player.transform.position;

        // 정중앙에 하나 쾅!
        GameObject centerVfx = Instantiate(overloadExplosionPrefab, center, Quaternion.identity);
        Destroy(centerVfx, 1.0f);

        // 페르마의 나선 (Fermat's Spiral) 공식 사용 -> 원을 가장 균일하게 채우는 방식
        // 해바라기 씨앗 배치와 같습니다. 빈 공간 없이 예쁘게 터집니다.
        float goldenAngle = 137.508f; // 황금각

        for (int i = 0; i < count; i++)
        {
            // 순번(i)이 뒤로 갈수록 중심에서 멀어짐
            // 비율(0~1) 계산: 뒤로 갈수록 1에 가까워짐
            float ratio = (float)i / count;

            // 거리 계산: 제곱근을 써야 원 안쪽에 뭉치지 않고 골고루 퍼짐
            float distance = radius * Mathf.Sqrt(ratio);

            // 각도 계산
            float angle = i * goldenAngle;

            // 좌표 변환 (극좌표 -> 직교좌표)
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
            float y = Mathf.Sin(angle * Mathf.Deg2Rad) * distance;

            Vector3 spawnPos = center + new Vector3(x, y, 0);

            // 이펙트 생성
            GameObject vfx = Instantiate(overloadExplosionPrefab, spawnPos, Quaternion.identity);

            // 외곽으로 갈수록 조금 더 크게, 혹은 랜덤 크기
            vfx.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);

            Destroy(vfx, 1.0f);

            // 한 번에 다 터지면 렉 걸리니까 아주 빠르게 순차적으로 터짐 (두두두둥!)
            yield return new WaitForSeconds(0.02f);
        }
    }

    // [태양의 망토] 주변 데미지
    void SolarCloakDamage()
    {
        //Vector2 centerPos = GameManager.instance.player.transform.position;
        Vector2 centerPos;
        if (GameManager.instance.isTowerPhase && GameManager.instance.tower != null)
        {
            centerPos = GameManager.instance.tower.transform.position;
        }
        else return;

        // ★ [추가] 화염 장판 이펙트 생성
        if (solarCloakEffectPrefab != null)
        {
            // centerPos에 오프셋을 더해서 위치를 살짝 옮김
            Vector2 spawnPos = centerPos + solarCloakOffset;

            GameObject vfx = Instantiate(solarCloakEffectPrefab, spawnPos, Quaternion.identity);

            Destroy(vfx, 0.6f);
        }

        // 반경 4m 내 적
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(centerPos, 4f);
        float dmg = StatsManager.instance.Attack * 0.5f;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                MonsterBase monster = hitCollider.GetComponent<MonsterBase>();
                if (monster != null) monster.ApplyDamage(dmg, ElementType.Fire);
            }
        }
    }

    // ========================================================================
    //  이벤트 훅 (외부 호출)
    // ========================================================================

    // 1. 플레이어 공격 시
    public void OnPlayerAttack(MonsterBase target, ref float damage, bool isCrit)
    {
        // [수호자의 결의]
        if (hasGuardiansVow)
        {
            if (target.myType == MonsterType.Tower) damage *= 1.5f; // 거점 몹 추뎀

            // 거점 위기 시 2배
            GameObject towerObj = GameManager.instance.tower;
            if (towerObj != null)
            {
                Tower towerScript = towerObj.GetComponent<Tower>();
                if (towerScript != null && !towerScript.isDestroyed)
                {
                    if ((towerScript.currentHealth / towerScript.maxHealth) <= 0.3f)
                        damage *= 2.0f;
                }
            }
        }

        // [마그마 코어]
        if (hasMagmaCore)
        {
            float dist = Vector2.Distance(GameManager.instance.player.transform.position, target.transform.position);
            if (dist < 3f) damage *= 1.6f;
            else damage *= 0.7f;
        }

        // [흑요석 칼날]
        if (hasObsidianEdge && !isCrit)
        {
            damage *= 0.7f;
        }
    }

    // 2. 적 처치 시
    public void OnEnemyKilled(MonsterBase monster)
    {
        // [그림자 분신]
        if (hasShadowClone && Random.value < 0.1f)
        {
            if (shadowClonePrefab != null)
            {
                // 그림자 생성 (해당 프리팹에는 'Untagged' 혹은 'Player' 태그와 어그로 끌리는 로직 필요)
                Instantiate(shadowClonePrefab, monster.transform.position, Quaternion.identity);
                Debug.Log("그림자 분신 생성!");
            }
        }

        // [과부하] (공속 증가)
        if (hasOverload)
        {
            overloadStacks++;
            // ★ [추가] UI 갱신 호출
            ArtifactSlotUI.instance.UpdateArtifactStack(ArtifactID.OverloadCrystal, overloadStacks);
            StatsManager.instance.artifactAtkSpdMult += 0.01f; // 공속 1% 증가
            StatsManager.instance.RecalculateStats();
        }
    }

    // 3. 치명타 발생 시
    public void OnCritProc()
    {
        // [대지의 공명]
        if (hasEarthResonance)
        {
            if (earthResonanceStacks < 25)
            {
                earthResonanceStacks++;
                // ★ [추가] UI 갱신 호출
                ArtifactSlotUI.instance.UpdateArtifactStack(ArtifactID.EarthResonance, earthResonanceStacks);
                StatsManager.instance.artifactAtkSpdMult -= 0.01f;
                StatsManager.instance.artifactCritDmgAdd += 0.01f;
                StatsManager.instance.RecalculateStats();
            }

            if (earthResonanceResetCoroutine != null) StopCoroutine(earthResonanceResetCoroutine);
            earthResonanceResetCoroutine = StartCoroutine(ResetEarthResonance());
        }
    }

    IEnumerator ResetEarthResonance()
    {
        yield return new WaitForSeconds(2f);
        StatsManager.instance.artifactAtkSpdMult += (earthResonanceStacks * 0.01f);
        StatsManager.instance.artifactCritDmgAdd -= (earthResonanceStacks * 0.01f);
        earthResonanceStacks = 0;
        // ★ [추가] 초기화 알림
        ArtifactSlotUI.instance.UpdateArtifactStack(ArtifactID.EarthResonance, 0);
        StatsManager.instance.RecalculateStats();
    }

    // 4. 플레이어 사망 시 (부활)
    public bool TryRevive()
    {
        if (hasThunderSoul || hasMirror)
        {
            if (hasThunderSoul) hasThunderSoul = false;
            else if (hasMirror) hasMirror = false; // 복수의 거울 부활권 소모

            // 화면 전체 전기 데미지 & 스턴
            NormalMonster[] monsters = FindObjectsByType<NormalMonster>(FindObjectsSortMode.None);
            foreach (var m in monsters)
            {
                m.ApplyDamage(StatsManager.instance.Attack * 5f, ElementType.Lightning);
            }

            // ★ 부활 이펙트
            if (reviveEffectPrefab != null)
                Instantiate(reviveEffectPrefab, GameManager.instance.player.transform.position, Quaternion.identity);

            // 체력 절반 회복
            GameManager.instance.health = StatsManager.instance.MaxHP * 0.5f;
            Debug.Log("부활!");
            return true;
        }
        return false;
    }

    // 5. 피격 시 (무적)
    public bool OnPlayerTakeDamage()
    {
        // [서리 갑옷]
        if (hasFrostArmor && Time.time > frostArmorCooldown)
        {
            frostArmorCooldown = Time.time + 30f;
            StartCoroutine(FrostArmorRoutine());
            return true;
        }
        return false;
    }

    IEnumerator FrostArmorRoutine()
    {
        StatsManager.instance.artifactAtkMult += 0.5f;
        StatsManager.instance.artifactSpeedMult += 0.5f;
        StatsManager.instance.RecalculateStats();

        GameManager.instance.player.SetInvincible(3f);

        // ★ 무적 이펙트 생성 (플레이어 자식으로 붙이기)
        GameObject vfx = null;
        if (frostArmorEffectPrefab != null)
        {
            vfx = Instantiate(frostArmorEffectPrefab, GameManager.instance.player.transform);
        }

        yield return new WaitForSeconds(3f);

        if (vfx != null) Destroy(vfx); // 이펙트 삭제

        StatsManager.instance.artifactAtkMult -= 0.5f;
        StatsManager.instance.artifactSpeedMult -= 0.5f;
        StatsManager.instance.RecalculateStats();
    }
}

/// <summary>
/// 아티팩트 ID (최종 확정)
/// </summary>
public enum ArtifactID
{
    ElixirOfLife,       // 생명의 묘약
    GuardiansVow,       // 수호자의 결의
    IllusionTotem,      // 그림자 분신
    OverloadCrystal,    // 과부하
    ThunderSoul,        // 뇌신의 영혼석
    // ThunderHammer,   // 삭제됨
    FrostArmor,         // 서리 갑옷
    MirrorOfVengeance,  // 복수의 거울
    SolarCloak,         // 태양의 망토
    MagmaCore,          // 마그마 코어
    EarthResonance,     // 대지의 공명
    ObsidianEdge        // 흑요석 칼날
}
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 아티팩트 전체 관리 시스템
/// - 아티팩트 정보 저장
/// - 선택 UI에서 선택한 아티팩트 적용
/// - 중복 체크
/// - 스탯 변화 반영까지 책임
/// </summary>
public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager instance;
    private void Awake() { instance = this; }

    // --- 아티팩트 보유 여부 플래그 (체크용) ---
    public bool hasCaffeine = false;        // 카페인 수혈
    public bool hasHotfix = false;          // 긴급 핫픽스
    public bool hasCompileError = false;    // 컴파일 에러
    public bool hasGitConflict = false;     // 깃허브 충돌
    public bool hasStackOverflow = false;   // 스택 오버플로우
    public bool hasBackupServer = false;    // 백업 서버
    public bool hasFinalSpec = false;       // 개발명세서
    public bool hasEscapeNo1 = false;       // 위기탈출 넘버원
    public bool hasHanwhaFan = false;       // 극성 한화팬
    public bool hasFirewall = false;        // 방화벽
    public bool hasRestrainingOrder = false;// 접근금지령
    public bool hasCriticalChain = false;   // 크리티컬 체인
    public bool hasDopamine = false;        // 도파민 중독

    // --- 내부 변수들 ---
    private int stackOverflowStacks = 0;
    private int criticalChainStacks = 0;
    private Coroutine critChainResetCoroutine;
    private float escapeNo1Cooldown = 0f;

    // 방화벽 관련
    private float firewallTimer = 0f;

    [Header("DEBUG")]
    public int debugArtifactIndex = 0; // 인스펙터에서 원하는 번호를 입력

    [ContextMenu("테스트: 아티팩트 획득하기")] // 우클릭 메뉴에 뜸
    public void DebugAcquire()
    {
        if (allArtifacts != null && allArtifacts.Count > debugArtifactIndex)
        {
            Debug.Log($"[치트] {allArtifacts[debugArtifactIndex].artifactName} 획득!");
            AcquireArtifact(allArtifacts[debugArtifactIndex]);
        }
    }


    // 아티팩트 데이터 목록
    public List<ArtifactData> allArtifacts;

    // 플레이어가 이미 획득한 아티팩트
    private List<ArtifactData> ownedArtifacts = new List<ArtifactData>();

    /// <summary> 랜덤 3개 아티팩트 뽑기 </summary>
    public List<ArtifactData> GetRandomArtifacts(int count = 3)
    {
        List<ArtifactData> result = new List<ArtifactData>();
        List<ArtifactData> pool = new List<ArtifactData>(allArtifacts);

        // 이미 가진 건 제외하고 싶다면 여기에 필터
        pool.RemoveAll(a => ownedArtifacts.Contains(a));

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    /// <summary> 아티팩트 획득 처리 </summary>
    public void AcquireArtifact(ArtifactData data)
    {
        if (ownedArtifacts.Contains(data))
        {
            Debug.Log($"{data.artifactName} 이미 보유 중");
            return;
        }

        ownedArtifacts.Add(data);

        // 슬롯 아이콘 추가
        ArtifactSlotUI.instance.AddArtifactIcon(data.icon);

        ApplyArtifact(data);
    }

    /// <summary> 아티팩트 효과 적용 (실제 구현은 각 아티팩트 클래스가 담당) </summary>
    private void ApplyArtifact(ArtifactData data)
    {
        switch (data.id)
        {
            case ArtifactID.Caffeine: // [카페인 수혈]
                hasCaffeine = true;
                StatsManager.instance.artifactSpeedMult += 0.1f; // 이속 10%
                StartCoroutine(CaffeineHealRoutine()); // 자동 회복 시작
                break;

            case ArtifactID.Hotfix: // [긴급 핫픽스]
                hasHotfix = true;
                break;

            case ArtifactID.CompileError: // [컴파일 에러]
                hasCompileError = true;
                break;

            case ArtifactID.GitConflict: // [깃허브 충돌]
                hasGitConflict = true;
                break;

            case ArtifactID.StackOverflow: // [스택 오버플로우]
                hasStackOverflow = true;
                StartCoroutine(StackOverflowRoutine());
                break;

            case ArtifactID.Overthink: // [고민중독]
                // TODO: UIManager나 ArtifactUI에 리롤 횟수 추가 함수 호출
                // UIManager.instance.AddRerollChance(1);
                Debug.Log("리롤 횟수 1회 증가");
                break;

            case ArtifactID.BackupServer: // [백업 서버]
                hasBackupServer = true;
                break;

            case ArtifactID.FinalSpecPDF: // [개발명세서]
                hasFinalSpec = true;
                float bonus = StatsManager.instance.AttackSpeed * 0.5f;
                StatsManager.instance.artifactAtkMult += bonus;
                StatsManager.instance.artifactAtkSpdMult -= 0.08f;
                break;

            case ArtifactID.EscapeNumberOne: // [위기탈출 넘버원]
                hasEscapeNo1 = true;
                break;

            case ArtifactID.HanwhaFan: // [극성 한화팬]
                hasHanwhaFan = true;
                StatsManager.instance.artifactAtkSpdMult -= 0.4f;
                StatsManager.instance.artifactCritChanceAdd -= 0.4f;
                StatsManager.instance.artifactDmgTakenMult -= 0.4f;
                StatsManager.instance.ReflectDamage += 1.5f;
                hasBackupServer = true; // 부활권이 여기에도 포함 (로직 공유)
                break;

            case ArtifactID.Firewall: // [방화벽]
                hasFirewall = true;
                break;

            case ArtifactID.RestrainingOrder: // [접근금지령]
                hasRestrainingOrder = true;
                break;

            case ArtifactID.CriticalChain: // [크리티컬 체인]
                hasCriticalChain = true;
                break;

            case ArtifactID.DopamineAddict: // [도파민 중독]
                hasDopamine = true;
                StatsManager.instance.artifactCritDmgAdd += 0.6f;
                StatsManager.instance.artifactCritChanceAdd += 0.1f;
                break;
        }

        // 스탯 변경 적용을 위해 갱신
        StatsManager.instance.RecalculateStats();
    }
    // ========================================================================
    //  주기적 효과 (Update / Coroutine)
    // ========================================================================

    void Update()
    {
        if (!GameManager.instance.isLive) return;

        // [방화벽] 매초 주변 적 데미지
        if (hasFirewall)
        {
            firewallTimer += Time.deltaTime;
            if (firewallTimer >= 1f)
            {
                firewallTimer = 0f;
                FirewallDamage();
            }
        }
    }

    // [카페인 수혈] 5초마다 체력 회복
    IEnumerator CaffeineHealRoutine()
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

    // [스택 오버플로우] 5초마다 폭발
    IEnumerator StackOverflowRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(5f);
            if (stackOverflowStacks > 0)
            {
                float dmg = stackOverflowStacks * 10f; // 스택당 10데미지
                // 화면 내 모든 적 타격 (간략화)
                NormalMonster[] monsters = FindObjectsByType<NormalMonster>(FindObjectsSortMode.None);
                foreach (var m in monsters) m.ApplyDamage(dmg);

                // 스택 초기화 및 공속 버프 해제
                StatsManager.instance.artifactAtkMult -= (stackOverflowStacks * 0.01f);
                stackOverflowStacks = 0;
                StatsManager.instance.RecalculateStats();
            }
        }
    }

    // [방화벽] 주변 데미지 로직
    void FirewallDamage()
    {
        // 거점 페이즈면 거점 주변, 아니면 플레이어 주변
        Vector2 centerPos = GameManager.instance.player.transform.position;
        if (GameManager.instance.isTowerPhase && GameManager.instance.tower != null)
        {
            centerPos = GameManager.instance.tower.transform.position;
        }

        // 반경 5m 내 적 감지
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(centerPos, 5f);
        float dmg = StatsManager.instance.Attack * 0.5f;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                MonsterBase monster = hitCollider.GetComponent<MonsterBase>();
                // 화염 데미지
                if (monster != null) monster.ApplyDamage(dmg, ElementType.Fire);
            }
        }
    }

    // ========================================================================
    //  이벤트 훅 (외부 호출)
    // ========================================================================

    // 1. 공격 시 (ArtifactManager.OnPlayerAttack)
    public void OnPlayerAttack(MonsterBase target, ref float damage, bool isCrit)
    {
        // [긴급 핫픽스]
        if (hasHotfix)
        {
            // [수정] 이제 myType(Enum)으로 정확하게 확인 가능!
            if (target.myType == MonsterType.Tower)
            {
                damage *= 1.5f; // 거점 몬스터에게 50% 추뎀
            }

            // 거점 체력 30% 이하면 공격력 2배
            GameObject towerObj = GameManager.instance.tower;

            if (towerObj != null)
            {
                // GameObject에 붙어있는 Tower 스크립트를 가져옵니다.
                Tower towerScript = towerObj.GetComponent<Tower>();

                if (towerScript != null && !towerScript.isDestroyed)
                {
                    float hpRatio = towerScript.currentHealth / towerScript.maxHealth;

                    // 체력이 30% 이하면 데미지 2배
                    if (hpRatio <= 0.3f)
                    {
                        damage *= 2.0f;
                        Debug.Log("긴급 핫픽스 발동! 공격력 2배");
                    }
                }
            }
        }

        // [컴파일 에러]
        if (hasCompileError && Random.value < 0.5f)
        {
            damage *= (Random.value < 0.5f) ? 0.5f : 2.0f;
        }

        // [접근 금지령]
        if (hasRestrainingOrder)
        {
            float dist = Vector2.Distance(GameManager.instance.player.transform.position, target.transform.position);
            if (dist < 3f) damage *= 1.6f;
            else damage *= 0.7f;
        }

        // [도파민 중독]
        if (hasDopamine && !isCrit)
        {
            damage *= 0.7f;
        }
    }

    // 2. 적 처치 시 (ArtifactManager.OnEnemyKilled)
    public void OnEnemyKilled(MonsterBase monster)
    {
        // [깃허브 충돌] 코드 덩어리(미끼) 생성
        if (hasGitConflict && Random.value < 0.1f)
        {
            // DummyPrefab 생성 (어그로 끌리는 오브젝트)
            // Instantiate(codeBlockPrefab, monster.transform.position, Quaternion.identity);
            Debug.Log("코드 덩어리 생성!");
        }

        // [스택 오버플로우]
        if (hasStackOverflow)
        {
            stackOverflowStacks++;
            StatsManager.instance.artifactAtkMult += 0.01f;
            StatsManager.instance.RecalculateStats();
        }
    }

    // 3. 치명타 발생 시
    public void OnCritProc()
    {
        // [크리티컬 체인]
        if (hasCriticalChain)
        {
            if (criticalChainStacks < 25)
            {
                criticalChainStacks++;
                StatsManager.instance.artifactAtkSpdMult -= 0.01f;
                StatsManager.instance.artifactCritDmgAdd += 0.01f;
                StatsManager.instance.RecalculateStats();
            }

            if (critChainResetCoroutine != null) StopCoroutine(critChainResetCoroutine);
            critChainResetCoroutine = StartCoroutine(ResetCritChain());
        }
    }

    IEnumerator ResetCritChain()
    {
        yield return new WaitForSeconds(2f);
        StatsManager.instance.artifactAtkSpdMult += (criticalChainStacks * 0.01f);
        StatsManager.instance.artifactCritDmgAdd -= (criticalChainStacks * 0.01f);
        criticalChainStacks = 0;
        StatsManager.instance.RecalculateStats();
    }

    // 4. 플레이어 사망 시 (Player.Die에서 호출)
    // 리턴값: true면 부활 성공, false면 진짜 사망
    public bool TryRevive()
    {
        if (hasBackupServer || hasHanwhaFan) // 백업서버 or 한화팬
        {
            if (hasBackupServer) hasBackupServer = false; // 소모성 (한 번만)
            else if (hasHanwhaFan) hasHanwhaFan = false;

            // 화면 전체 전기 데미지 & 스턴
            NormalMonster[] monsters = FindObjectsByType<NormalMonster>(FindObjectsSortMode.None);
            foreach (var m in monsters)
            {
                m.ApplyDamage(StatsManager.instance.Attack * 5f, ElementType.Lightning);
            }

            // 체력 절반 회복
            GameManager.instance.health = StatsManager.instance.MaxHP * 0.5f;
            Debug.Log("백업 서버 가동! 부활했습니다.");
            return true;
        }
        return false;
    }

    // 5. 플레이어가 데미지를 입으려 할 때 (Player.ApplyDamage에서 호출)
    // 리턴값: true면 "이번 데미지 무효화(무적 발동)", false면 "그냥 맞음"
    public bool OnPlayerTakeDamage()
    {
        // [위기 탈출 넘버원]
        // 1. 갖고 있고, 2. 쿨타임이 지났다면
        if (hasEscapeNo1 && Time.time > escapeNo1Cooldown)
        {
            // 쿨타임 30초 갱신
            escapeNo1Cooldown = Time.time + 30f;

            // 버프 및 무적 코루틴 시작
            StartCoroutine(EscapeNo1Routine());

            Debug.Log("위기 탈출 넘버원 발동! (3초 무적 + 버프)");
            return true; // 이번 데미지는 무효화!
        }

        return false; // 아티팩트 발동 안 함 -> 그냥 데미지 입음
    }

    IEnumerator EscapeNo1Routine()
    {
        // 1. 버프 적용
        StatsManager.instance.artifactAtkMult += 0.5f;
        StatsManager.instance.artifactSpeedMult += 0.5f;
        StatsManager.instance.RecalculateStats();

        // 2. [추가] 플레이어에게 3초 무적 부여!
        GameManager.instance.player.SetInvincible(3f);

        // 3초 대기 (버프 지속시간)
        yield return new WaitForSeconds(3f);

        // 3. 버프 해제
        StatsManager.instance.artifactAtkMult -= 0.5f;
        StatsManager.instance.artifactSpeedMult -= 0.5f;
        StatsManager.instance.RecalculateStats();
    }
}


public enum ArtifactID
{
    Caffeine,            // 카페인 수혈
    Hotfix,              // 긴급 핫픽스
    Enlistment,          // 입영통지서
    DoubleCurse,         // 2의 저주
    CompileError,        // 컴파일 에러
    GitConflict,         // 깃허브 충돌
    StackOverflow,       // 스택 오버플로우
    Overthink,           // 고민중독
    BackupServer,        // 백업 서버
    TetrisMaster,        // 테트리스 과제 만점자
    FinalSpecPDF,        // 개발명세서 최종본
    EscapeNumberOne,     // 위기 탈출 넘버원
    HanwhaFan,           // 극성 한화팬
    SmokingBooth,        // 흡연부스
    Firewall,            // 방화벽
    RestrainingOrder,    // 접근금지령
    CriticalChain,       // 크리티컬 체인
    DopamineAddict       // 도파민 중독
}

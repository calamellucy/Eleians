using System.Collections.Generic;
using UnityEngine;

// [추가] 엘리트 등장을 예약하는 구조체
[System.Serializable]
public class EliteSpawnEvent
{
    public string note;         // 메모용 (예: "1스테이지 중간보스")
    public float spawnTime;     // 등장 시간 (초)
    public int eliteIndex;      // 엘리트 종류 (0~3)
    [HideInInspector] public bool isSpawned; // 이미 소환됐는지 체크하는 플래그
}

[System.Serializable]
public struct Wave
{
    public string waveName;
    public float startTime;
    public float endTime;
    public float spawnInterval;

    [Header("Spawn Settings")]
    public MonsterType type;
    public int[] spriteIndices;
    public int spawnAmountOnce; // 0이면 1로 취급
}

public class Spawner : MonoBehaviour
{
    [Header("Settings")]
    public SpawnData[] spawnData;

    [Header("Monster Stats Config")]
    public MonsterStats[] normalStats; // 일반 몬스터 스탯 (인덱스 0~3)
    public MonsterStats[] eliteStats;  // 엘리트 몬스터 스탯 (인덱스 0~3)
    public MonsterStats[] towerStats;    // 거점 몬스터 스탯 (1개)

    [Header("Wave Config")]
    public Wave[] waves;
    public Wave towerWave;

    [Header("Elite Schedule")]
    public List<EliteSpawnEvent> eliteEvents; // [추가] 엘리트 편성표 리스트

    [Header("Runtime Check")]
    private Wave currentWave;
    // [중요] 타이머를 2개로 분리!
    private float normalSpawnTimer;
    private float towerSpawnTimer;

    [Header("Difficulty Scaling")]
    public float stageDuration = 180f; // 한 스테이지의 길이 (3분 = 180초)
    [Range(0f, 1f)] public float hpIncreaseRate = 0.3f;  // 스테이지당 체력 30% 증가
    [Range(0f, 1f)] public float dmgIncreaseRate = 0.2f; // 스테이지당 공격력 20% 증가
    [Range(0f, 1f)] public float expIncreaseRate = 0.3f; // 스테이지당 경험치 30% 증가

    [Header("Battle Area")]
    public Vector2 innerMin = new Vector2(-16.0f, -37.0f);
    public Vector2 innerMax = new Vector2(50.0f, 13.0f);

    // private float timer; // [삭제] spawnTimer가 그 역할을 대신함
    // private int currentWaveIndex = -1; // [삭제] 매 프레임 시간 체크하므로 필요 없음

    // [삭제] Awake도 이제 필요 없습니다. (spawnPoints를 안 쓰니까요)
    // private void Awake() { }

    private void OnEnable()
    {
        // 게임 재시작 시 엘리트 소환 기록 초기화
        foreach (var evt in eliteEvents)
        {
            evt.isSpawned = false;
        }
    }
    private void Update()
    {
        if (!GameManager.instance.isLive) return;
        // 보스 페이즈 예외 처리
        if (GameManager.instance.isBossPhase) return;

        // 1. 일반 몬스터 웨이브 로직 (항상 실행)
        UpdateNormalWave();
        // 일반 타이머(normalSpawnTimer)를 넘겨줌
        ProcessWave(currentWave, ref normalSpawnTimer);

        // 2. 거점 몬스터 웨이브 로직 (거점 페이즈일 때만 추가 실행)
        if (GameManager.instance.isTowerPhase)
        {
            // 타워 타이머(towerSpawnTimer)를 넘겨줌
            ProcessWave(towerWave, ref towerSpawnTimer);
        }

        // 3. [추가] 엘리트 스케줄 체크
        CheckEliteSchedule();
    }

    // [추가] 시간이 되면 엘리트 소환
    void CheckEliteSchedule()
    {
        float currentTime = GameManager.instance.gameTime;

        foreach (var evt in eliteEvents)
        {
            // 시간이 됐고, 아직 소환 안 했으면
            if (!evt.isSpawned && currentTime >= evt.spawnTime)
            {
                SpawnElite(evt.eliteIndex);
                evt.isSpawned = true; // 소환 완료 체크
                Debug.Log($"엘리트 소환됨! (Time: {evt.spawnTime}, ID: {evt.eliteIndex})");
            }
        }
    }

    void UpdateNormalWave()
    {
        float currentTime = GameManager.instance.gameTime;

        // 최적화: 현재 웨이브가 아직 유효하다면 굳이 다시 찾지 않음
        if (currentWave.endTime > currentTime && currentWave.startTime <= currentTime)
            return;

        bool found = false;
        foreach (var wave in waves)
        {
            if (currentTime >= wave.startTime && currentTime < wave.endTime)
            {
                currentWave = wave;
                found = true;
                break;
            }
        }

        // 해당하는 웨이브가 없으면 빈 웨이브 (스폰 안 함)
        if (!found) currentWave = new Wave();
    }

    // 타이머 변수를 ref로 받아서 각각 관리
    void ProcessWave(Wave wave, ref float timer)
    {
        if (wave.spawnInterval <= 0) return;

        timer += Time.deltaTime;

        if (timer > wave.spawnInterval)
        {
            timer = 0f;
            SpawnMob(wave);
        }
    }

    // [수정됨] 매개변수로 받은 wave 데이터를 사용하도록 변경
    void SpawnMob(Wave wave)
    {
        // 1. 몬스터 종류 데이터 체크
        if (wave.spriteIndices == null || wave.spriteIndices.Length == 0) return;

        // 2. 위치 선정
        Vector3 spawnPos = Vector3.zero;
        bool found = false;

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = GetRandomPosOnViewport();
            if (IsInsideBattleArea(randomPos))
            {
                spawnPos = randomPos;
                found = true;
                break;
            }
        }

        if (!found) return; // 위치 못 잡으면 이번엔 스킵

        // 3. 몬스터 선택 (넘겨받은 wave의 indices를 써야 함!)
        // [버그 수정] currentWave -> wave 로 변경
        int randIndex = Random.Range(0, wave.spriteIndices.Length);
        int targetSpriteIndex = wave.spriteIndices[randIndex];

        // 4. 실체화 (역시 wave의 정보를 넘겨줌)
        SpawnProcess(wave, wave.type, targetSpriteIndex, spawnPos);
    }

    // [수정됨] wave.spawnAmountOnce 처리를 위해 wave 매개변수 추가 (혹은 int amount 직접 전달)
    public void SpawnProcess(Wave wave, MonsterType type, int spriteIndex, Vector3 pos)
    {
        int prefabIndex = GetPoolIndexByType(type, spriteIndex);
        // ★ 타입에 맞는 스탯 가져오기
        MonsterStats stats = new MonsterStats();

        if (type == MonsterType.Normal)
            stats = normalStats[spriteIndex];
        else if (type == MonsterType.Tower)
            stats = towerStats[spriteIndex];

        // 2. ★★★ 스탯 강화 (여기 한 줄만 추가하면 끝!) ★★★
        stats = GetScaledStats(stats);

        // spawnAmountOnce가 0일 수도 있으니 최소 1로 보정
        int count = Mathf.Max(1, wave.spawnAmountOnce);

        for (int i = 0; i < count; i++)
        {
            Vector3 finalPos = pos;

            // 여러 마리일 경우 흩뿌리기
            if (count > 1) finalPos += (Vector3)Random.insideUnitCircle * 1.5f;

            if (!IsInsideBattleArea(finalPos)) finalPos = pos;

            GameObject enemy = GameManager.instance.pool.Get(prefabIndex);
            enemy.transform.position = finalPos;

            var monster = enemy.GetComponent<NormalMonster>();
            if (monster != null)
            {
                // ★ Init에 stats 전달
                monster.Init(stats, type);
            }
        }
    }

    // [수정됨] 엘리트도 뷰포트 랜덤 위치에서 나오게 변경
    public void SpawnElite(int spriteIndex)
    {
        Vector3 spawnPos = Vector3.zero;
        bool found = false;

        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = GetRandomPosOnViewport();
            if (IsInsideBattleArea(randomPos))
            {
                spawnPos = randomPos;
                found = true;
                break;
            }
        }

        if (found)
        {
            // 엘리트는 Wave 정보가 없으므로 가짜 Wave 정보를 만들거나 
            // SpawnProcess 오버로딩을 만들어서 처리. 
            // 여기서는 직접 구현부를 복사해서 단순화함.

            int prefabIndex = GetPoolIndexByType(MonsterType.Elite, spriteIndex);
            SpawnData data = GetSpawnData(MonsterType.Elite);
            MonsterStats stats = eliteStats[spriteIndex];
            // 2. ★★★ 스탯 강화 ★★★
            stats = GetScaledStats(stats);

            GameObject enemy = GameManager.instance.pool.Get(prefabIndex);
            enemy.transform.position = spawnPos;

            var monster = enemy.GetComponent<NormalMonster>();
            if (monster != null)
            {
                monster.Init(stats, MonsterType.Elite);
            }
        }
    }

    Vector3 GetRandomPosOnViewport()
    {
        Camera cam = Camera.main;
        int side = Random.Range(0, 4);
        Vector2 spawnPoint = Vector2.zero;

        switch (side)
        {
            case 0: spawnPoint = new Vector2(Random.value, 1.1f); break; // 상
            case 1: spawnPoint = new Vector2(Random.value, -0.1f); break; // 하
            case 2: spawnPoint = new Vector2(-0.1f, Random.value); break; // 좌
            case 3: spawnPoint = new Vector2(1.1f, Random.value); break; // 우
        }

        Vector3 worldPos = cam.ViewportToWorldPoint(spawnPoint);
        worldPos.z = 0;
        return worldPos;
    }

    bool IsInsideBattleArea(Vector3 pos)
    {
        return (pos.x >= innerMin.x && pos.x <= innerMax.x &&
                pos.y >= innerMin.y && pos.y <= innerMax.y);
    }

    SpawnData GetSpawnData(MonsterType type)
    {
        foreach (var data in spawnData)
        {
            if (data.monsterType == type) return data;
        }
        return spawnData[0];
    }

    int GetPoolIndexByType(MonsterType type, int spriteIndex)
    {
        switch (type)
        {
            case MonsterType.Normal: return 0;
            case MonsterType.Tower:
                // 0번(근거리)이면 3번 풀, 1번(원거리)이면 4번 풀 반환 (예시)
                if (spriteIndex == 0) return 3;
                else return 12;
            case MonsterType.Elite: return 11;
            default: return 0;
        }
    }

    // ★★★ [추가] 스탯을 뻥튀기 해주는 도우미 함수 ★★★
    MonsterStats GetScaledStats(MonsterStats baseStats)
    {
        // 1. 현재 스테이지 계산 (0, 1, 2...)
        // 게임 시간이 흐름에 따라 자동으로 스테이지가 올라감
        int currentStage = Mathf.FloorToInt(GameManager.instance.gameTime / stageDuration);

        // 2. 구조체 복사 (원본 데이터 오염 방지)
        MonsterStats newStats = baseStats;

        // 3. 배율 적용
        // 예: 2스테이지(Index 2)면 -> 1 + (2 * 0.3) = 1.6배 (60% 증가)
        float hpMultiplier = 1f + (currentStage * hpIncreaseRate);
        float dmgMultiplier = 1f + (currentStage * dmgIncreaseRate);

        newStats.maxHealth *= hpMultiplier;
        newStats.damage *= dmgMultiplier;

        // ★★★ [추가] 경험치도 스테이지마다 30%씩 더 줌!
        // (정수로 변환)
        newStats.exp = Mathf.RoundToInt(baseStats.exp * (1f + (currentStage * expIncreaseRate)));

        return newStats;
    }
}

public enum MonsterType
{
    Normal,
    Tower,
    Elite
}

[System.Serializable]
public class SpawnData
{
    public MonsterType monsterType;
    public int spriteCount;
    public float spawnTime;
    public int health;
    public float speed;
    public float damage;
}

// 1. 내성 정보 (어떤 속성을 얼마나 막을지)
[System.Serializable]
public struct Resistance
{
    public ElementType element;      // 저항할 속성 (예: Earth)
    [Range(0f, 1f)]
    public float damageReduction;    // 데미지 감소율 (0.3 = 30% 감소)
    public bool ignoreCC;            // 상태이상(CC) 무시 여부
}

// 2. 몬스터 스탯 정보 (HP, 공격력, 내성 등)
[System.Serializable]
public struct MonsterStats
{
    public string name;              // 에디터 식별용 이름 (예: "나무더지")
    public RuntimeAnimatorController animatorController;
    public int patternId;            // 엘리트용 패턴 아이디
    public int exp;                // 경험치
    public float maxHealth;          // 최대 체력
    public float damage;             // 공격력
    public float speed;              // 이동 속도
    public Resistance resistance;    // 이 몬스터의 내성 정보
}
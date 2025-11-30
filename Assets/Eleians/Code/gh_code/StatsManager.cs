using System;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager instance;

    /*
    [Header("Player's Stats")]
    public float Attack;
    public float HP;
    public float MovementSpeed;
    public float AttackSpeed;
    public float CritChance;
    public float CritDamage;
    public int Level;
    */

    [Header("Final Stats (Read Only)")]
    public float Attack;
    public float MaxHP; // MaxHP 분리 필요
    public float MovementSpeed;
    public float AttackSpeed;
    public float CritChance;
    public float CritDamage;
    public float DamageTakenMultiplier = 1f; // 받는 피해량 계수
    public float ReflectDamage = 0f; // 반사 데미지

    [Header("Base Stats")]
    public float baseAttack = 100;
    public float baseMaxHP = 100;
    public float baseMoveSpeed = 3;
    public float baseAttackSpeed = 1;
    public float baseCritChance = 0.05f;
    public float baseCritDamage = 2.0f;

    // 아티팩트로 인한 추가 능력치 (퍼센트)
    [HideInInspector] public float artifactAtkMult = 0f;      // 0.5면 50% 증가
    [HideInInspector] public float artifactSpeedMult = 0f;
    [HideInInspector] public float artifactAtkSpdMult = 0f;
    [HideInInspector] public float artifactCritChanceAdd = 0f;
    [HideInInspector] public float artifactCritDmgAdd = 0f;
    [HideInInspector] public float artifactDmgTakenMult = 0f; // -0.4면 40% 감소

    [Header("Elements Count")]
    public int ElectricCnt;
    public int FireCnt;
    public int IceCnt;
    public int EarthCnt;


    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        // ResetToBase();
        RecalculateStats(); // 시작할 때 계산
    }

    void Update()
    {
        // 레벨업 등으로 원소 카운트가 바뀌면 재계산이 필요하므로
        // GameManager에서 레벨업 할 때 RecalculateStats()를 호출하는 게 정석이지만
        // 편의상 여기서 계속 갱신해도 됩니다. (단, 아래 로직으로)
        RecalculateStats();
    }

    public void RecalculateStats()
    {
        // 1. 기본 + 원소 스탯 계산
        float elemAttack = baseAttack + FireCnt * 4f;
        float elemHP = baseMaxHP + IceCnt * 8f;
        float elemAtkSpd = baseAttackSpeed + ElectricCnt * 0.08f;
        float elemCrit = baseCritChance + EarthCnt * 0.015f;

        // 2. 아티팩트 보정치 적용 (여기가 핵심)
        Attack = elemAttack * (1f + artifactAtkMult);
        MaxHP = elemHP; // HP는 현재 체력이 아니라 최대 체력이 변하는 것
        AttackSpeed = elemAtkSpd * (1f + artifactAtkSpdMult);
        MovementSpeed = baseMoveSpeed * (1f + artifactSpeedMult);
        CritChance = Mathf.Clamp01(elemCrit + artifactCritChanceAdd);
        CritDamage = baseCritDamage + artifactCritDmgAdd;

        DamageTakenMultiplier = 1f + artifactDmgTakenMult;
    }

    /*
    public void ResetToBase()
    {
        Attack = 100;
        HP = 100;
        MovementSpeed = 3;
        AttackSpeed = 1;
        CritChance = Mathf.Clamp01(0.05f);
        CritDamage = 2;
        Level = 0;

        FireCnt = IceCnt = ElectricCnt = 0;
        EarthCnt = 0;
    }

    // 레벨업 → 원소 선택 시 호출
    void Update()
    {
        Level = GameManager.instance.level;
        Attack = 100 + FireCnt * 4f; 
        HP = 100 + IceCnt * 8f; 
        AttackSpeed = 1 + ElectricCnt * 0.08f; 
        CritChance = Mathf.Clamp01(0.05f + EarthCnt*0.015f);
    }
    */

    // 편의 함수들
    public float GetAttackPeriod() => 1f / Mathf.Max(0.01f, AttackSpeed);
    public bool RollCrit() => UnityEngine.Random.value < CritChance;
    public float ApplyCrit(float damage) => RollCrit() ? damage * CritDamage : damage;
}

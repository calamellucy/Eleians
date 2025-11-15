using System;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
    public static StatsManager instance;

    [Header("Player's Stats")]
    public float Attack;
    public float HP;
    public float MovementSpeed;
    public float AttackSpeed;
    public float CritChance;
    public float CritDamage;
    public int Level;

    // 원소 선택 누적 카운트(원하면 UI에 노출)
    [Header("Elements Count")]
    public int ElectricCnt;
    public int FireCnt;
    public int IceCnt;
    public int EarthCnt;


    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        ResetToBase();
    }

    public void ResetToBase()
    {
        Attack = 100;
        HP = 100;
        MovementSpeed = 3;
        AttackSpeed = 1;
        CritChance = Mathf.Clamp01(0.05f);
        CritDamage = 2;
        Level = 0;

        FireCnt = IceCnt = ElectricCnt = EarthCnt = 0;
    }

    // 레벨업 → 원소 선택 시 호출
    public void LevelUpdate()
    {
        Level = GameManager.instance.level;
        Attack = 100 + FireCnt * 4f; 
        HP = 100 + IceCnt * 8f; 
        AttackSpeed = 1 + ElectricCnt * 0.08f; 
        CritChance = Mathf.Clamp01(0.05f + EarthCnt*0.015f);
    }

    // 편의 함수들
    public float GetAttackPeriod() => 1f / Mathf.Max(0.01f, AttackSpeed);
    public bool RollCrit() => UnityEngine.Random.value < CritChance;
    public float ApplyCrit(float damage) => RollCrit() ? damage * CritDamage : damage;
}

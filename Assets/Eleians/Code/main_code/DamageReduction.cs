using UnityEngine;

public class DamageReduction : MonoBehaviour
{
    public static DamageReduction instance;

    public bool IsIceShield = false;

    void Start()   // Awake → Start
    {
        instance = this;
    }

    public float ProcessDamage(float dmg)
    {
        if (IsIceShield)
            return dmg * 0.2f;
        return dmg;
    }
}

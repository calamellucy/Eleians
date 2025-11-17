using UnityEngine;

public class Seori_Shuri : MonoBehaviour
{
    public float damage;
    public int per;
    public float slowRate;

    public void Init(float damage, int per, float slowRate)
    {
        this.damage = damage;
        this.per = per;
        this.slowRate = slowRate;
    }
}

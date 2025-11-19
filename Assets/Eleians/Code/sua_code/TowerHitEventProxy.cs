using UnityEngine;

public class TowerHitEventProxy : MonoBehaviour
{
    public Tower tower;

    public void ShowDust()
    {
        tower.ShowDust();
    }

    public void HideDust()
    {
        tower.HideDust();
    }
}

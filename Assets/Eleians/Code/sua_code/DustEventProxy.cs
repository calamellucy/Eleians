using UnityEngine;

public class DustEventProxy : MonoBehaviour
{
    public Tower tower;

    public void HideDust()
    {
        tower.HideDust();
    }

    public void ShowDust()
    {
        tower.ShowDust();
    }
}

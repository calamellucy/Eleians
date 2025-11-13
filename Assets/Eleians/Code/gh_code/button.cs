using UnityEngine;

public class button : MonoBehaviour
{
    public int type;
    public void OnClick()
    {
        switch (type)
        {
            case 0:
                StatsManager.instance.ElectricCnt++;
                break;
            case 1:
                StatsManager.instance.FireCnt++;
                break;
            case 2:
                StatsManager.instance.IceCnt++;
                GameManager.instance.health += 8;
                break;
            case 3:
                StatsManager.instance.EarthCnt++;
                break;
        }
        StatsManager.instance.LevelUpdate();
    }
}

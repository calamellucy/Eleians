using UnityEngine;
using UnityEngine.UI;


public class HUD : MonoBehaviour
{
    public enum InfoType {Exp, Level, Time, Health, Electric, Fire, Ice, Earth}
    public InfoType type;
    Text myText;
    Slider mySlider;

    void Awake()
    {
        myText = GetComponent<Text>();
        mySlider = GetComponent<Slider>();

    }

    void LateUpdate()
    {
        switch (type)
        {
            case InfoType.Exp:
                float curExp = GameManager.instance.exp;
                float maxExp = GameManager.instance.nextExp[GameManager.instance.level];
                mySlider.value = curExp/maxExp; 
                break;
            case InfoType.Level:
                myText.text = string.Format("Lv.{0:F0}", GameManager.instance.level); 
                break;
            case InfoType.Electric:
                myText.text = string.Format("{0:F0}", StatsManager.instance.ElectricCnt);
                break;
            case InfoType.Fire:
                myText.text = string.Format("{0:F0}", StatsManager.instance.FireCnt);

                break;
            case InfoType.Ice:
                myText.text = string.Format("{0:F0}", StatsManager.instance.IceCnt);

                break;
            case InfoType.Earth:
                myText.text = string.Format("{0:F0}", StatsManager.instance.EarthCnt);
                break;
            case InfoType.Time:
                float spendedTime = GameManager.instance.gameTime;
                int min = Mathf.FloorToInt(spendedTime / 60);
                int sec = Mathf.FloorToInt(spendedTime % 60);
                myText.text = string.Format("{0:D2}:{1:D2}", min, sec);
                break;
            case InfoType.Health:
                float curHP = GameManager.instance.health;
                float maxHP = StatsManager.instance.HP;
                mySlider.value = curHP / maxHP;
                break;

        }
    }
}

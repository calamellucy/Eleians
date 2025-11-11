using UnityEngine;
using UnityEngine.UI;


public class HUD : MonoBehaviour
{
    public enum InfoType {Exp, Level, Kill, Time, Health}
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
            case InfoType.Kill:

                break;
            case InfoType.Time:
                float spendedTime = GameManager.instance.gameTime;
                int min = Mathf.FloorToInt(spendedTime / 60);
                int sec = Mathf.FloorToInt(spendedTime % 60);
                myText.text = string.Format("{0:D2}:{1:D2}", min, sec);
                break;
            case InfoType.Health:
                float curHP = GameManager.instance.health;
                float maxHP = GameManager.instance.maxHealth;
                mySlider.value = curHP / maxHP;
                break;

        }
    }
}

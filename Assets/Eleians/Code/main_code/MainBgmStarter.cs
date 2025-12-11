using UnityEngine;
public class MainBgmStarter : MonoBehaviour
{
    void Start()
    {
        AudioManager.instance.PlayBgm(AudioManager.Bgm.Main);
    }
}
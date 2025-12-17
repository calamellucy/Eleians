using UnityEngine;

public class EventClear : MonoBehaviour
{
    private int lastTime = -1; // 마지막으로 체크한 시간 (중복 실행 방지용)

    [Header("메시지 설정")]
    public string artifactMessage = "거점 수호 성공! 아티팩트를 획득하세요!";
    public string bossWarningMessage = "중앙에서 어둠의 기운이 진동합니다";
    public string defendMessage = "원소 거점을 수호하세요!";

    private void Start()
    {
        AudioManager.instance.PlayBgm(AudioManager.Bgm.Base);
    }

    void Update()
    {
        // 게임 매니저가 없으면 실행 안 함
        if (GameManager.instance == null) return;

        // gameTime을 정수(초 단위)로 변환
        int currentTime = Mathf.FloorToInt(GameManager.instance.gameTime);

        // 시간이 1초 지났을 때만 로직 실행 (매 프레임 실행 방지)
        if (currentTime != lastTime)
        {
            CheckEventTime(currentTime);
            lastTime = currentTime;
        }
    }

    void CheckEventTime(int time)
    {
        // 1. 거점 수호 성공 메시지 (180, 360, 540, 720초)
        if (time == 180 || time == 360 || time == 540 || time == 720)
        {
            ShowMessage(artifactMessage);
            if(time<700)
                AudioManager.instance.TurnOffAudio(1f, AudioManager.Bgm.Base);
            else
                AudioManager.instance.TurnOffAudio(1f, AudioManager.Bgm.Null);
        }
        else if(time == 150 || time == 330 || time == 510 || time == 690)
        {
            ShowMessage(defendMessage);
        }
        else if (time == 149 || time == 329 || time == 519 || time == 689)
        {
            AudioManager.instance.TurnOffAudio(1f, AudioManager.Bgm.Battle);
        }
        // 2. 보스 등장 예고 메시지 (722초)
        else if (time == 721)
        {
           
            ShowMessage(bossWarningMessage);
        }
    }

    void ShowMessage(string msg)
    {
        if (SkillAlertSystem.instance != null)
        {
            SkillAlertSystem.instance.EnqueueMessage(msg);
        }
        else
        {
            Debug.LogWarning("SkillAlertSystem이 씬에 없습니다! 메시지: " + msg);
        }
    }
}
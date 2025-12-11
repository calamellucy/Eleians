using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScenesMove : MonoBehaviour
{
    public void GameScenesCtrl()
    {
        SceneManager.LoadScene("IntroScene"); //어떤 씬 이름으로 이동할지
        AudioManager.instance.TurnOffAudio(0.5f, AudioManager.Bgm.Cutscene);

    }

    // 버튼에 연결할 함수
    public void OnExitClick()
    {
        // 1. 유니티 에디터에서 실행 중일 때
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // 2. 실제 빌드된 게임(exe 파일 등)에서 실행 중일 때
        Application.Quit();
#endif
    }

}

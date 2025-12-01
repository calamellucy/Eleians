using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // Legacy Text를 쓰기 위해 필수!

public class SkillAlertSystem : MonoBehaviour
{
    [Header("UI Components")]
    public Text alertText; // TextMeshProUGUI 대신 일반 Text 사용
    public CanvasGroup canvasGroup;

    [Header("Settings")]
    public float displayTime = 2.0f;
    public float fadeDuration = 1.0f;

    // --- 내부 감시용 변수 ---
    private int lastFireCnt = 0;
    private int lastIceCnt = 0;
    private int lastElectricCnt = 0;
    private int lastEarthCnt = 0;

    // --- 메시지 대기열 ---
    private Queue<string> messageQueue = new Queue<string>();
    private bool isDisplaying = false;

    void Start()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        // 초기 스탯 동기화
        if (StatsManager.instance != null)
        {
            lastFireCnt = StatsManager.instance.FireCnt;
            lastIceCnt = StatsManager.instance.IceCnt;
            lastElectricCnt = StatsManager.instance.ElectricCnt;
            lastEarthCnt = StatsManager.instance.EarthCnt;
        }
    }

    void Update()
    {
        if (StatsManager.instance == null) return;

        CheckStatChange("Fire", StatsManager.instance.FireCnt, ref lastFireCnt);
        CheckStatChange("Ice", StatsManager.instance.IceCnt, ref lastIceCnt);
        CheckStatChange("Electric", StatsManager.instance.ElectricCnt, ref lastElectricCnt);
        CheckStatChange("Earth", StatsManager.instance.EarthCnt, ref lastEarthCnt);
    }

    void CheckStatChange(string element, int currentCnt, ref int lastCnt)
    {
        if (currentCnt > lastCnt)
        {
            for (int i = lastCnt + 1; i <= currentCnt; i++)
            {
                string msg = GetMilestoneMessage(element, i);
                if (!string.IsNullOrEmpty(msg))
                {
                    EnqueueMessage(msg);
                }
            }
            lastCnt = currentCnt;
        }
    }

    string GetMilestoneMessage(string element, int level)
    {
        string skillName = "";
        switch (element)
        {
            case "Electric": skillName = "전자파"; break;
            case "Fire": skillName = "화염검"; break;
            case "Ice": skillName = "서리표창"; break;
            case "Earth": skillName = "암석난사"; break;
        }

        switch (level)
        {
            case 5: return $"{skillName} 능력 강화!";
            case 10: return $"{skillName} 1차 각성!";
            case 15: return $"{skillName} 액티브 개방!";
            case 20: return $"{skillName} 2차 각성!";
            default: return null;
        }
    }

    void EnqueueMessage(string message)
    {
        messageQueue.Enqueue(message);
        if (!isDisplaying)
        {
            StartCoroutine(DisplayRoutine());
        }
    }

    IEnumerator DisplayRoutine()
    {
        isDisplaying = true;

        while (messageQueue.Count > 0)
        {
            string msg = messageQueue.Dequeue();

            alertText.text = msg;
            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlaySfx(AudioManager.Sfx.PerksAcqui);
            }

            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(displayTime);

            float timer = 0f;
            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            yield return new WaitForSeconds(0.1f);
        }

        isDisplaying = false;
    }
}
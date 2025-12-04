using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public Transform player;
    public Transform tower;

    public float radius = 2.5f;

    // 알림 메시지 내용 설정 (인스펙터에서 수정 가능)
    [TextArea]
    public string phaseStartMessage = "거점 페이즈가 곧 시작합니다!";

    bool active = false;

    void Awake()
    {
        transform.localScale = Vector3.zero;  // 초기에 숨김
    }

    public void Activate(Transform towerTransform)
    {
        tower = towerTransform;
        active = true;
        transform.localScale = Vector3.one;

        // 화살표가 켜질 때 SkillAlertSystem에 메시지 띄우라고 요청
        if (SkillAlertSystem.instance != null)
        {
            SkillAlertSystem.instance.EnqueueMessage(phaseStartMessage);
        }
        else
        {
            Debug.LogWarning("SkillAlertSystem이 씬에 없습니다!");
        }
    }

    public void Deactivate()
    {
        active = false;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (!active || player == null || tower == null) return;

        if (IsTowerVisible())
        {
            transform.localScale = Vector3.zero;
            return;
        }
        else
        {
            transform.localScale = Vector3.one;
        }

        Vector3 dir = (tower.position - player.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.position = player.position + dir * radius;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    bool IsTowerVisible()
    {
        Vector3 screen = Camera.main.WorldToScreenPoint(tower.position);

        return screen.z > 0 &&
               screen.x >= 0 && screen.x <= Screen.width &&
               screen.y >= 0 && screen.y <= Screen.height;
    }
}
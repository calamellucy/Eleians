using UnityEngine;

public class ArrowController : MonoBehaviour
{
    public Transform player;
    public Transform target;

    public float radius = 2.5f;

    bool active = false;
    bool hideWhenOnScreen = true;

    void Awake()
    {
        transform.localScale = Vector3.zero;  // 초기에 숨김
    }

    public void Activate(Transform targetTransform, bool _hideWhenOnScreen = true)
    {
        target = targetTransform;
        hideWhenOnScreen = _hideWhenOnScreen; // 옵션 저장
        active = true;
        transform.localScale = Vector3.one;
    }

    public void Deactivate()
    {
        active = false;
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        if (!active || player == null || target == null) return;

        if (hideWhenOnScreen && IsTowerVisible())
        {
            transform.localScale = Vector3.zero;
            return;
        }
        else
        {
            transform.localScale = Vector3.one;
        }

        // --- 월드 좌표 계산 ---
        Vector3 dir = (target.position - player.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // 위치 설정 (월드 좌표)
        transform.position = player.position + dir * radius;

        // 회전 설정
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    bool IsTowerVisible()
    {
        Vector3 screen = Camera.main.WorldToScreenPoint(target.position);

        return screen.z > 0 &&
               screen.x >= 0 && screen.x <= Screen.width &&
               screen.y >= 0 && screen.y <= Screen.height;
    }
}

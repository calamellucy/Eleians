using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager instance;

    // 프리펩들을 보관할 변수
    public GameObject[] prefabs;
    // 풀 담당을 하는 리스트들
    List<GameObject>[] pools;

    public Transform damageCanvas;

    private void Awake()
    {
        instance = this;

        pools = new List<GameObject>[prefabs.Length];
        // pools의 크기를 prefabs의 크기로 초기화

        for (int index = 0; index < pools.Length; index++)
        {   // 배열의 원소들 초기화
            pools[index] = new List<GameObject>();
        }
    }

    public GameObject Get(int index)
    {
        GameObject select = null;

        // ... 선택한 풀의 놀고 있는 게임오브젝트 접근
        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {   // ... 발견하면 select 변수에 할당
                select = item;
                select.SetActive(true);
                break;
            }
        }

        // ... 못 찾았으면?
        if (select == null)
        {   // ... 새롭게 생성하고 select 변수에 할당
            select = Instantiate(prefabs[index], transform); // 새롭게 생성
            pools[index].Add(select); // pools에 등록
        }

        return select;
    }

    // 데미지 텍스트 출력 함수
    public GameObject ShowDamage(int index, float dmg, Vector3 worldPos, bool isCrit)
    {
        GameObject obj = Get(index);

        if (damageCanvas != null && obj.transform.parent != damageCanvas)
            obj.transform.SetParent(damageCanvas, false);

        obj.transform.position = worldPos;

        var dt = obj.GetComponent<DamageText>();
        if (dt != null)
            dt.SetDamage(dmg, isCrit);

        return obj;
    }


}

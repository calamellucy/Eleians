using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    // ��������� ������ ����
    public GameObject[] prefabs;
    // Ǯ ����� �ϴ� ����Ʈ��
    List<GameObject>[] pools;

    private void Awake()
    {
        pools = new List<GameObject>[prefabs.Length];
        // pools�� ũ�⸦ prefabs�� ũ��� �ʱ�ȭ

        for (int index = 0; index < pools.Length; index++)
        {   // �迭�� ���ҵ� �ʱ�ȭ
            pools[index] = new List<GameObject>();
        }
    }

    public GameObject Get(int index)
    {
        GameObject select = null;

        // ... ������ Ǯ�� ��� �ִ� ���ӿ�����Ʈ ����
        foreach (GameObject item in pools[index])
        {
            if (!item.activeSelf)
            {   // ... �߰��ϸ� select ������ �Ҵ�
                select = item;
                select.SetActive(true);
                break;
            }
        }

        // ... �� ã������?
        if (select == null)
        {   // ... ���Ӱ� �����ϰ� select ������ �Ҵ�
            select = Instantiate(prefabs[index], transform); // ���Ӱ� ����
            pools[index].Add(select); // pools�� ���
        }

        return select;
    }

}

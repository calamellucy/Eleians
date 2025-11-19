using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아티팩트 전체 관리 시스템
/// - 아티팩트 정보 저장
/// - 선택 UI에서 선택한 아티팩트 적용
/// - 중복 체크
/// - 스탯 변화 반영까지 책임
/// </summary>
public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager instance;

    private void Awake()
    {
        instance = this;
    }

    // 아티팩트 데이터 목록
    public List<ArtifactData> allArtifacts;

    // 플레이어가 이미 획득한 아티팩트
    private List<ArtifactData> ownedArtifacts = new List<ArtifactData>();

    /// <summary> 랜덤 3개 아티팩트 뽑기 </summary>
    public List<ArtifactData> GetRandomArtifacts(int count = 3)
    {
        List<ArtifactData> result = new List<ArtifactData>();
        List<ArtifactData> pool = new List<ArtifactData>(allArtifacts);

        // 이미 가진 건 제외하고 싶다면 ↓ 여기에 필터
        pool.RemoveAll(a => ownedArtifacts.Contains(a));

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }

    /// <summary> 아티팩트 획득 처리 </summary>
    public void AcquireArtifact(ArtifactData data)
    {
        if (ownedArtifacts.Contains(data))
        {
            Debug.Log($"{data.artifactName} 이미 보유 중");
            return;
        }

        ownedArtifacts.Add(data);

        // 슬롯 아이콘 추가
        ArtifactSlotUI.instance.AddArtifactIcon(data.icon);

        ApplyArtifact(data);
    }

    /// <summary> 아티팩트 효과 적용 (실제 구현은 각 아티팩트 클래스가 담당) </summary>
    private void ApplyArtifact(ArtifactData data)
    {
        switch (data.id)
        {
            case ArtifactID.Enlistment:
                // TODO: [입영통지서] 효과 구현
                break;

            case ArtifactID.DoubleCurse:
                // TODO: [2의 저주] 효과 구현
                break;

            case ArtifactID.CompileError:
                // TODO: [컴파일 에러] 효과 구현
                break;

            case ArtifactID.GitConflict:
                // TODO: [깃허브 충돌] 효과 구현
                break;

            case ArtifactID.StackOverflow:
                // TODO: [스택 오버플로우] 효과 구현
                break;

            case ArtifactID.Overthink:
                // TODO: [고민중독] 효과 구현
                break;

            case ArtifactID.TetrisMaster:
                // TODO: [테트리스 과제 만점자] 효과 구현
                break;

            case ArtifactID.FinalSpecPDF:
                // TODO: 개발명세서 최종본 효과
                break;

            case ArtifactID.EscapeNumberOne:
                // TODO: 위기 탈출 넘버원 효과
                break;

            case ArtifactID.HanwhaFan:
                // TODO: 극성 한화팬 효과
                break;

            case ArtifactID.SmokingBooth:
                // TODO: 흡연부스 효과
                break;

            case ArtifactID.RestrainingOrder:
                // TODO: 접근금지령 효과
                break;

            case ArtifactID.CriticalChain:
                // TODO: 크리티컬 체인 효과
                break;

            case ArtifactID.DopamineAddict:
                // TODO: 도파민 중독 효과
                break;

            default:
                Debug.LogWarning("정의되지 않은 아티팩트 ID");
                break;
        }
    }
}


public enum ArtifactID
{
    Enlistment,          // 입영통지서
    DoubleCurse,         // 2의 저주
    CompileError,        // 컴파일 에러
    GitConflict,         // 깃허브 충돌
    StackOverflow,       // 스택 오버플로우
    Overthink,           // 고민중독
    TetrisMaster,        // 테트리스 과제 만점자
    FinalSpecPDF,        // 개발명세서 최종본
    EscapeNumberOne,     // 위기 탈출 넘버원
    HanwhaFan,           // 극성 한화팬
    SmokingBooth,        // 흡연부스
    RestrainingOrder,    // 접근금지령
    CriticalChain,       // 크리티컬 체인
    DopamineAddict       // 도파민 중독
}

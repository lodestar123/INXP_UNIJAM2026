using System;
using UnityEngine;

/// <summary>
/// 보드 채우기 진행 위치를 관리하는 클래스
/// 7×7 보드에서 현재 몇 번째 칸까지 채워졌는지 추적합니다.
/// </summary>
[Serializable]
public class BoardFillCursor
{
    [SerializeField] private int _currentIndex = 0;

    private const int BoardSize = 7 * 7; // 선형 인덱스 0~48 = 칸, 커서 49 = 더 채울 칸 없음(IsBoardFull)

    /// <summary>
    /// 큐 기준 채우기 순서(왼쪽 아래 → 오른쪽 → 위 줄)의 선형 인덱스를 타일 그리드 (x, y)로 변환합니다.
    /// </summary>
    public static (int x, int y) FillOrderIndexToCell(int linearIndex, int width, int height)
    {
        int rowFromBottom = linearIndex / width;
        int x = linearIndex % width;
        int y = height - 1 - rowFromBottom;
        return (x, y);
    }

    /// <summary>
    /// 다음에 채울 선형 인덱스(0부터). 값은 0~49 범위로 클램프되며, 가득 참은 <see cref="IsBoardFull"/>로 판별합니다.
    /// </summary>
    public int CurrentIndex 
    {
        get => _currentIndex;
        private set => _currentIndex = Mathf.Clamp(value, 0, BoardSize);
    }

    /// <summary>
    /// 보드가 완전히 채워졌는지 확인
    /// </summary>
    public bool IsBoardFull => CurrentIndex >= BoardSize;

    /// <summary>
    /// 남은 칸 수
    /// </summary>
    public int RemainingSlots => Mathf.Max(0, BoardSize - CurrentIndex);

    /// <summary>
    /// 커서를 다음 위치로 이동
    /// </summary>
    public void MoveNext(int count = 1)
    {
        CurrentIndex += count;
    }

    /// <summary>
    /// 커서를 특정 위치로 설정
    /// </summary>
    public void SetPosition(int index)
    {
        CurrentIndex = index;
    }

    /// <summary>
    /// 커서 초기화 (0으로 리셋)
    /// </summary>
    public void Reset()
    {
        CurrentIndex = 0;
    }

    /// <summary>
    /// 인덱스를 좌표로 변환
    /// </summary>
    /// <param name="index">인덱스 (0~48)</param>
    /// <param name="width">보드 너비</param>
    /// <returns>(x, y) 좌표</returns>
    public static (int x, int y) IndexToPosition(int index, int width)
    {
        int x = index % width;
        int y = index / width;
        return (x, y);
    }

    /// <summary>
    /// 좌표를 인덱스로 변환
    /// </summary>
    /// <param name="x">X 좌표</param>
    /// <param name="y">Y 좌표</param>
    /// <param name="width">보드 너비</param>
    /// <returns>인덱스</returns>
    public static int PositionToIndex(int x, int y, int width)
    {
        return y * width + x;
    }

    /// <summary>
    /// 디버그용: 현재 상태 출력
    /// </summary>
    public void DebugPrint()
    {
        Debug.Log($"[BoardFillCursor] 현재 위치: {CurrentIndex}/49, 남은 칸: {RemainingSlots}, 완료: {IsBoardFull}");
    }
}

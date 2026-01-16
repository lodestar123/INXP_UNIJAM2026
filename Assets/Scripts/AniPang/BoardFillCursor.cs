using System;
using UnityEngine;

/// <summary>
/// 보드 채우기 진행 위치를 관리하는 클래스
/// 7×7 보드에서 현재 몇 번째 칸까지 채워졌는지 추적합니다.
/// </summary>
[Serializable]
public class BoardFillCursor
{
    [SerializeField] private int _currentIndex = 0; // 현재 채워진 위치 (0~48)
    
    private const int BoardSize = 7 * 7; // 49칸

    /// <summary>
    /// 현재 커서 위치 (0~48)
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

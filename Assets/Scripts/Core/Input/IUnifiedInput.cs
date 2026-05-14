using UnityEngine;

namespace Core.Input
{
    // 두 게임(플래피 버드, 애니팡)이 공통으로 사용할 입력 인터페이스
    public interface IUnifiedInput
    {
        // 현재 화면을 누르고 있는지 여부
        bool IsPressing { get; }

        // 이번 프레임에 막 눌렀는지 여부
        bool WasTappedThisFrame { get; }

        bool WasReleasedThisFrame { get; }

        // 현재 포인터(마우스/터치)의 스크린 좌표
        Vector2 PointerPosition { get; }
    }
}

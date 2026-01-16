using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace Core.Input
{
    public class UnifiedInputManager : Singleton<UnifiedInputManager>, IUnifiedInput
    {
        // 현재 입력을 누르고 있는지 확인하는 프로퍼티
        public bool IsPressing => Pointer.current != null && Pointer.current.press.isPressed;

        // 이번 프레임에 탭(클릭)했는지 확인하는 프로퍼티
        public bool WasTappedThisFrame => Pointer.current != null && Pointer.current.press.wasPressedThisFrame;

        // 포인터 위치를 반환하는 프로퍼티
        public Vector2 PointerPosition => Pointer.current == null ? Vector2.zero : Pointer.current.position.ReadValue();

        // 초기화 로직이 필요하다면 여기에 작성
        protected override void Awake()
        {
            base.Awake();
            // 씬 전환 시 파괴되지 않도록 설정 (필요 시)
            // DontDestroyOnLoad(gameObject);
        }
    }
}

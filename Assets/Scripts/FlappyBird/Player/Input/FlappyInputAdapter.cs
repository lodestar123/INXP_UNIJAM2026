using Core.Input;
using FlappyBird.Interfaces.Player;
using UnityEngine;

namespace FlappyBird.Player.Input
{
    // 플래피 버드 플레이어가 이해할 수 있는 형태(IBirdInputSource)로 변환해주는 어댑터 클래스입니다.
    public class FlappyInputAdapter : MonoBehaviour, IBirdInputSource
    {
        // 통합 입력 시스템에 대한 참조 (인터페이스 의존)
        private IUnifiedInput _globalInput;

        private void Start()
        {
            // 싱글톤 인스턴스 할당 (또는 DI 컨테이너 사용 가능)
            _globalInput = UnifiedInputManager.Instance;

            if (_globalInput == null)
            {
                Debug.LogError("FlappyInputAdapter: UnifiedInputManager를 찾을 수 없습니다.");
            }
        }

        // IBirdInputSource 인터페이스 구현
        // 플래피 버드는 '누르고 있는 상태'를 비행 신호로 사용합니다.
        public bool IsHolding => _globalInput != null && _globalInput.IsPressing;
    }
}

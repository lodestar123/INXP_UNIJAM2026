using Core.Input;
using FlappyBird.Interfaces.Player;
using UnityEngine;

namespace FlappyBird.Player.Input
{
    /// <summary>
    /// 통합 입력 값을 플래피버드 입력 형식으로 전달합니다.
    /// </summary>
    public class FlappyInputAdapter : MonoBehaviour, IBirdInputSource
    {
        // 통합 입력 소스 참조
        private IUnifiedInput _globalInput;

        private void Start()
        {
            // 통합 입력 인스턴스 조회
            _globalInput = UnifiedInputManager.Instance;

            if (_globalInput == null)
            {
                Debug.LogError("FlappyInputAdapter: UnifiedInputManager를 찾을 수 없습니다.");
            }
        }

        // 누르고 있는 동안 상승 입력으로 처리
        public bool IsHolding => _globalInput != null && _globalInput.IsPressing;
    }
}

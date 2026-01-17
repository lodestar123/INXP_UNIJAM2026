using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class NumberCounter : MonoBehaviour
    {
        private TextMeshProUGUI _textComponent;
        private Coroutine _currentCoroutine;

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        // 즉시 값을 설정하는 메서드 (초기화/리셋용)
        public void SetValue(int value)
        {
            if (_textComponent == null) _textComponent = GetComponent<TextMeshProUGUI>();
            
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _textComponent.text = value.ToString("N0");
        }

        // 애니메이션 재생 메서드 (점수 획득용)
        public void PlayCountAnimation(int startValue, int targetValue, float duration)
        {
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(CountRoutine(startValue, targetValue, duration));
        }

        private IEnumerator CountRoutine(int startValue, int targetValue, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                
                // EaseOutExpo: 끝에 갈수록 부드럽게 멈추는 공식 (선택사항)
                // progress = progress == 1 ? 1 : 1 - Mathf.Pow(2, -10 * progress);

                float currentValue = Mathf.Lerp(startValue, targetValue, progress);
                _textComponent.text = ((int)currentValue).ToString("N0");

                yield return null;
            }

            _textComponent.text = targetValue.ToString("N0");
            _currentCoroutine = null;
        }
    }
}
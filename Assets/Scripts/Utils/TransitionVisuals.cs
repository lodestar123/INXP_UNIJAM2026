using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using URPGlitch;

namespace Utils
{
    public class TransitionVisuals : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private Volume pastVolume;
        [SerializeField] private Volume presentVolume;
        [SerializeField] private float defaultDuration = 0.8f; // 변수명 명확화
        
        private DigitalGlitchVolume _digitalGlitch;
        private AnalogGlitchVolume _analogGlitch;

        private void Awake()
        {
            if (pastVolume == null)
            {
                Debug.LogError("TransitionVisuals: Volume이 할당되지 않았습니다.", this);
                return;
            }

            if (!pastVolume.profile.TryGet(out _digitalGlitch))
            {
                Debug.LogWarning("프로파일에서 DigitalGlitchVolume을 찾을 수 없습니다.");
            }

            if (!pastVolume.profile.TryGet(out _analogGlitch))
            {
                Debug.LogWarning("프로파일에서 AnalogGlitchVolume을 찾을 수 없습니다.");
            }
            
            // 초기화
            if (_digitalGlitch != null) _digitalGlitch.intensity.value = 0f;
            if (_analogGlitch != null) _analogGlitch.verticalJump.value = 0f;
        }

        public Tween PlayStartDigitalGlitchAnimation(float targetValue)
        {
            if (_digitalGlitch is null) return DOTween.Sequence();
            
            return DOTween.To(() => _digitalGlitch.intensity.value, 
                x => _digitalGlitch.intensity.value = x, targetValue, defaultDuration)
                .SetEase(Ease.InOutCubic);
        }

        public Tween PlayStartAnalogGlitchAnimation(float targetValue)
        {
            if (_analogGlitch is null) return DOTween.Sequence();

            return DOTween.To(() => _analogGlitch.verticalJump.value, 
                x => _analogGlitch.verticalJump.value = x, targetValue, defaultDuration)
                .SetEase(Ease.InOutCubic);
        }

        /// <summary>
        /// 디지털 글리치 종료 애니메이션을 반환합니다.
        /// </summary>
        public Tween PlayEndDigitalGlitchAnimation(float targetValue)
        {
            if (_digitalGlitch is null) return DOTween.Sequence();

            return DOTween.To(() => _digitalGlitch.intensity.value, 
                x => _digitalGlitch.intensity.value = x, targetValue, defaultDuration)
                .SetEase(Ease.InOutCubic);
        }
        
        /// <summary>
        /// 아날로그 글리치 종료 애니메이션을 반환합니다.
        /// </summary>
        public Tween PlayEndAnalogGlitchAnimation(float targetValue)
        {
            if (_analogGlitch is null) return DOTween.Sequence();

            return DOTween.To(() => _analogGlitch.verticalJump.value, 
                x => _analogGlitch.verticalJump.value = x, targetValue, defaultDuration)
                .SetEase(Ease.InOutCubic);
        }
        
        /// <summary>
        /// 디지털과 아날로그 글리치를 동시에 섞어서 시작합니다.
        /// </summary>
        /// <param name="digitalIntensity">디지털 글리치 강도 (0~1)</param>
        /// <param name="analogIntensity">아날로그 글리치 강도 (0~1)</param>
        public Tween PlayStartMixedGlitch(float digitalIntensity, float analogIntensity)
        {
            Sequence sequence = DOTween.Sequence();

            // 두 효과를 동시에 실행 (Join)
            // 디지털은 보통 0.5~0.8 정도면 적당하고, 아날로그는 1.0까지 올려도 좋습니다.
            if (_digitalGlitch)
            {
                sequence.Join(DOTween.To(() => _digitalGlitch.intensity.value, 
                        x => _digitalGlitch.intensity.value = x, digitalIntensity, defaultDuration)
                    .SetEase(Ease.InOutCubic));
            }

            if (_analogGlitch)
            {
                sequence.Join(DOTween.To(() => _analogGlitch.verticalJump.value, 
                        x => _analogGlitch.verticalJump.value = x, analogIntensity, defaultDuration)
                    .SetEase(Ease.InOutCubic));
            }

            return sequence;
        }

        /// <summary>
        /// 디지털과 아날로그 글리치를 동시에 종료합니다.
        /// </summary>
        public Tween PlayEndMixedGlitch()
        {
            Sequence sequence = DOTween.Sequence();

            // 두 효과 모두 0으로 초기화
            if (_digitalGlitch)
            {
                sequence.Join(DOTween.To(() => _digitalGlitch.intensity.value, 
                        x => _digitalGlitch.intensity.value = x, 0f, defaultDuration)
                    .SetEase(Ease.InOutCubic));
            }

            if (_analogGlitch)
            {
                sequence.Join(DOTween.To(() => _analogGlitch.verticalJump.value, 
                        x => _analogGlitch.verticalJump.value = x, 0f, defaultDuration)
                    .SetEase(Ease.InOutCubic));
            }

            return sequence;
        }
        
        /// <summary>
        /// Volume의 Weight를 조절하는 Tween을 반환합니다.
        /// 외부 시퀀스에서 Join으로 연결하기 위해 Tween 타입을 반환하도록 변경했습니다.
        /// </summary>
        public Tween FadePastVolumeWeight(float targetWeight, float duration = -1f)
        {
            if (!pastVolume) return DOTween.Sequence();

            float animDuration = duration > 0 ? duration : defaultDuration;

            // 컴포넌트를 끄지 않고, 오직 weight 값만 조절합니다.
            return DOTween.To(() => pastVolume.weight, x => pastVolume.weight = x, targetWeight, animDuration)
                .SetEase(Ease.InOutCubic);
        }
        
        public Tween FadePresentVolumeWeight(float targetWeight, float duration = -1f)
        {
            if (!presentVolume) return DOTween.Sequence();

            float animDuration = duration > 0 ? duration : defaultDuration;

            // 컴포넌트를 끄지 않고, 오직 weight 값만 조절합니다.
            return DOTween.To(() => presentVolume.weight, x => presentVolume.weight = x, targetWeight, animDuration)
                .SetEase(Ease.InOutCubic);
        }

        // -----------------------------------------------------------------------
        // [해결책] 글리치 종료와 볼륨 해제를 동시에 실행하는 편의 메서드
        // -----------------------------------------------------------------------
        
        /// <summary>
        /// 글리치 효과를 끄면서 동시에 Volume도 서서히 해제합니다.
        /// </summary>
        public Tween StopGlitchAndReleaseVolume(float glitchDuration = 0.8f)
        {
            Sequence sequence = DOTween.Sequence();

            if (_digitalGlitch != null)
            {
                sequence.Join(DOTween.To(() => _digitalGlitch.intensity.value, 
                    x => _digitalGlitch.intensity.value = x, 0f, glitchDuration)
                    .SetEase(Ease.InOutCubic));
            }

            if (_analogGlitch != null)
            {
                sequence.Join(DOTween.To(() => _analogGlitch.verticalJump.value, 
                    x => _analogGlitch.verticalJump.value = x, 0f, glitchDuration)
                    .SetEase(Ease.InOutCubic));
            }

            if (presentVolume != null)
            {
                sequence.Join(DOTween.To(() => presentVolume.weight, 
                    x => presentVolume.weight = x, 0f, glitchDuration)
                    .SetEase(Ease.InOutCubic));
            }

            return sequence;
        }
    }
}
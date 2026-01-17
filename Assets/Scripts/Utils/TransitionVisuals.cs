using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using URPGlitch;

namespace Utils
{
    public class TransitionVisuals : MonoBehaviour
    {
        [SerializeField] private Volume volume;
        
        [SerializeField] private float duration = 0.8f;
        
        private DigitalGlitchVolume _digitalGlitch;
        private AnalogGlitchVolume _analogGlitch;

        private void Awake()
        {
            if (!volume.profile.TryGet(out _digitalGlitch))
            {
                Debug.LogWarning("프로파일에서 DigitalGlitchVolume을 찾을 수 없습니다. Add Override를 확인하세요.");
            }

            if (!volume.profile.TryGet(out _analogGlitch))
            {
                Debug.LogWarning("프로파일에서 AnalogGlitchVolume을 찾을 수 없습니다. Add Override를 확인하세요.");
            }
            
            if (_digitalGlitch is not null)
            {
                _digitalGlitch.intensity.value = 0f; 
            }
        }

        public Tween PlayStartAnimation()
        {
            Sequence sequence = DOTween.Sequence();
            
            // 둘 중에 하나, 또는 둘다 사용
            
            if (volume is not null)
                sequence.Join(DOTween.To(() => _digitalGlitch.intensity.value, x => _digitalGlitch.intensity.value = x, 1f, duration)).SetEase(Ease.InOutCubic);
            if (volume is not null)
                sequence.Join(DOTween.To(() => _analogGlitch.verticalJump.value, x => _analogGlitch.verticalJump.value = x, 1f, duration)).SetEase(Ease.InOutCubic);
            
            
            // if (_fadeCanvasGroup != null)
            //     sequence.Join(_fadeCanvasGroup.DOFade(1f, _duration));
        
            return sequence;
        }
        
        public Tween PlayEndAnimation()
        {
            Sequence sequence = DOTween.Sequence();
            // if (_fadeCanvasGroup != null)
            //     sequence.Join(_fadeCanvasGroup.DOFade(0f, _duration));
            
            // 둘 중에 하나, 또는 둘다 사용
            
            if (volume is not null)
                sequence.Join(DOTween.To(() => _digitalGlitch.intensity.value, x => _digitalGlitch.intensity.value = x, 0f, duration)).SetEase(Ease.InOutCubic);
            
            if (volume is not null)
                sequence.Join(DOTween.To(() => _analogGlitch.verticalJump.value, x => _analogGlitch.verticalJump.value = x, 0f, duration)).SetEase(Ease.InOutCubic);
        
            return sequence;
        }
        
        public void SetVolumeActive(bool isActive)
        {
            if (volume is not null)
            {
                volume.enabled = isActive;
            }
        }
    }
}

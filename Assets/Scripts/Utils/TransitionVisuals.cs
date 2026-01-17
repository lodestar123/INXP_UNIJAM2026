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

        public Tween PlayStartDigitalGlitchAnimation(float value)
        {
            Sequence sequence = DOTween.Sequence();
            
            if (volume is not null)
                sequence.Join(DOTween.To(() => _digitalGlitch.intensity.value, x => _digitalGlitch.intensity.value = x, value, duration)).SetEase(Ease.InOutCubic);
            
            return sequence;
        }

        public Tween PlayStartAnalogGlitchAnimation(float value)
        {
            Sequence sequence = DOTween.Sequence();
            
            if (volume is not null)
                sequence.Join(DOTween.To(() => _analogGlitch.verticalJump.value, x => _analogGlitch.verticalJump.value = x, value, duration)).SetEase(Ease.InOutCubic);
        
            return sequence;
        }

        public Tween PlayEndDigitalGlitchAnimation(float value)
        {
            Sequence sequence = DOTween.Sequence();

            if (volume is not null)
                sequence.Join(DOTween.To(() => _digitalGlitch.intensity.value, x => _digitalGlitch.intensity.value = x, value, duration)).SetEase(Ease.InOutCubic);
            
            return sequence;
        }
        
        public Tween PlayEndAnalogGlitchAnimation(float value)
        {
            Sequence sequence = DOTween.Sequence();
            
            if (volume is not null)
                sequence.Join(DOTween.To(() => _analogGlitch.verticalJump.value, x => _analogGlitch.verticalJump.value = x, value, duration)).SetEase(Ease.InOutCubic);
        
            return sequence;
        }
        
        public void SetVolumeActive(bool isActive)
        {
            Sequence sequence = DOTween.Sequence();

            if (volume is null) return;
            
            if (isActive)
            {
                sequence.Join(DOTween.To(() => volume.weight, x => volume.weight = x, 1f, 1f)).SetEase(Ease.InOutCubic);
                //     .AppendInterval(0.7f)
                //     .AppendCallback(() => volume.enabled = true);
                // if (!volume.enabled) volume.enabled = true;
            }
            else
            {
                sequence.Join(DOTween.To(() => volume.weight, x => volume.weight = x, 0f, 1.3f)).SetEase(Ease.InOutCubic);
                //     .AppendInterval(0.7f)
                //     .AppendCallback(() => volume.enabled = false);
                // if (volume.enabled) volume.enabled = false;
            }
        }
    }
}

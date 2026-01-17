using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
public class SettingManager : MonoBehaviour
{
    // 오디오 믹서
    public AudioMixer audioMixer;

    // 슬라이더
    public Slider BgmSlider;
    public Slider SfxSlider;

    private const float MinLinear = 0.001f; // 로그 계산용 최소값(= -60dB 정도)
    private const float MinDb = -80f; // 사실상 무음
    private const float MaxDb = 0f; // 최대

    private void OnEnable() // 재활성화 시 초기화
    {
        LoadAndApply(); // 저장값 로드해서 슬라이더/믹서 동기화
    }

    public void SetBgmVolme()
    {
        float linear = Mathf.Clamp(BgmSlider.value, 0f, 1f); // 0~1 고정
        SetMixerFromLinear("BGM", linear); // 믹서 적용
        GameManager.Instance.GameData.backGroundMusicVolume = linear;

    }

    public void SetSFXVolme()
    {
        float linear = Mathf.Clamp(SfxSlider.value, 0f, 1f); // 0~1 고정
        SetMixerFromLinear("SFX", linear); // 믹서 적용
        GameManager.Instance.GameData.effectSoundVolume = linear;
    }

    private void LoadAndApply() // 저장값 로드 + 동기화
    {
        float bgmLinear = GameManager.Instance.GameData.backGroundMusicVolume;
        float sfxLinear = GameManager.Instance.GameData.effectSoundVolume;

        BgmSlider.SetValueWithoutNotify(bgmLinear);
        SfxSlider.SetValueWithoutNotify(sfxLinear);

        SetMixerFromLinear("BGM", bgmLinear);
        SetMixerFromLinear("SFX", sfxLinear);
    }

    private void SetMixerFromLinear(string exposedName, float linear)
    {
        float safeLinear = Mathf.Max(MinLinear, linear);
        float db = Mathf.Log10(safeLinear) * 20f;
        db = Mathf.Clamp(db, MinDb, MaxDb);
        audioMixer.SetFloat(exposedName, db); // 적용
    }

}

using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Sound Players")]
    public AudioSource bgmPlayer;                       //배경음 재생하기 위한 오디오소스
    public AudioSource[] sfxPlayers = new AudioSource[10];//효과음을 재생하기 위한 오디오소스 (동시재생을 위해 배열로 구현)
    public GameObject soundmanager;

    [Header("Audio Clips")]
    public AudioClip[] bgmSound;                //재생할 BGM 저장소
    public AudioClip[] sfxSounds;               //재생할 SFX 저장소

    private void Awake()
    {
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i] = soundmanager.AddComponent<AudioSource>();
        }
    }
    private void Start()
    {
        Init();
        PlayBGM(bgmSound[0]); //타이틀 BGM 재생
    }

    //BGM 바꾸고싶을 때만 매개변수 전달하기. 그냥 재생은 매개변수 없어도 됨
    public void PlayBGM(AudioClip bgmclip = null)
    {
        if (bgmclip != null)
        {
            bgmPlayer.clip = bgmclip;
        }

        bgmPlayer.Play();
    }
    public void StopBGM()
    {
        bgmPlayer.Stop();
    }

    public void PauseBGM()
    {
        bgmPlayer.Pause();
    }

    public void UnPauseBGM()
    {
        bgmPlayer.UnPause();
    }

    public void PlaySFX(AudioClip sfxSound)
    {
        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            if (sfxPlayers[index].isPlaying)
                continue;

            sfxPlayers[index].clip = sfxSound;
            sfxPlayers[index].Play();
            return;
        }
    }
    public void PlaySFX(int sfxSound)
    {
        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            if (sfxPlayers[index].isPlaying)
                continue;

            sfxPlayers[index].clip = sfxSounds[sfxSound];
            sfxPlayers[index].Play();
            return;
        }
    }


    public void Init() //오디오 소스 설정 초기화
    {
        bgmPlayer.playOnAwake = true;
        bgmPlayer.loop = true;
        bgmPlayer.clip = bgmSound[0]; //기본 타이틀 BGM 설정

        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i].playOnAwake = false;
            sfxPlayers[i].loop = false;
        }

        ApplyVolume();
    }
    public void ApplyVolume()
    {
        print(gameObject.GetComponent<GameData>().backGroundMusicVolume);
        bgmPlayer.volume = gameObject.GetComponent<GameData>().backGroundMusicVolume;
        for (int i = 0; i < sfxPlayers.Length; i++)
        {
            sfxPlayers[i].volume = gameObject.GetComponent<GameData>().effectSoundVolume;
        }
    }
}

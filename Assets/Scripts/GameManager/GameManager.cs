using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }


    [SerializeField] private GameData gamedata = new GameData();    // GameData (세이브 필요한 데이터)
    public GameData GameData => gamedata;

    public SoundManager soundManager { get; private set; }

    void Awake()
    {
        // 싱글톤
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // 컴포넌트 초기화
            soundManager = GetComponent<SoundManager>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
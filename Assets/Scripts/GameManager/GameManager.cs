using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

    public static GameManager Instance { get; private set; }

    // GameData 노출
    [SerializeField] private GameData data = new GameData();
    public GameData Data => data;

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
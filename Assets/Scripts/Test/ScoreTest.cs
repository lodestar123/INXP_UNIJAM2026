using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 테스트용: 인게임에서 점수를 조작합니다.
/// GameSceneManager가 있는 씬의 활성 오브젝트에 붙이고,
/// 플레이 중 Inspector에서 값을 바꾼 뒤 F5(가산) / F6(고정) 키를 누르세요.
/// </summary>
public class ScoreTest : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Debug - 인게임 점수 조작 (플레이 중 Inspector에서 값 변경 후 키 입력)")]
    [SerializeField] private int debugAddScore = 1000;   // F5: 이만큼 가산
    [SerializeField] private int debugSetScore = 3000;   // F6: 이 값으로 고정

    void Update()
    {
        if (GameSceneManager.Instance == null) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            GameSceneManager.Instance.AddScore(debugAddScore, true);
            Debug.Log($"[Debug] 점수 +{debugAddScore} → 현재 {GameSceneManager.Instance.CurrentScore}");
        }
        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
            GameSceneManager.Instance.SetScore(debugSetScore);
            Debug.Log($"[Debug] 점수 설정 → {GameSceneManager.Instance.CurrentScore}");
        }
    }
#endif
}

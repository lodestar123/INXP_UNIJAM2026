using UnityEngine;
using System.Collections.Generic;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance { get; private set; }

    private List<int> collectedItems = new List<int>(); // 수집한 아이템 id를 차례로 저장

    public IReadOnlyList<int> CollectedItems => collectedItems;
    public int CurrentScore { get; private set; } // 현재 점수
    public float CurrentTime { get; private set; } // 남은 시간 카운트

    private bool isGameOver = false;

    [SerializeField] private float gameTimeLimit = 60f; // 게임 제한 시간

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentTime = gameTimeLimit;
    }
    void Update()
    {
        if (isGameOver) return;

        CurrentTime -= Time.deltaTime;

        if (CurrentTime <= 0)
        {
            CurrentTime = 0;
            OnGameOver(); // 게임오버
        }

    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void AddItem(int itemId) // 아이템 id 매개변수로 받아 리스트에 추가
    {
        if (isGameOver) return;
        collectedItems.Add(itemId);
    }
    public void ClearItem() // 아이템 전체 삭제
    {
        if (isGameOver) return;
        collectedItems.Clear();
    }

    public void AddScore(int score) // 점수 추가
    {
        if (isGameOver) return;
        CurrentScore += score;
    }

    void OnGameOver()
    {
        if (isGameOver) return;

        isGameOver = true; // 게임 오버

        // 최종 점수 비교 전달
        if (GameManager.Instance != null && GameManager.Instance.GameData != null)
        {
            if (CurrentScore > GameManager.Instance.GameData.highScore)
            {
                GameManager.Instance.GameData.highScore = CurrentScore;
            }
        }
    }
}


using System;
using System.Collections.Generic;
using FlappyBird.Configs;
using FlappyBird.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

namespace FlappyBird.Game
{
    // 플래피 버드 게임의 전체 상태와 흐름을 관리하는 싱글톤 클래스입니다.
    public class FlappyBirdGameManager : MonoBehaviour
    {
        public static FlappyBirdGameManager Instance { get; private set; }
        private enum GameState
        {
            Ready,    // 게임 시작 대기
            Playing,  // 게임 진행 중
            GameOver  // 플레이어 사망
        }

        [Header("설정 및 참조")]
        [SerializeField] private FlappyBirdConfig flappyBirdConfig;
        [SerializeField] private FlappyBirdPlayer player;
        [SerializeField] private PipeSpawner pipeSpawner;

        private GameState CurrentState { get; set; }
        private int Score { get; set; }

        private readonly List<Item> _collectedItems = new List<Item>();

        private void OnEnable()
        {
            if (this == null || gameObject == null) return;
            
            ResetGameState();
            
            if (player is not null)
            {
                SetState(GameState.Ready);
                player.ResetPlayer();
            }

            if (pipeSpawner != null)
            {
                pipeSpawner.ClearPipes();

                bool preserveSpeed = true;
                if (GameSceneManager.Instance != null && GameSceneManager.Instance.IsResetting)
                {
                    preserveSpeed = false;
                }
                
                pipeSpawner.PreparePipes(preserveSpeed);
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            
            Instance = this;
        }

        private void Start()
        {
            if (this == null || gameObject == null) return;
            
            if (player == null || pipeSpawner == null)
            {
                if (GameSceneManager.Instance != null && !GameSceneManager.Instance.IsGameOver)
                {
                    Debug.LogWarning("[FlappyBirdGameManager] 필수 오브젝트 참조가 누락되었습니다. (씬 전환 중일 수 있음)");
                }
                return;
            }
            
            if (CurrentState != GameState.Ready)
            {
                SetState(GameState.Ready);
                player.ResetPlayer();
            }
        }

        private void Update()
        {
            if (player is null || pipeSpawner is null) return;
            
            if (Pointer.current == null) return;

            // 플레이어가 등장 애니메이션 중이면 입력 무시
            if (player is not null && player.IsAnimating) return;
            
            bool isPressedThisFrame = Pointer.current.press.wasPressedThisFrame;

            if (CurrentState == GameState.Ready && isPressedThisFrame)
            {
                StartGame();
            }
        }

        public void StartGame()
        {
            if (CurrentState != GameState.Ready) return;

            SetState(GameState.Playing);
            player.ActivatePlayer();

            pipeSpawner.StartSpawning();

            Score = 0;
            _collectedItems.Clear();
#if UNITY_EDITOR
            Debug.Log("게임 시작!");
#endif
        }

        public void EndGame()
        {
            if (CurrentState != GameState.Playing) return;

            SetState(GameState.GameOver);
            pipeSpawner.StopSpawning();
            pipeSpawner.StopPipeMovement();
#if UNITY_EDITOR
            Debug.Log($"게임 종료! 점수: {Score}, 아이템: {_collectedItems.Count}");
#endif
        }

        public void TransitionToNextGame()
        {
            if (CurrentState != GameState.GameOver) return;
            
            // 애니메이션 종료 후 호출됨
            if (GameSceneManager.Instance == null) return;

            pipeSpawner.ClearPipes();
            GameSceneManager.Instance.OnChangeGame();
        }

        /// <summary>
        /// 게임 재시작 시 상태를 초기화하는 메서드
        /// </summary>
        public void ResetGameState()
        {
            SetState(GameState.Ready);
            
            if (player != null)
            {
                try
                {
                    player.CancelDeathAnimation();
                }
                catch (System.Exception)
                {
                    // ignored
                }
            }
            
            Score = 0;
            _collectedItems.Clear();
            
            pipeSpawner?.StopSpawning();
            pipeSpawner?.StopPipeMovement();
            pipeSpawner?.ClearPipes();
        }

        public void OnItemCollected(Item item)
        {
            if (CurrentState != GameState.Playing || item == null) return;

            _collectedItems.Add(item);
            
            FlappyItemCollector.CollectItem(item);
#if UNITY_EDITOR
            Debug.Log($"아이템 획득: {item.name}");
#endif
        }

        public List<Item> GetCollectedItems()
        {
            List<Item> items = new List<Item>(_collectedItems);
            _collectedItems.Clear();
            return items;
        }

        public void IncrementScore()
        {
            if (CurrentState != GameState.Playing) return;

            Score++;
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            { 
                Instance = null;
            }
            
            player = null;
            pipeSpawner = null;
        }
    }
}
using FlappyBird.Configs;
using FlappyBird.Player;
using UnityEngine;
using UnityEngine.InputSystem; 
using Utils;

namespace FlappyBird.Game
{
    // 플래피 버드 게임의 전체 상태와 흐름을 관리하는 싱글톤 클래스입니다.
    public class FlappyBirdGameManager : Singleton<FlappyBirdGameManager>
    {
        public enum GameState
        {
            Ready,    // 게임 시작 대기
            Playing,  // 게임 진행 중
            GameOver  // 플레이어 사망
        }

        [Header("설정 및 참조")]
        [SerializeField] private FlappyBirdConfig flappyBirdConfig;
        [SerializeField] private FlappyBirdPlayer player;
        [SerializeField] private PipeSpawner pipeSpawner; 

        public GameState CurrentState { get; private set; }
        public int Score { get; private set; }

        // 수집된 아이템 리스트
        private System.Collections.Generic.List<Item> _collectedItems = new System.Collections.Generic.List<Item>();

        private void Start()
        {
            if (player == null || pipeSpawner == null)
            {
                Debug.LogError("GameManager: 필수 오브젝트 참조가 누락되었습니다.");
                return;
            }
            
            SetState(GameState.Ready);
            player.ResetPlayer();
        }

        private void Update()
        {
            if (Pointer.current == null) return;
            
            bool isPressedThisFrame = Pointer.current.press.wasPressedThisFrame;

            // 대기 중 터치 시 게임 시작
            if (CurrentState == GameState.Ready && isPressedThisFrame)
            {
                StartGame();
            }
        }

        // 게임을 시작합니다.
        public void StartGame()
        {
            if (CurrentState != GameState.Ready) return;
            
            SetState(GameState.Playing);
            player.ActivatePlayer();
            pipeSpawner.StartSpawning();
            
            Score = 0;
            _collectedItems.Clear();
            Debug.Log("게임 시작!");
        }

        // 게임을 종료합니다.
        public void EndGame()
        {
            if (CurrentState != GameState.Playing) return;

            SetState(GameState.GameOver);
            pipeSpawner.StopSpawning();
            
            Debug.Log($"게임 종료! 점수: {Score}, 아이템: {_collectedItems.Count}");
        }

        // 아이템 획득 시 호출됩니다.
        public void OnItemCollected(Item item)
        {
            if (CurrentState != GameState.Playing || item == null) return;

            _collectedItems.Add(item);
            Debug.Log($"아이템 획득: {item.name}");
        }

        // 모은 아이템 리스트를 반환합니다.
        public System.Collections.Generic.List<Item> GetCollectedItems()
        {
            System.Collections.Generic.List<Item> items = new System.Collections.Generic.List<Item>(_collectedItems);
            _collectedItems.Clear();
            return items;
        }

        // 점수를 증가시킵니다.
        public void IncrementScore()
        {
            if (CurrentState != GameState.Playing) return;
            
            Score++;
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
        }
    }
}

using FlappyBird.Configs;
using FlappyBird.Player;
using UnityEngine;
using UnityEngine.InputSystem; // Import the new Input System namespace
using Utils;

namespace FlappyBird.Game
{
    // Required references for the GameManager to function
    // [RequireComponent(typeof(PipeSpawner))] // Will add later

    public class FlappyBirdGameManager : Singleton<FlappyBirdGameManager>
    {
        public enum GameState
        {
            Ready,
            Playing,
            GameOver
        }

        [Header("Game Configuration")]
        [Tooltip("Assign the main configuration file for the game.")]
        [SerializeField] private FlappyBirdConfig flappyBirdConfig;
        
        [Header("Object References")]
        [Tooltip("Assign the Player object from the scene.")]
        [SerializeField] private FlappyBirdPlayer player;
        [Tooltip("Assign the Pipe Spawner object from the scene.")]
        [SerializeField] private PipeSpawner pipeSpawner; 

        public GameState CurrentState { get; private set; }
        public int Score { get; private set; }

        private void Start()
        {
            // Ensure essential references are set
            if (player == null || pipeSpawner == null)
            {
                return;
            }
            
            // Set initial game state
            SetState(GameState.Ready);
            player.ResetPlayer();
        }

        private void Update()
        {
            // Ensure a pointer device is active before checking for input.
            if (Pointer.current == null) return;
            
            bool isPressedThisFrame = Pointer.current.press.wasPressedThisFrame;

            // In the Ready state, wait for the first input to start the game.
            if (CurrentState == GameState.Ready && isPressedThisFrame)
            {
                StartGame();
            }
            // In the GameOver state, wait for input to restart the game.
            else if (CurrentState == GameState.GameOver && isPressedThisFrame)
            {
                RestartGame();
            }
        }

        public void StartGame()
        {
            if (CurrentState != GameState.Ready) return;
            
            SetState(GameState.Playing);
            player.ActivatePlayer();
            pipeSpawner.StartSpawning();
            
            Score = 0;
            Debug.Log("Score: 0");
            // TODO: Call UI Manager to update score display
        }

        public void EndGame()
        {
            if (CurrentState != GameState.Playing) return;

            SetState(GameState.GameOver);
            pipeSpawner.StopSpawning();
            
            Debug.Log($"Game Over! Final Score: {Score}");
            // TODO: Call UI Manager to show the game over screen
        }

        public void IncrementScore()
        {
            if (CurrentState != GameState.Playing) return;
            
            Score++;
            Debug.Log($"Score: {Score}");
            // TODO: Call UI Manager to update score display
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
            Debug.Log($"[GameManager] State changed to: {newState}");
        }

        private void RestartGame()
        {
            Debug.Log("Restarting Game...");
            
            // Reset player and pipes
            player.ResetPlayer();
            pipeSpawner.ClearPipes();

            // Set state to ready for the next round
            SetState(GameState.Ready);

            // TODO: Reset UI elements
        }

        private void ReloadScene()
        {
            // This is a harder reset, reloads the entire scene.
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }
    }
}

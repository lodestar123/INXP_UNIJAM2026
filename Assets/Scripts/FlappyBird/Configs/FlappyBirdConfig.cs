using UnityEngine;

namespace FlappyBird.Configs
{
    [CreateAssetMenu(fileName = "FlappyBirdConfig", menuName = "Scriptable Objects/FlappyBirdConfig")]
    public class FlappyBirdConfig : ScriptableObject
    {
        [Header("Player")]
        [Min(0.0f)] public float HoldForce = 25.0f;
        [Min(0.0f)] public float MaxUpVelocity = 6.0f;
        [Min(0.0f)] public float MaxDownVelocity = 10.0f;

        [Header("Pipes")]
        [Min(0.1f)] public float PipeSpawnInterval = 1.2f;
        [Min(0.0f)] public float PipeMoveSpeed = 3.5f;
        public float PipeSpawnX = 10.0f;
        public float PipeMinY = -2.0f;
        public float PipeMaxY = 2.0f;
    }
}

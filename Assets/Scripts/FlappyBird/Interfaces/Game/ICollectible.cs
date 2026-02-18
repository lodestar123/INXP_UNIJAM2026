using UnityEngine;

namespace FlappyBird.Interfaces.Game
{
    public interface ICollectible
    {
        bool TryCollect(GameObject collector);
    }
}

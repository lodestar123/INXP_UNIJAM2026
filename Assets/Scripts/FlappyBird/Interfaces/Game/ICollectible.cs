using UnityEngine;

namespace FlappyBird.Interfaces.Game
{
    /// <summary>
    /// 수집 가능한 오브젝트의 획득 동작을 정의합니다.
    /// </summary>
    public interface ICollectible
    {
        bool TryCollect(GameObject collector);
    }
}

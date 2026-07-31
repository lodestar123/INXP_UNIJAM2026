using FlappyBird.Interfaces.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace FlappyBird.Game
{
    /// <summary>
    /// 포인터(터치/마우스) 입력으로 게임 시작 신호를 제공합니다.
    /// </summary>
    public class PointerGameStartInput : MonoBehaviour, IGameStartInput
    {
        public bool IsStartPressedThisFrame
        {
            get
            {
                if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
                    return false;

                // StartAalarm 같은 UI가 화면을 덮고 있는 동안의 탭은 게임 시작 신호로 보지 않는다
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                    return false;

                return true;
            }
        }
    }
}

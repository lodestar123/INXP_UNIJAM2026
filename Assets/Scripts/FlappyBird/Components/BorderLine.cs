using System;
using FlappyBird.Game;
using UnityEngine;

namespace FlappyBird.Components
{
    public class BorderLine : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                FlappyBirdGameManager.Instance.EndGame();
            }
        }
    }
}
using System.Threading;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UICanvasManager : MonoBehaviour
{
    public Image myGameTimer;
    public TextMeshProUGUI myGameScore;


    void OnEnable()
    {
        if (GameSceneManager.Instance != null && !GameSceneManager.Instance.gameTimers.Contains(myGameTimer))
        {
            GameSceneManager.Instance.gameTimers.Add(myGameTimer);
        }
        
        GameSceneManager.Instance.gameScore = myGameScore;
        myGameScore.text = GameSceneManager.Instance.CurrentScore.ToString();

    }
}

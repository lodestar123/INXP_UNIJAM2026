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
        GameSceneManager.Instance.gameTimer = myGameTimer;
        GameSceneManager.Instance.gameScore = myGameScore;
        myGameScore.text = GameSceneManager.Instance.CurrentScore.ToString();

    }
}

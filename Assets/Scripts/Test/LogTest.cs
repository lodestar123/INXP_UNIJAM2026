using UnityEngine;
using Utils;

public class LogTest : MonoBehaviour
{
    private void Start()
    {
        CustomLog.Info("일반 로그");
        CustomLog.Warn("경고 로그");
        CustomLog.Error("에러 로그");
    }
}
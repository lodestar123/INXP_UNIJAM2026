using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DashboardManager : MonoBehaviour
{
    [Header("스테이지 랭킹 버튼")]
    [SerializeField] private Button[] stageRankButtons = new Button[4];

    [Header("랭킹 목록 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI rankContentText;

    const string LoadingMessage = "불러오는 중...";
    const string ErrorMessage = "랭킹을 불러올 수 없습니다.";
    const string EmptyMessage = "아직 기록이 없습니다.";

    void Awake()
    {
        if (stageRankButtons == null || stageRankButtons.Length == 0)
            return;
        

        for (int i = 0; i < stageRankButtons.Length; i++)
        {
            if (stageRankButtons[i] == null) continue;

            int stageIndex = i;
            stageRankButtons[i].onClick.AddListener(() =>
            {
                if (GameManager.Instance?.soundManager != null)
                    GameManager.Instance.soundManager.PlaySFX(SoundManager.SFX.ButtonClick);
                RefreshStageRank(stageIndex);
            });
        }
    }

    void OnEnable()
    {
        RefreshStageRank(0);
    }

    /// <summary>
    /// 지정한 스테이지 인덱스(0~3)의 랭킹을 백엔드에서 가져와 rankContentText에 표시합니다.
    /// </summary>
    public void RefreshStageRank(int stageIndex)
    {
        if (rankContentText == null) return;

        rankContentText.text = LoadingMessage;

        BackendRank.Instance.GetRankListForUI(
            stageIndex,
            onSuccess: list =>
            {
                if (rankContentText == null) return;
                if (list == null || list.Count == 0)
                {
                    rankContentText.text = EmptyMessage;
                    return;
                }

                string content = "";
                foreach (var item in list)
                {
                    string nickname = item.nickname ?? "";
                    string scoreStr = item.score.ToString();
                    int dashLength = 20 - nickname.Length - scoreStr.Length;
                    if (dashLength < 0) dashLength = 0;
                    content += $"{item.rank}. [ {nickname} ] {new string('-', dashLength)} {item.score} 점\n";
                }
                if (rankContentText != null)
                    rankContentText.text = content.TrimEnd();
            },
            onFailure: () =>
            {
                if (rankContentText != null)
                    rankContentText.text = ErrorMessage;
            }
        );
    }
}

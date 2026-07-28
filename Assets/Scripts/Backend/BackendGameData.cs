using UnityEngine;
using BackEnd;

public class BackendGameData
{
    private static BackendGameData _instance = null;

    public static BackendGameData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BackendGameData();
            }

            return _instance;
        }
    }

    private string gameDataRowInDate = string.Empty;

    const string UserDataTableName = "USER_DATA";

    /// <summary>
    /// 현재 로그인 유저의 USER_DATA 행이 없으면 <see cref="GameDataInsert"/>로 생성합니다.
    /// (구글 등 페더레이션 로그인 직후 랭킹/게임정보가 참조할 행을 미리 만듭니다.)
    /// </summary>
    public void EnsureUserDataForCurrentUser()
    {
        Debug.Log("[BackendGameData] USER_DATA 존재 여부 확인");

        var bro = Backend.GameData.GetMyData(UserDataTableName, new Where());
        if (!bro.IsSuccess())
        {
            Debug.LogError("[BackendGameData] USER_DATA 조회 실패: " + bro);
            return;
        }

        var rows = bro.FlattenRows();
        if (rows != null && rows.Count > 0)
        {
            gameDataRowInDate = rows[0]["inDate"].ToString();
            Debug.Log("[BackendGameData] 기존 USER_DATA 사용. inDate=" + gameDataRowInDate);
            return;
        }

        Debug.Log("[BackendGameData] USER_DATA 없음 — GameDataInsert()로 초기 행 생성");
        GameDataInsert();
    }

    // 게임 정보 삽입
    public void GameDataInsert()
    {
        Debug.Log("데이터를 초기화합니다.");

        Debug.Log("뒤끝 업데이트 목록에 해당 데이터들을 추가합니다.");
        Param param = new Param();

        int stageCount = GameData.StageCount;
        for (int i = 0; i < stageCount; i++)
        {
            string stageScoreKey = $"score_{i}";
            param.Add(stageScoreKey, -1);
        }
        param.Add("score_rank", -1); // 랭크모드 점수 컬럼 초기화

        Debug.Log("게임 정보 데이터 삽입을 요청합니다.");
        var bro = Backend.GameData.Insert(UserDataTableName, param);

        if (bro.IsSuccess())
        {
            Debug.Log("게임 정보 데이터 삽입에 성공했습니다. : " + bro);
            gameDataRowInDate = bro.GetInDate();
        }
        else
        {
            Debug.LogError("게임 정보 데이터 삽입에 실패했습니다. : " + bro);
        }
    }
}

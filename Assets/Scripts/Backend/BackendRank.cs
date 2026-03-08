using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BackEnd;

/// <summary>
/// 랭킹 관리 클래스
/// </summary>
public class BackendRank
{
    // 스테이지별 랭킹 UUID
    // 인덱스: GameManager.currentStageNum 기준
    static readonly string[] RankUUIDs = new string[] // 뒤끝 콘솔에서 확인
    {
        "019c9e1e-5b11-73fe-b84c-4fae6d9d9470",
        "019c9e1e-8868-759d-a02f-6d05c55eecf1",
        "019c9e1e-bec6-7b12-b52f-3468d8a54ae7",
        "019c9e1e-fa59-791b-9d69-4132b2ec7c89",
    };

    const string GameDataTableName = "USER_DATA"; // GameData 테이블

    static string GetScoreColumnName(int stageIndex)
    {
        if (stageIndex < 0) stageIndex = 0;
        return $"score_{stageIndex}";
    }

    private static BackendRank _instance = null;

    public static BackendRank Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BackendRank();
            }

            return _instance;
        }
    }

    /// <summary>
    /// 현재 스테이지의 하이스코어를 가져옵니다.
    /// </summary>
    public int GetCurrentStageHighScore()
    {
        if (GameManager.Instance == null || GameManager.Instance.GameData == null)
            return 0;

        var stageScores = GameManager.Instance.GameData.stageHighScore;
        if (stageScores == null || stageScores.Count == 0)
            return 0;

        int stageIndex = GameManager.Instance.currentStageNum;

        // 인덱스가 범위를 벗어나면 0번 스테이지로 강제 매핑
        if (stageIndex < 0 || stageIndex >= stageScores.Count)
            stageIndex = 0;

        return stageScores[stageIndex];
    }

    /// <summary>
    /// 현재 스테이지의 하이스코어를 서버 랭킹에 등록합니다.
    /// </summary>
    public void RankInsertCurrentStageHighScore()
    {
        int stageHighScore = GetCurrentStageHighScore();
        int stageIndex = (GameManager.Instance != null) ? GameManager.Instance.currentStageNum : 0;
        RankInsertForStage(stageIndex, stageHighScore);
    }

    /// <summary>
    /// 지정한 스테이지 인덱스용 랭킹에 점수를 등록합니다.
    /// </summary>
    public void RankInsertForStage(int stageIndex, int score)
    {
        if (!TryGetRankConfig(stageIndex, out string rankUUID, out string tableName))
            return;

        string rowInDate = string.Empty;

        // 랭킹을 삽입하기 위해 게임 데이터에서 사용하는 데이터의 inDate값이 필요하므로 해당 데이터의 inDate값을 추출합니다.
        Debug.Log($"[{stageIndex}]번 스테이지용 데이터 조회를 시도합니다. (tableName={tableName})");
        var bro = Backend.GameData.GetMyData(tableName, new Where());

        if (bro.IsSuccess() == false)
        {
            Debug.LogError("데이터 조회 중 문제가 발생했습니다 : " + bro);
            return;
        }

        Debug.Log("데이터 조회에 성공했습니다 : " + bro);

        if (bro.FlattenRows().Count > 0)
        {
            rowInDate = bro.FlattenRows()[0]["inDate"].ToString();
        }
        else
        {
            Debug.Log("데이터가 존재하지 않습니다. 데이터 삽입을 시도합니다.");

            // USER_DATA 초기 삽입 시 스테이지별 점수 컬럼을 -1로 초기화
            Param initParam = new Param();
            int stageCount = GameData.StageCount;
            for (int i = 0; i < stageCount; i++)
            {
                initParam.Add(GetScoreColumnName(i), -1);
            }

            var bro2 = Backend.GameData.Insert(tableName, initParam);

            if (bro2.IsSuccess() == false)
            {
                Debug.LogError("데이터 삽입 중 문제가 발생했습니다 : " + bro2);
                return;
            }

            Debug.Log("데이터 삽입에 성공했습니다 : " + bro2);

            rowInDate = bro2.GetInDate();
        }

        Debug.Log("내 게임 정보의 rowInDate : " + rowInDate);

        Param param = new Param();
        // 스테이지별 전용 점수 컬럼에 기록
        string scoreColumn = GetScoreColumnName(stageIndex);
        param.Add(scoreColumn, score);

        // 추출된 rowIndate를 가진 데이터에 param값으로 수정을 진행하고 랭킹에 데이터를 업데이트합니다.  
        Debug.Log($"[{stageIndex}]번 스테이지 랭킹 삽입을 시도합니다. (rankUUID={rankUUID}, tableName={tableName})");
        var rankBro = Backend.URank.User.UpdateUserScore(rankUUID, tableName, rowInDate, param);

        if (rankBro.IsSuccess() == false)
        {
            Debug.LogError("랭킹 등록 중 오류가 발생했습니다. : " + rankBro);
            return;
        }

        Debug.Log("랭킹 삽입에 성공했습니다. : " + rankBro);
    }

    /// <summary>
    /// 스테이지 인덱스를 기반으로 랭킹 UUID / 게임 정보 테이블 이름을 가져옵니다.
    /// </summary>
    bool TryGetRankConfig(int stageIndex, out string rankUUID, out string tableName)
    {
        rankUUID = null;
        tableName = null;

        // 인덱스가 범위를 벗어나면 0번 스테이지로 강제 매핑
        int idx = stageIndex;
        if (idx < 0 || idx >= RankUUIDs.Length)
        {
            Debug.LogWarning($"잘못된 스테이지 인덱스입니다. stageIndex={stageIndex}, 0번 스테이지로 대체");
            idx = 0;
        }

        rankUUID = RankUUIDs[idx];
        // 뒤끝 구조상 랭킹은 GameData 테이블을 참조하므로,
        // 여기서는 공통 게임 정보 테이블(USER_DATA)을 사용한다.
        tableName = GameDataTableName;

        if (string.IsNullOrEmpty(rankUUID) || string.IsNullOrEmpty(tableName))
        {
            Debug.LogError($"스테이지 {stageIndex}의 랭킹 설정이 비어 있습니다. RankUUIDs/RankTableNames를 확인하세요.");
            return false;
        }

        return true;
    }

    // 랭킹 조회 (테스트용, 0번 스테이지 기준)
    public void RankGet()
    {
        if (!TryGetRankConfig(0, out string rankUUID, out string tableName))
            return;

        var bro = Backend.URank.User.GetRankList(rankUUID);

        if (bro.IsSuccess() == false)
        {
            Debug.LogError("랭킹 조회중 오류가 발생했습니다. : " + bro);
            return;
        }

        Debug.Log("랭킹 조회에 성공했습니다. : " + bro);

        Debug.Log("총 랭킹 등록 유저 수 : " + bro.GetFlattenJSON()["totalCount"].ToString());

        foreach (LitJson.JsonData jsonData in bro.FlattenRows())
        {
            StringBuilder info = new StringBuilder();

            info.AppendLine("순위 : " + jsonData["rank"].ToString());
            info.AppendLine("닉네임 : " + jsonData["nickname"].ToString());
            info.AppendLine("점수 : " + jsonData["score"].ToString());
            info.AppendLine("gamerInDate : " + jsonData["gamerInDate"].ToString());
            info.AppendLine("정렬번호 : " + jsonData["index"].ToString());
            info.AppendLine();
            Debug.Log(info);
        }
    }

    /// <summary>
    /// 랭킹 목록을 가져와서 UI에서 쓸 수 있도록 콜백으로 전달합니다. (기본: 0번 스테이지)
    /// </summary>
    public void GetRankListForUI(Action<List<(int rank, string nickname, int score)>> onSuccess, Action onFailure = null)
    {
        GetRankListForUI(0, onSuccess, onFailure);
    }

    /// <summary>
    /// 특정 스테이지 인덱스의 랭킹 목록을 UI에서 쓸 수 있도록 콜백으로 전달합니다.
    /// </summary>
    public void GetRankListForUI(int stageIndex, Action<List<(int rank, string nickname, int score)>> onSuccess, Action onFailure = null)
    {
        if (!TryGetRankConfig(stageIndex, out string rankUUID, out string tableName))
        {
            onFailure?.Invoke();
            return;
        }

        var bro = Backend.URank.User.GetRankList(rankUUID);

        if (!bro.IsSuccess())
        {
            Debug.LogError("랭킹 조회 실패: " + bro);
            onFailure?.Invoke();
            return;
        }

        var rows = bro.FlattenRows();
        if (rows == null || rows.Count == 0)
        {
            onSuccess?.Invoke(new List<(int rank, string nickname, int score)>());
            return;
        }

        var list = new List<(int rank, string nickname, int score)>();
        // 이 랭킹이 어떤 스테이지용인지에 따라 점수 컬럼 이름을 결정
        string scoreKey = GetScoreColumnName(stageIndex);
        foreach (LitJson.JsonData row in rows)
        {
            int rank = SafeGetInt(row, "rank", 0);
            string nickname = SafeGetStr(row, "nickname");
            string scoreStr = SafeGetStr(row, scoreKey) ?? SafeGetStr(row, "score");
            int score = int.TryParse(scoreStr, out var s) ? s : 0;
            list.Add((rank, nickname, score));
        }
        onSuccess?.Invoke(list);
    }

    static string SafeGetStr(LitJson.JsonData data, string key)
    {
        if (data == null) return null;
        try
        {
            var v = data[key];
            return v?.ToString();
        }
        catch (KeyNotFoundException) { return null; }
    }

    static int SafeGetInt(LitJson.JsonData data, string key, int fallback = 0)
    {
        var s = SafeGetStr(data, key);
        return int.TryParse(s, out var n) ? n : fallback;
    }
}

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
    string rankUUID = "019c7477-49b9-72ec-8655-f72bf4338ad2";
    string tableName = "USER_DATA";
    const string RankScoreColumn = "score";

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

    // 랭킹 삽입
    public void RankInsert(int score)
    {
        string rowInDate = string.Empty;

        // 랭킹을 삽입하기 위해 게임 데이터에서 사용하는 데이터의 inDate값이 필요하므로 해당 데이터의 inDate값을 추출합니다.
        Debug.Log("데이터 조회를 시도합니다.");
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
            var bro2 = Backend.GameData.Insert(tableName);

            if (bro2.IsSuccess() == false)
            {
                Debug.LogError("데이터 삽입 중 문제가 발생했습니다 : " + bro2);
                return;
            }

            Debug.Log("데이터 삽입에 성공했습니다 : " + bro2);

            rowInDate = bro2.GetInDate();
        }

        Debug.Log("내 게임 정보의 rowInDate : " + rowInDate); // 추출된 rowIndate의 값은 다음과 같습니다.  

        Param param = new Param();
        param.Add(RankScoreColumn, score);

        // 추출된 rowIndate를 가진 데이터에 param값으로 수정을 진행하고 랭킹에 데이터를 업데이트합니다.  
        Debug.Log("랭킹 삽입을 시도합니다.");
        var rankBro = Backend.URank.User.UpdateUserScore(rankUUID, tableName, rowInDate, param);

        if (rankBro.IsSuccess() == false)
        {
            Debug.LogError("랭킹 등록 중 오류가 발생했습니다. : " + rankBro);
            return;
        }

        Debug.Log("랭킹 삽입에 성공했습니다. : " + rankBro);
    }

    // 랭킹 조회
    public void RankGet()
    {
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
    /// 랭킹 목록을 가져와서 UI에서 쓸 수 있도록 콜백으로 전달합니다.
    /// 성공 시 (순위, 닉네임, 점수) 리스트를 onSuccess에 넘기고, 실패 시 onFailure 호출.
    /// </summary>
    public void GetRankListForUI(Action<List<(int rank, string nickname, int score)>> onSuccess, Action onFailure = null)
    {
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
        foreach (LitJson.JsonData row in rows)
        {
            int rank = SafeGetInt(row, "rank", 0);
            string nickname = SafeGetStr(row, "nickname") ?? SafeGetStr(row, "nickName") ?? "";
            string scoreStr = SafeGetStr(row, "score") ?? SafeGetStr(row, "level") ?? "0";
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

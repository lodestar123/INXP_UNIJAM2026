using System.Collections.Generic;
using System.Text;
using UnityEngine;
using BackEnd; 

/// <summary>
/// 유저 데이터 클래스
/// </summary>
public class UserData
{
    public int score = 0;

    public override string ToString()
    { 
        StringBuilder result = new StringBuilder();
        result.AppendLine($"score : {score}");

        return result.ToString();
    }
}

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

    public static UserData userData;

    private string gameDataRowInDate = string.Empty;

    // 게임 정보 삽입
    public void GameDataInsert()
    {
        if (userData == null)
        {
            userData = new UserData();
        }

        Debug.Log("데이터를 초기화합니다.");
        userData.score = 0;

        Debug.Log("뒤끝 업데이트 목록에 해당 데이터들을 추가합니다.");
        Param param = new Param();

        int stageCount = GameData.StageCount;
        for (int i = 0; i < stageCount; i++)
        {
            string stageScoreKey = $"score_{i}";
            param.Add(stageScoreKey, -1);
        }


        Debug.Log("게임 정보 데이터 삽입을 요청합니다.");
        var bro = Backend.GameData.Insert("USER_DATA", param);

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

    public void GameDataGet()
    {
        Debug.Log("게임 정보 조회 함수를 호출합니다.");

        var bro = Backend.GameData.GetMyData("USER_DATA", new Where());

        if (bro.IsSuccess())
        {
            Debug.Log("게임 정보 조회에 성공했습니다. : " + bro);

            LitJson.JsonData gameDataJson = bro.FlattenRows(); // Json으로 리턴된 데이터를 받아옵니다.
            if (gameDataJson == null || gameDataJson.Count <= 0)
            {
                Debug.LogWarning("데이터가 존재하지 않습니다.");
            }
            else
            {
                gameDataRowInDate = gameDataJson[0]["inDate"].ToString(); 

                userData = new UserData();

                userData.score = int.Parse(gameDataJson[0]["score"].ToString());

                Debug.Log(userData.ToString());
            }
        }
        else
        {
            Debug.LogError("게임 정보 조회에 실패했습니다. : " + bro);
        }
    }

    public void UpdateScoreToBackend(){
        CurrentScoreToUserData();  // 현재 스테이지의 하이스코어를 userData.score에 넣는다.
        GameDataUpdate(); // userData.score를 백엔드로 전송
    }

    public void CurrentScoreToUserData()
    {
        if (userData == null)
            userData = new UserData();

        int stageHighScore = 0;

        // GameManager에 저장된 현재 스테이지의 하이스코어를 사용
        if (GameManager.Instance != null && GameManager.Instance.GameData != null)
        {
            var stageScores = GameManager.Instance.GameData.stageHighScore;
            if (stageScores != null && stageScores.Count > 0)
            {
                int stageIndex = GameManager.Instance.currentStageNum;
                if (stageIndex < 0 || stageIndex >= stageScores.Count)
                    stageIndex = 0;

                stageHighScore = stageScores[stageIndex];
            }
        }

        userData.score = stageHighScore;
    }

    public void GameDataUpdate()
    {
        if (userData == null)
        {
            Debug.LogError("서버에서 다운받거나 새로 삽입한 데이터가 존재하지 않습니다. Insert 혹은 Get을 통해 데이터를 생성해주세요.");
            return;
        }

        Param param = new Param();
        param.Add("score", userData.score);
        BackendReturnObject bro = null;

        if (string.IsNullOrEmpty(gameDataRowInDate))
        {
            Debug.Log("내 제일 최신 게임 정보 데이터 수정을 요청합니다.");

            bro = Backend.GameData.Update("USER_DATA", new Where(), param);
        }
        else
        {
            Debug.Log($"{gameDataRowInDate}의 게임 정보 데이터 수정을 요청합니다.");

            bro = Backend.GameData.UpdateV2("USER_DATA", gameDataRowInDate, Backend.UserInDate, param);
        }

        if (bro.IsSuccess())
        {
            Debug.Log("게임 정보 데이터 수정에 성공했습니다. : " + bro);
        }
        else
        {
            Debug.LogError("게임 정보 데이터 수정에 실패했습니다. : " + bro);
        }
    }
}
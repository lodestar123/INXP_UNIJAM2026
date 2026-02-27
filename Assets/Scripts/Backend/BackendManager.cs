using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd; // 뒤끝 SDK namespace

public class BackendManager : MonoBehaviour
{
    void Start()
    {
        var bro = Backend.Initialize(); // 뒤끝 초기화

        // 뒤끝 초기화에 대한 응답값
        if (bro.IsSuccess())
        {
            Debug.Log("초기화 성공 : " + bro); // 성공일 경우 statusCode 204 Success
        }
        else
        {
            Debug.LogError("초기화 실패 : " + bro); // 실패일 경우 statusCode 400대 에러 발생
        }

        Test();
    }
    

    void Test()
    {
        //BackendLogin.Instance.CustomSignUp("user1","1234"); // 뒤끝 회원가입

        //BackendLogin.Instance.CustomLogin("user1", "1234"); // 로그인 테스트

        //BackendLogin.Instance.CustomSignUp("user3","1234"); // 뒤끝 회원가입
        BackendLogin.Instance.CustomLogin("user3", "1234"); // 로그인 테스트

        //BackendLogin.Instance.UpdateNickname("원하는 이름"); // 닉네임 변겅

        //BackendGameData.Instance.GameDataInsert(); // 게임 정보 삽입

        //BackendGameData.Instance.GameDataGet(); // 게임 정보 조회

        //BackendGameData.Instance.UpdateScoreToBackend(); // 최종 점수 전송

        //BackendRank.Instance.RankInsert(100); // 랭킹 삽입

        //BackendRank.Instance.RankGet(); // 랭킹 조회


        Debug.Log("테스트를 종료합니다.");
    }
}

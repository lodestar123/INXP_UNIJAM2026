using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd; // 뒤끝 SDK namespace

/// <summary>
/// 뒤끝 로그인 관리 클래스
/// </summary>
public class BackendLogin
{
    private static BackendLogin _instance = null;

    // 싱글톤 인스턴스 
    public static BackendLogin Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BackendLogin();
            }

            return _instance;
        }
    }

    public static bool IsLoggedIn()
    {
        return Backend.IsLogin;
    }

    // 회원가입 
    public void CustomSignUp(string id, string pw)
    {
        Debug.Log("회원가입을 요청합니다.");

        var bro = Backend.BMember.CustomSignUp(id, pw);

        if (bro.IsSuccess())
        {
            Debug.Log("회원가입에 성공했습니다. : " + bro);
        }
        else
        {
            Debug.LogError("회원가입에 실패했습니다. : " + bro);
        }
    }

    // 로그인 
    public bool CustomLogin(string id, string pw)
    {
        Debug.Log("로그인을 요청합니다.");

        var bro = Backend.BMember.CustomLogin(id, pw);

        if (bro.IsSuccess())
        {
            Debug.Log("로그인이 성공했습니다. : " + bro);
            return true;
        }

        Debug.LogError("로그인이 실패했습니다. : " + bro);
        return false;
    }

    /// <summary>
    /// 로그인된 계정의 닉네임을 변경합니다. 결과는 반환값으로 확인할 수 있습니다.
    /// </summary>
    public BackendReturnObject UpdateNickname(string nickname)
    {
        Debug.Log("닉네임 변경을 요청합니다.");

        var bro = Backend.BMember.UpdateNickname(nickname);

        if (bro.IsSuccess())
        {
            Debug.Log("닉네임 변경에 성공했습니다 : " + bro);
        }
        else
        {
            Debug.LogError("닉네임 변경에 실패했습니다 : " + bro);
        }

        return bro;
    }
}

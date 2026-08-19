#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 스테이지 해금 상태를 테스트용으로 일괄 변경하는 에디터 메뉴
/// </summary>
public static class StageUnlockEditorMenu
{
    private const string MenuRoot = "Tools/Game/";

    private static string SavePath => Path.Combine(Application.persistentDataPath, "savedata.json");

    [MenuItem(MenuRoot + "스테이지 전체 해금")]
    public static void UnlockAllStages()
    {
        if (Application.isPlaying && GameManager.Instance != null)
        {
            GameManager.Instance.UnlockAllStagesForTest();
            RefreshLobbyUnlockVisuals();
            EditorUtility.DisplayDialog("스테이지 해금", "모든 스테이지와 랭크모드를 해금했습니다.\n(저장됨)", "확인");
            return;
        }

        if (ApplyStageUnlockToSaveFile(unlockAll: true))
        {
            EditorUtility.DisplayDialog(
                "스테이지 해금",
                $"모든 스테이지와 랭크모드를 해금했습니다.\n\n저장 경로:\n{SavePath}",
                "확인");
        }
    }

    [MenuItem(MenuRoot + "스테이지 잠금 초기화 (1스테이지만 해금)")]
    public static void ResetStageUnlock()
    {
        if (Application.isPlaying && GameManager.Instance != null)
        {
            GameManager.Instance.ResetStageUnlockToDefault();
            RefreshLobbyUnlockVisuals();
            EditorUtility.DisplayDialog("스테이지 해금", "1스테이지만 해금된 초기 상태로 초기화됨\n(저장됨)", "확인");
            return;
        }

        if (ApplyStageUnlockToSaveFile(unlockAll: false))
        {
            EditorUtility.DisplayDialog(
                "스테이지 해금",
                $"1스테이지만 해금된 초기 상태로 초기화됨\n\n저장 경로:\n{SavePath}",
                "확인");
        }
    }

    private static bool ApplyStageUnlockToSaveFile(bool unlockAll)
    {
        GameData data;

        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                data = JsonUtility.FromJson<GameData>(json);
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("오류", $"저장 데이터 읽기 실패\n{e.Message}", "확인");
                return false;
            }
        }
        else
        {
            data = new GameData();
        }

        if (data.stageUnlocked == null)
        {
            data.stageUnlocked = new List<bool>();
        }

        while (data.stageUnlocked.Count < GameData.StageCount)
        {
            data.stageUnlocked.Add(false);
        }

        for (int i = 0; i < GameData.StageCount; i++)
        {
            data.stageUnlocked[i] = unlockAll || i == 0;
        }

        data.rankModeUnlocked = unlockAll;

        try
        {
            string outJson = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, outJson);
            Debug.Log(unlockAll
                ? "[StageUnlockEditor] savedata.json — 모든 스테이지/랭크모드 해금"
                : "[StageUnlockEditor] savedata.json — 1스테이지만 해금");
            return true;
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("오류", $"저장 데이터 쓰기 실패\n{e.Message}", "확인");
            return false;
        }
    }

    private static void RefreshLobbyUnlockVisuals()
    {
        LobbyManager lobby = UnityEngine.Object.FindFirstObjectByType<LobbyManager>();
        lobby?.RefreshRankModeButton();
    }
}
#endif

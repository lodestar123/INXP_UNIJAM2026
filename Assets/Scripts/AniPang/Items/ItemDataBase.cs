using UnityEngine;

public class ItemDataBase
{
    private static Item[] _items;
    private static int _loadedStageIndex = int.MinValue;

    public static Item[] Items
    {
        get
        {
            EnsureInitializedForCurrentStage();
            return _items;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        ReloadForCurrentStage();
    }

    public static void ReloadForCurrentStage()
    {
        _loadedStageIndex = GetCurrentStageIndex();
        string stagePath = GetStageResourcePath(_loadedStageIndex);

        _items = Resources.LoadAll<Item>(stagePath);

        if (_items == null || _items.Length == 0)
        {
            Debug.LogWarning($"[ItemDataBase] '{stagePath}'에서 아이템을 찾지 못했습니다. 전체 Items 폴더를 사용합니다.");
            _items = Resources.LoadAll<Item>("Items/");
        }

        if (_items == null)
        {
            _items = new Item[0];
        }
    }

    private static void EnsureInitializedForCurrentStage()
    {
        int currentStageIndex = GetCurrentStageIndex();
        if (_items == null || _loadedStageIndex != currentStageIndex)
        {
            ReloadForCurrentStage();
        }
    }

    private static int GetCurrentStageIndex()
    {
        if (GameManager.Instance == null)
        {
            return 0;
        }

        GameManager.Instance.EnsureValidCurrentStage();
        return Mathf.Max(0, GameManager.Instance.currentStageNum);
    }

    private static string GetStageResourcePath(int stageIndex)
    {
        return $"Items/Stage {stageIndex + 1}";
    }
}

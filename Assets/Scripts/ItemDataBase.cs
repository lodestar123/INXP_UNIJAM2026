using UnityEngine;

public class ItemDataBase : MonoBehaviour
{
    public static Item[] Items { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Items = Resources.LoadAll<Item>("Items/");

    }

}

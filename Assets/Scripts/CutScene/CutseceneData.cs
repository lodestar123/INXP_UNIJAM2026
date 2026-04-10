using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/CutsceneData")]
public class CutsceneData : ScriptableObject
{
    public string BGMName;
    public CutsceneFrame[] frames;
}

using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/CutsceneData")]
public class CutsceneData : ScriptableObject
{
    public SoundManager.BGM targetBGM;
    public CutsceneFrame[] frames;
}

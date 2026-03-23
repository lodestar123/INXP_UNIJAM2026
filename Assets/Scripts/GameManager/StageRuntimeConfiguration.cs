using System;
using UnityEngine;

[Serializable]
public class StageRuntimeConfiguration
{
    [Min(0)]
    public int stageIndex;

    [Header("Fixed Slot (Unused - Present is fixed in scene)")]
    public GameObject presentGamePrefab;
    public GameObject presentUIPrefab;
    public SoundManager.BGM presentBgm = SoundManager.BGM.Anipang;

    [Header("Variable Slot")]
    public GameObject pastGamePrefab;
    public GameObject pastUIPrefab;
    public SoundManager.BGM pastBgm = SoundManager.BGM.FlappyBird;
}

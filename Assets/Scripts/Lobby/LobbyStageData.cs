using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/LobbyStageData")]
public class LobbyStageData : ScriptableObject
{
    public int StageID;
    public string StageName; // 스테이지 이름 필요하면? 
    public Sprite StageThumbnail; // 스프라이트 (필요하면)
    public string normalStageCriteria;  // {}점을 달성하자!
    public string hardStageCriteria; // 달성 기준
    [TextArea] public string stageDescription; // 아래 설명

}

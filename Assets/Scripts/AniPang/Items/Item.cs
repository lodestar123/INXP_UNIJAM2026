using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Match-3/Item")]
public class Item : ScriptableObject
{
    public int value;
    public Sprite spritePast;
    public Sprite spritePresent;
    public Sprite spritePop;
}
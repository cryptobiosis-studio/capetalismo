using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum relicType{
    Life,
    Speed,
    Firerate,
    Damage,
    Invincibility
}
[CreateAssetMenu(fileName = "Relic", menuName = "Relic/New Relic", order = 1)]
public class Relic : ScriptableObject
{   
    public string relicName;
    [TextArea]public string description;
    public relicType relicType;
    public Sprite relicSprite;

}

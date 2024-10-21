using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum shootingStyles{
    Simple,
    Spread
}

[CreateAssetMenu(fileName = "Gun", menuName = "Gun/New Gun", order = 1)]
public class GunObj : ScriptableObject
{
    public string gunName;

    [TextArea]public string desciption;

    public float damage;

    public float firerate;

    public float bulletSpeed;

    public int numberOfBullets;

    public shootingStyles shootingStyle;
    public Sprite gunSprite;
    public Sprite bulletSprite;

}
